﻿namespace ET
{
    [FriendOf(typeof(StorehouseComponent))]
    [EntitySystemOf(typeof(StorehouseComponent))]
    public static partial class StorehouseComponentSystem
    {
        [EntitySystem]
        private static void Awake(this StorehouseComponent self)
        {
            self.Resources.Clear();
            self.Capacity = 100;
        }

        [EntitySystem]
        private static void Destroy(this StorehouseComponent self)
        {
            self.Resources.Clear();
        }

        /// <summary>尝试存入资源，返回实际存入数量</summary>
        public static int Deposit(this StorehouseComponent self, int resourceType, int amount)
        {
            int current = self.GetAmount(resourceType);
            int total = self.TotalAmount();
            int canStore = self.Capacity - total;
            int actualStore = System.Math.Min(amount, canStore);
            if (actualStore <= 0) return 0;

            self.Resources[resourceType] = current + actualStore;

            EventSystem.Instance.Publish(self.Scene(), new ResourceChanged()
            {
                ResourceType = resourceType,
                NewAmount = self.Resources[resourceType]
            });
            return actualStore;
        }

        /// <summary>尝试消耗资源，返回是否成功</summary>
        public static bool Consume(this StorehouseComponent self, int resourceType, int amount)
        {
            int current = self.GetAmount(resourceType);
            if (current < amount) return false;
            self.Resources[resourceType] = current - amount;

            EventSystem.Instance.Publish(self.Scene(), new ResourceChanged()
            {
                ResourceType = resourceType,
                NewAmount = self.Resources[resourceType]
            });
            return true;
        }

        public static int GetAmount(this StorehouseComponent self, int resourceType)
        {
            self.Resources.TryGetValue(resourceType, out int v);
            return v;
        }

        public static int TotalAmount(this StorehouseComponent self)
        {
            int total = 0;
            foreach (var kv in self.Resources) total += kv.Value;
            return total;
        }

        /// <summary>是否有足够资源建造</summary>
        public static bool CanAfford(this StorehouseComponent self, BuildingConfig cfg)
        {
            return self.GetAmount(ResourceType.Wood) >= cfg.CostWood
                && self.GetAmount(ResourceType.Stone) >= cfg.CostStone
                && self.GetAmount(ResourceType.Food) >= cfg.CostFood;
        }

        /// <summary>从服务端快照恢复仓库数据（初始化专用，不触发 ResourceChanged 事件）</summary>
        public static void RestoreFromSnapshot(this StorehouseComponent self, int resourceType, int amount)
        {
            self.Resources[resourceType] = amount;
        }

        /// <summary>设置仓库容量上限</summary>
        public static void SetCapacity(this StorehouseComponent self, int capacity)
        {
            self.Capacity = capacity;
        }
    }
}

