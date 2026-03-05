namespace ET
{
    [FriendOf(typeof(VillageComponent))]
    [EntitySystemOf(typeof(VillageComponent))]
    public static partial class VillageComponentSystem
    {
        [EntitySystem]
        private static void Awake(this VillageComponent self)
        {
            self.ResourceNodeIds.Clear();
            self.BuildingIds.Clear();
            self.VillagerIds.Clear();
        }

        [EntitySystem]
        private static void Destroy(this VillageComponent self)
        {
        }

        public static void AddVillager(this VillageComponent self, long id)     => self.VillagerIds.Add(id);
        public static void AddBuilding(this VillageComponent self, long id)     => self.BuildingIds.Add(id);
        public static void AddResourceNode(this VillageComponent self, long id) => self.ResourceNodeIds.Add(id);
        public static void RemoveVillager(this VillageComponent self, long id)     => self.VillagerIds.Remove(id);
        public static void RemoveBuilding(this VillageComponent self, long id)     => self.BuildingIds.Remove(id);
        public static void RemoveResourceNode(this VillageComponent self, long id) => self.ResourceNodeIds.Remove(id);

        /// <summary>查找离指定位置最近的可用资源节点（指定类型）</summary>
        public static Unit FindNearestResource(this VillageComponent self, Unity.Mathematics.float3 pos, int resourceType)
        {
            UnitComponent unitComponent = self.Scene().GetComponent<UnitComponent>();
            Unit best = null;
            float bestDist = float.MaxValue;

            foreach (long id in self.ResourceNodeIds)
            {
                Unit node = unitComponent.Get(id);
                if (node == null) continue;
                ResourceNodeComponent res = node.GetComponent<ResourceNodeComponent>();
                if (res == null || !res.IsAvailable()) continue;
                if (res.Config().ResourceType != resourceType) continue;

                float dist = Unity.Mathematics.math.distancesq(pos, node.Position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = node;
                }
            }
            return best;
        }

        /// <summary>查找最近的仓库建筑</summary>
        public static Unit FindNearestStorehouse(this VillageComponent self, Unity.Mathematics.float3 pos)
        {
            UnitComponent unitComponent = self.Scene().GetComponent<UnitComponent>();
            Unit best = null;
            float bestDist = float.MaxValue;

            foreach (long id in self.BuildingIds)
            {
                Unit building = unitComponent.Get(id);
                if (building == null) continue;
                BuildingComponent bc = building.GetComponent<BuildingComponent>();
                if (bc == null || !bc.IsBuilt()) continue;
                if (bc.Config().BuildingType != BuildingType.Storehouse) continue;

                float dist = Unity.Mathematics.math.distancesq(pos, building.Position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = building;
                }
            }
            return best;
        }
    }
}

