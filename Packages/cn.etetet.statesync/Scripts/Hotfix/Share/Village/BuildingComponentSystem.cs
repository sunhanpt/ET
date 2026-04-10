namespace ET
{
    [FriendOf(typeof(StorehouseComponent))]
    [EntitySystemOf(typeof(BuildingComponent))]
    public static partial class BuildingComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BuildingComponent self, int configId)
        {
            self.ConfigId = configId;
            self.State = BuildingState.UnderConstruction;
            self.CurrentWorkers = 0;
        }

        [EntitySystem]
        private static void Destroy(this BuildingComponent self)
        {
        }
        
        
        public static BuildingConfig Config(this BuildingComponent self) => BuildingConfigCategory.Instance.Get(self.ConfigId);

        public static bool IsBuilt(this BuildingComponent self) => self.State == BuildingState.Built;

        public static bool CanAddWorker(this BuildingComponent self)
        {
            return self.IsBuilt() && self.CurrentWorkers < self.Config().MaxWorkers;
        }

        public static void AddWorker(this BuildingComponent self)
        {
            self.CurrentWorkers++;
        }

        public static void RemoveWorker(this BuildingComponent self)
        {
            if (self.CurrentWorkers > 0) self.CurrentWorkers--;
        }

        /// <summary>完成建造</summary>
        public static void FinishBuild(this BuildingComponent self)
        {
            self.State = BuildingState.Built;
            EventSystem.Instance.Publish(self.Scene(), new BuildingFinished() { BuildingUnitId = self.GetParent<Unit>().Id });
            // House 建成 → 增加人口上限
            self.ApplyHouseBonus();
        }

        /// <summary>
        /// 扩展仓库容量（仅仓库类建筑）
        /// </summary>
        public static void ApplyStorageBonus(this BuildingComponent self)
        {
            if (self.Config().BuildingType != BuildingType.Storehouse) return;
            var storehouse = self.Scene().GetComponent<StorehouseComponent>();
            if (storehouse != null)
                storehouse.Capacity += self.Config().StorageCapacity;
        }

        /// <summary>
        /// 增加人口上限（仅 House 类建筑）
        /// </summary>
        public static void ApplyHouseBonus(this BuildingComponent self)
        {
            if (self.Config().BuildingType != BuildingType.House) return;
            var pop = self.Scene().GetComponent<PopulationComponent>();
            pop?.AddHouseCapacity(5);
        }
    }
}

