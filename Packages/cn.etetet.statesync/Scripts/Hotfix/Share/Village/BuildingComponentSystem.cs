namespace ET
{
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
    }
}

