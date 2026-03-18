using Unity.Mathematics;

namespace ET
{
    public static class VillageFactory
    {
        /// <summary>创建村民 Unit</summary>
        public static Unit CreateVillager(Scene scene, int villagerConfId, float3 position)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            VillageComponent village    = scene.GetComponent<VillageComponent>();

            Unit villager = unitComponent.AddChild<Unit, int>(villagerConfId);
            villager.Position = position;

            NumericComponent numeric = villager.AddComponent<NumericComponent>();
            numeric.Set(NumericType.Speed,             6f);
            numeric.Set(NumericType.GatherSpeedBase,   10000L);
            numeric.Set(NumericType.CarryCapacityBase, 5);

            villager.AddComponent<MoveComponent>();
            villager.AddComponent<ObjectWait>();
            villager.AddComponent<VillagerComponent,int>(villagerConfId);
            
            village.AddVillager(villager.Id);

            EventSystem.Instance.Publish(scene, new AfterVillagerCreate() { VillagerUnit = villager });
            return villager;
        }

        /// <summary>创建资源节点 Unit（网络恢复时指定当前剩余量）</summary>
        public static Unit CreateResourceNode(Scene scene, int resourceConfigId, float3 position, int currentAmount = -1)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            VillageComponent village    = scene.GetComponent<VillageComponent>();

            Unit node = unitComponent.AddChild<Unit, int>(resourceConfigId);
            node.Position = position;
            var resComp = node.AddComponent<ResourceNodeComponent, int>(resourceConfigId);
            // 覆盖初始储量（-1 保持 Awake 里读 Config.MaxAmount 的默认值）
            if (currentAmount >= 0)
            {
                resComp.CurrentAmount = currentAmount;
            }
            
            village.AddResourceNode(node.Id);

            EventSystem.Instance.Publish(scene, new AfterResourceNodeCreate() { NodeUnit = node });
            return node;
        }
    }

    public struct AfterVillagerCreate
    {
        public Unit VillagerUnit;
    }

    public struct AfterResourceNodeCreate
    {
        public Unit NodeUnit;
    }
}

