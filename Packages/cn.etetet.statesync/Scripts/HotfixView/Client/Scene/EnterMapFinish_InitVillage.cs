using Unity.Mathematics;

namespace ET.Client
{
    /// <summary>
    /// EnterMap 完成后，向服务端请求村庄快照，根据网络数据重建客户端村庄
    /// </summary>
    [Event(SceneType.Current)]
    public class EnterMapFinish_InitVillage : AEvent<Scene, EnterMapFinish>
    {
        protected override async ETTask Run(Scene scene, EnterMapFinish args)
        {
            // ── 1. 向服务端请求村庄快照 ─────────────────────────
            M2C_EnterVillage response = await scene.Root().GetComponent<ClientSenderComponent>()
                    .Call(C2M_EnterVillage.Create()) as M2C_EnterVillage;

            if (response == null || response.Error != 0)
            {
                Log.Error($"EnterVillage failed: {response?.Message}");
                return;
            }

            // ── 2. 初始化仓库存量 ─────────────────────────────
            StorehouseComponent store = scene.GetComponent<StorehouseComponent>();
            store.SetCapacity(response.StorageCapacity);
            foreach (var stock in response.Stocks)
            {
                store.RestoreFromSnapshot(stock.ResourceType, stock.Amount);
            }

            // ── 3. 创建资源节点 ───────────────────────────────
            foreach (var info in response.ResourceNodes)
            {
                VillageFactory.CreateResourceNode(scene, info.ConfigId,
                    new float3(info.Position.x, info.Position.y, info.Position.z), info.CurrentAmount);
            }

            // ── 4. 创建建筑 ───────────────────────────────────
            foreach (var info in response.Buildings)
            {
                await BuildingHelper.RestoreAsync(scene, info.ConfigId,
                    new float3(info.Position.x, info.Position.y, info.Position.z),
                    (BuildingState)info.State);
            }

            // ── 5. 创建村民并分配任务 ─────────────────────────
            foreach (var info in response.Villagers)
            {
                Unit villager = VillageFactory.CreateVillager(scene,
                    new float3(info.Position.x, info.Position.y, info.Position.z));

                if (info.GatheringResourceType != 0)
                {
                    villager.GetComponent<VillagerComponent>()
                        .AssignGatherTask(info.GatheringResourceType);
                }
            }
        }
    }
}

