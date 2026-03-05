using Unity.Mathematics;

namespace ET.Client
{
    /// <summary>
    /// EnterMap 完成后，初始化村庄场景：生成资源节点、建筑、村民
    /// </summary>
    [Event(SceneType.Current)]
    public class EnterMapFinish_InitVillage : AEvent<Scene, EnterMapFinish>
    {
        protected override async ETTask Run(Scene scene, EnterMapFinish args)
        {
            // ── 1. 创建初始仓库建筑 ──────────────────────────────
            // 仓库不消耗资源（初始赠送），直接调工厂，跳过扣费
            await BuildingHelper.BuildFreeAsync(scene, BuildingType.Storehouse,
                new float3(0, 0, 0));

            // ── 2. 放置资源节点 ──────────────────────────────────
            // 木材节点
            VillageFactory.CreateResourceNode(scene, 1, new float3(5, 0, 3));
            VillageFactory.CreateResourceNode(scene, 1, new float3(7, 0, -2));
            VillageFactory.CreateResourceNode(scene, 1, new float3(9, 0, 5));
            // 石矿节点
            VillageFactory.CreateResourceNode(scene, 2, new float3(-5, 0, 4));
            VillageFactory.CreateResourceNode(scene, 2, new float3(-8, 0, -3));
            // 食物节点
            VillageFactory.CreateResourceNode(scene, 3, new float3(3, 0, -6));
            VillageFactory.CreateResourceNode(scene, 3, new float3(-3, 0, -7));

            // ── 3. 创建初始村民，并分配采集任务 ─────────────────
            Unit v1 = VillageFactory.CreateVillager(scene, new float3(-1, 0, 0));
            Unit v2 = VillageFactory.CreateVillager(scene, new float3(-2, 0, 1));
            Unit v3 = VillageFactory.CreateVillager(scene, new float3(-1.5f, 0, -1));

            // 给仓库加一些初始资源（方便测试建造）
            StorehouseComponent store = scene.GetComponent<StorehouseComponent>();
            store.Deposit(ResourceType.Wood, 5);
            store.Deposit(ResourceType.Stone, 3);

            // 分配采集任务：v1、v2 采木材，v3 采石矿
            v1.GetComponent<VillagerComponent>().AssignGatherTask(ResourceType.Wood);
            v2.GetComponent<VillagerComponent>().AssignGatherTask(ResourceType.Wood);
            v3.GetComponent<VillagerComponent>().AssignGatherTask(ResourceType.Stone);

            await ETTask.CompletedTask;
        }
    }
}

