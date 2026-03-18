using Unity.Mathematics;

namespace ET
{
    /// <summary>
    /// 建筑建造流程：扣资源 → 创建 Unit → 等待建造时间 → 完成
    /// </summary>
    public static class BuildingHelper
    {
        public static async ETTask BuildAsync(Scene scene, int buildingConfigId, float3 position)
        {
            StorehouseComponent storehouse = scene.GetComponent<StorehouseComponent>();
            BuildingConfig cfg = BuildingConfigCategory.Instance.Get(buildingConfigId);

            // 1. 检查资源是否足够
            if (!storehouse.CanAfford(cfg))
            {
                Log.Warning($"资源不足，无法建造: {cfg.Name}");
                return;
            }

            // 2. 扣除资源
            storehouse.Consume(ResourceType.Wood,  cfg.CostWood);
            storehouse.Consume(ResourceType.Stone, cfg.CostStone);
            storehouse.Consume(ResourceType.Food,  cfg.CostFood);

            await BuildFreeAsync(scene, buildingConfigId, position);
        }

        /// <summary>不扣资源直接建造（用于初始化）</summary>
        public static async ETTask BuildFreeAsync(Scene scene, int buildingConfigId, float3 position)
        {
            BuildingConfig cfg = BuildingConfigCategory.Instance.Get(buildingConfigId);

            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            VillageComponent village     = scene.GetComponent<VillageComponent>();

            Unit building = unitComponent.AddChild<Unit, int>(buildingConfigId);
            building.Position = position;
            building.AddComponent<BuildingComponent, int>(buildingConfigId);
            village.AddBuilding(building.Id);

            EventSystem.Instance.Publish(scene, new AfterBuildingCreate() { BuildingUnit = building });

            await scene.Root().GetComponent<TimerComponent>().WaitAsync(cfg.BuildTime);
            if (building.IsDisposed) return;

            BuildingComponent bc = building.GetComponent<BuildingComponent>();
            bc.FinishBuild();
            bc.ApplyStorageBonus();
        }

        /// <summary>
        /// 根据服务端快照直接恢复建筑（不等待建造时间，直接设置 State）
        /// </summary>
        public static ETTask RestoreAsync(Scene scene, int buildingConfigId, float3 position, BuildingState state)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            VillageComponent village    = scene.GetComponent<VillageComponent>();

            Unit building = unitComponent.AddChild<Unit, int>(buildingConfigId);
            building.Position = position;
            BuildingComponent bc = building.AddComponent<BuildingComponent, int>(buildingConfigId);
            village.AddBuilding(building.Id);

            // 直接应用服务端状态
            if (state == BuildingState.Built)
            {
                bc.State = BuildingState.Built;
                bc.ApplyStorageBonus();
                EventSystem.Instance.Publish(scene, new BuildingFinished() { BuildingUnitId = building.Id });
            }

            EventSystem.Instance.Publish(scene, new AfterBuildingCreate() { BuildingUnit = building });
            return ETTask.CompletedTask;
        }
    }

    public struct AfterBuildingCreate
    {
        public Unit BuildingUnit;
    }
}
