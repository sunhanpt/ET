namespace ET
{
    [EntitySystemOf(typeof(PopulationComponent))]
    [FriendOf(typeof(PopulationComponent))]
    [FriendOf(typeof(VillageComponent))]
    public static partial class PopulationComponentSystem
    {
        [EntitySystem]
        private static void Awake(this PopulationComponent self)
        {
            self.MaxPopulation = 5;
        }

        [EntitySystem]
        private static void Destroy(this PopulationComponent self)
        {
        }

        /// <summary>House 建成时，增加人口上限</summary>
        public static void AddHouseCapacity(this PopulationComponent self, int amount = 5)
        {
            self.MaxPopulation += amount;
            EventSystem.Instance.Publish(self.Scene(), new PopulationCapacityChanged()
            {
                NewMaxPopulation = self.MaxPopulation
            });
        }

        /// <summary>是否已达人口上限</summary>
        public static bool IsFull(this PopulationComponent self)
        {
            VillageComponent village = self.Scene().GetComponent<VillageComponent>();
            return village != null && village.VillagerIds.Count >= self.MaxPopulation;
        }
    }

    public struct PopulationCapacityChanged
    {
        public int NewMaxPopulation;
    }
}
