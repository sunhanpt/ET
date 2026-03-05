using Unity.Mathematics;

namespace ET
{
    public static class VillageFactory
    {
        /// <summary>创建村民 Unit</summary>
        public static Unit CreateVillager(Scene scene, float3 position)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            VillageComponent village    = scene.GetComponent<VillageComponent>();

            Unit villager = unitComponent.AddChild<Unit, int>(2001);
            villager.Position = position;

            NumericComponent numeric = villager.AddComponent<NumericComponent>();
            numeric.Set(NumericType.Speed,             6f);
            numeric.Set(NumericType.GatherSpeedBase,   10000L);
            numeric.Set(NumericType.CarryCapacityBase, 5);

            villager.AddComponent<MoveComponent>();
            villager.AddComponent<ObjectWait>();
            villager.AddComponent<VillagerComponent>();

            unitComponent.Add(villager);
            village.AddVillager(villager.Id);

            EventSystem.Instance.Publish(scene, new AfterVillagerCreate() { VillagerUnit = villager });
            return villager;
        }

        /// <summary>创建资源节点 Unit</summary>
        public static Unit CreateResourceNode(Scene scene, int resourceConfigId, float3 position)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            VillageComponent village    = scene.GetComponent<VillageComponent>();

            Unit node = unitComponent.AddChild<Unit, int>(resourceConfigId);
            node.Position = position;
            node.AddComponent<ResourceNodeComponent, int>(resourceConfigId);

            unitComponent.Add(node);
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

