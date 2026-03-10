namespace ET
{
    [EntitySystemOf(typeof(ResourceNodeComponent))]
    public static partial class ResourceNodeComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ResourceNodeComponent self, int configId)
        {
            self.ConfigId = configId;
            self.CurrentAmount = self.Config().MaxAmount;
            self.GathererUnitId = 0;
        }

        [EntitySystem]
        private static void Destroy(this ResourceNodeComponent self)
        {
            self.GathererUnitId = 0;
        }

        /// <summary>是否可被采集（未耗尽且无人占用）</summary>
        public static bool IsAvailable(this ResourceNodeComponent self)
        {
            return !self.IsExhausted && self.GathererUnitId == 0;
        }

        /// <summary>占用此资源节点，返回是否成功</summary>
        public static bool TryOccupy(this ResourceNodeComponent self, long unitId)
        {
            if (!self.IsAvailable()) return false;
            self.GathererUnitId = unitId;
            return true;
        }

        /// <summary>采集一次，返回实际采集量（0 = 已耗尽）</summary>
        public static int Gather(this ResourceNodeComponent self)
        {
            if (self.IsExhausted) return 0;
            ResourceConfig cfg = self.Config();
            int amount = System.Math.Min(cfg.GatherAmount, self.CurrentAmount);
            self.CurrentAmount -= amount;

            if (self.IsExhausted)
            {
                EventSystem.Instance.Publish(self.Scene(), new ResourceNodeExhausted() { NodeUnitId = self.GetParent<Unit>().Id });
            }
            return amount;
        }
        
        public static ResourceConfig Config(this ResourceNodeComponent self) => ResourceConfigCategory.Instance.Get(self.ConfigId);

        /// <summary>释放占用</summary>
        public static void Release(this ResourceNodeComponent self)
        {
            self.GathererUnitId = 0;
        }
    }
}

