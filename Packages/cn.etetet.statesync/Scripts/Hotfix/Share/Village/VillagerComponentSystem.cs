using System;
using Unity.Mathematics;

namespace ET
{
    [EntitySystemOf(typeof(VillagerComponent))]
    public static partial class VillagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this VillagerComponent self)
        {
            self.State = VillagerState.Idle;
            self.TargetResourceUnitId = 0;
            self.CarryResourceType = 0;
            self.CarryAmount = 0;
        }

        [EntitySystem]
        private static void Destroy(this VillagerComponent self)
        {
            // 释放占用的资源节点
            self.ReleaseCurrentNode();
        }

        private static void ReleaseCurrentNode(this VillagerComponent self)
        {
            if (self.TargetResourceUnitId == 0) return;
            UnitComponent unitComponent = self.Scene().GetComponent<UnitComponent>();
            Unit node = unitComponent?.Get(self.TargetResourceUnitId);
            node?.GetComponent<ResourceNodeComponent>()?.Release();
            self.TargetResourceUnitId = 0;
        }

        // ─────────────────────────────────────────────
        // 对外接口：指定资源类型，驱动整个 AI 循环
        // ─────────────────────────────────────────────
        public static void AssignGatherTask(this VillagerComponent self, int resourceType)
        {
            self.GatheringResourceType = resourceType;
            self.RunGatherLoopAsync().NoContext();
        }

        public static void StopTask(this VillagerComponent self)
        {
            self.GatheringResourceType = 0;
            self.ReleaseCurrentNode();
            self.State = VillagerState.Idle;
            Unit villager = self.GetParent<Unit>();
            villager.GetComponent<MoveComponent>()?.Stop(false);
        }

        // ─────────────────────────────────────────────
        // AI 主循环
        // ─────────────────────────────────────────────
        private static async ETTask RunGatherLoopAsync(this VillagerComponent self)
        {
            Unit villager = self.GetParent<Unit>();

            while (!self.IsDisposed && self.GatheringResourceType != 0)
            {
                try
                {
                    // 1. 找最近可用资源节点
                    VillageComponent village = self.Scene().GetComponent<VillageComponent>();
                    Unit resNode = village.FindNearestResource(villager.Position, self.GatheringResourceType);
                    if (resNode == null)
                    {
                        // 没有资源节点，等待一段时间后重试
                        await self.Scene().Root().GetComponent<TimerComponent>().WaitAsync(2000);
                        continue;
                    }

                    ResourceNodeComponent resComp = resNode.GetComponent<ResourceNodeComponent>();
                    if (!resComp.TryOccupy(villager.Id))
                        continue;

                    self.TargetResourceUnitId = resNode.Id;

                    // 2. 走向资源节点
                    self.State = VillagerState.WalkToRes;
                    ResourceConfig resCfg = resComp.Config();
                    float3 targetPos = resNode.Position + new float3(resCfg.WalkToOffset, 0, 0);
                    float speed = villager.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
                    await villager.GetComponent<MoveComponent>().MoveToAsync(
                        new System.Collections.Generic.List<float3> { villager.Position, targetPos }, speed);
                    if (self.IsDisposed) return;

                    // 3. 循环采集直到携带满 or 节点耗尽
                    int carryMax = villager.GetComponent<NumericComponent>().GetAsInt(NumericType.CarryCapacity);
                    if (carryMax <= 0) carryMax = 5; // 默认携带上限

                    self.State = VillagerState.Gathering;
                    self.CarryResourceType = resCfg.ResourceType;

                    // 通知 View 层播放采集动画
                    EventSystem.Instance.Publish(self.Scene(), new VillagerGatherStart()
                    {
                        VillagerUnitId = villager.Id,
                        GatherMotion = resCfg.GatherMotion
                    });

                    while (!self.IsDisposed && self.CarryAmount < carryMax && !resComp.IsExhausted)
                    {
                        // 等待一次采集时长
                        float gatherSpeedPct = villager.GetComponent<NumericComponent>().GetAsFloat(NumericType.GatherSpeed);
                        if (gatherSpeedPct < 0.0001f) gatherSpeedPct = 1f;
                        int waitMs = (int)(resCfg.GatherTime / gatherSpeedPct);

                        await self.Scene().Root().GetComponent<TimerComponent>().WaitAsync(waitMs);
                        if (self.IsDisposed) return;

                        int gathered = resComp.Gather();
                        self.CarryAmount += gathered;
                    }

                    // 4. 通知 View 层停止采集动画
                    EventSystem.Instance.Publish(self.Scene(), new VillagerGatherStop() { VillagerUnitId = villager.Id });

                    // 释放节点占用
                    resComp.Release();
                    self.TargetResourceUnitId = 0;

                    if (self.CarryAmount <= 0) continue;

                    // 5. 走向仓库
                    self.State = VillagerState.WalkToStore;
                    Unit storehouse = village.FindNearestStorehouse(villager.Position);
                    if (storehouse != null)
                    {
                        float3 storePos = storehouse.Position + new float3(1.5f, 0, 0);
                        float storeSpeed = villager.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
                        await villager.GetComponent<MoveComponent>().MoveToAsync(
                            new System.Collections.Generic.List<float3> { villager.Position, storePos }, storeSpeed);
                        if (self.IsDisposed) return;
                    }

                    // 6. 存入仓库
                    self.State = VillagerState.Storing;
                    StorehouseComponent store = self.Scene().GetComponent<StorehouseComponent>();
                    store.Deposit(self.CarryResourceType, self.CarryAmount);
                    self.CarryAmount = 0;
                    self.CarryResourceType = 0;

                    // 短暂停留模拟存储动作
                    await self.Scene().Root().GetComponent<TimerComponent>().WaitAsync(500);
                }
                catch (Exception e)
                {
                    if (self.IsDisposed) return;
                    Log.Error($"VillagerAI error: {e}");
                    await self.Scene().Root().GetComponent<TimerComponent>().WaitAsync(1000);
                }
            }

            self.State = VillagerState.Idle;
        }
    }
}

