﻿using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(VillageMapComponent))]
    [FriendOf(typeof(VillageMapComponent))]
    public static partial class VillageMapComponentSystem
    {
        [EntitySystem]
        private static void Awake(this VillageMapComponent self)
        {
            self.ResourceNodes.Clear();
            self.Buildings.Clear();
            self.Villagers.Clear();
            self.Stocks.Clear();
            self.StorageCapacity = 100;

            // ── 默认初始村庄（可改为从 DB 加载）────────────────

            // 建筑：初始仓库（已建成）
            self.Buildings.Add(new VillageBuildingData
            {
                ConfigId = 1, // Storehouse ConfigId
                Position = new float3(0, 0, 0),
                State    = (int)BuildingState.Built,
            });

            // 资源节点：-1 表示满量，由客户端读 Config.MaxAmount
            self.ResourceNodes.Add(new VillageResNodeData { ConfigId = 1, Position = new float3(5,  0,  3),  CurrentAmount = -1 }); // Wood
            self.ResourceNodes.Add(new VillageResNodeData { ConfigId = 1, Position = new float3(7,  0, -2),  CurrentAmount = -1 }); // Wood
            self.ResourceNodes.Add(new VillageResNodeData { ConfigId = 1, Position = new float3(9,  0,  5),  CurrentAmount = -1 }); // Wood
            self.ResourceNodes.Add(new VillageResNodeData { ConfigId = 2, Position = new float3(-5, 0,  4),  CurrentAmount = -1 }); // Stone
            self.ResourceNodes.Add(new VillageResNodeData { ConfigId = 2, Position = new float3(-8, 0, -3),  CurrentAmount = -1 }); // Stone
            self.ResourceNodes.Add(new VillageResNodeData { ConfigId = 3, Position = new float3(3,  0, -6),  CurrentAmount = -1 }); // Food
            self.ResourceNodes.Add(new VillageResNodeData { ConfigId = 3, Position = new float3(-3, 0, -7),  CurrentAmount = -1 }); // Food

            // 村民
            self.Villagers.Add(new VillagerData { Position = new float3(-1,    0,  0),  GatheringResourceType = ResourceType.Wood  });
            self.Villagers.Add(new VillagerData { Position = new float3(-2,    0,  1),  GatheringResourceType = ResourceType.Wood  });
            self.Villagers.Add(new VillagerData { Position = new float3(-1.5f, 0, -1),  GatheringResourceType = ResourceType.Stone });

            // 初始仓库存量
            self.Stocks[ResourceType.Wood]  = 5;
            self.Stocks[ResourceType.Stone] = 3;
            self.Stocks[ResourceType.Food]  = 0;
        }

        [EntitySystem]
        private static void Destroy(this VillageMapComponent _)
        {
        }

        /// <summary>将当前村庄状态打包填入 M2C_EnterVillage</summary>
        public static void FillSnapshot(this VillageMapComponent self, M2C_EnterVillage response)
        {
            response.StorageCapacity = self.StorageCapacity;
            FillSnapshotCore(self, response.ResourceNodes, response.Buildings, response.Villagers, response.Stocks);
        }

        private static void FillSnapshotCore(
            VillageMapComponent self,
            System.Collections.Generic.List<VillageResourceInfo>  resourceNodes,
            System.Collections.Generic.List<VillageBuildingInfo>  buildings,
            System.Collections.Generic.List<VillagerInfo>         villagers,
            System.Collections.Generic.List<VillageResourceStock> stocks)
        {
            foreach (var r in self.ResourceNodes)
            {
                var info = VillageResourceInfo.Create();
                info.ConfigId      = r.ConfigId;
                info.Position      = r.Position;
                info.CurrentAmount = r.CurrentAmount >= 0
                    ? r.CurrentAmount
                    : ResourceConfigCategory.Instance.Get(r.ConfigId).MaxAmount;
                resourceNodes.Add(info);
            }

            foreach (var b in self.Buildings)
            {
                var info = VillageBuildingInfo.Create();
                info.ConfigId = b.ConfigId;
                info.Position = b.Position;
                info.State    = b.State;
                buildings.Add(info);
            }

            foreach (var v in self.Villagers)
            {
                var info = VillagerInfo.Create();
                info.Position              = v.Position;
                info.GatheringResourceType = v.GatheringResourceType;
                villagers.Add(info);
            }

            foreach (var kv in self.Stocks)
            {
                var stock = VillageResourceStock.Create();
                stock.ResourceType = kv.Key;
                stock.Amount       = kv.Value;
                stocks.Add(stock);
            }
        }

        // ── 服务端建造辅助方法 ─────────────────────────────────────

        public static bool CanAfford(this VillageMapComponent self, BuildingConfig cfg)
        {
            self.Stocks.TryGetValue(ResourceType.Wood,  out int wood);
            self.Stocks.TryGetValue(ResourceType.Stone, out int stone);
            self.Stocks.TryGetValue(ResourceType.Food,  out int food);
            return wood >= cfg.CostWood && stone >= cfg.CostStone && food >= cfg.CostFood;
        }

        public static void ConsumeStock(this VillageMapComponent self, int resourceType, int amount)
        {
            if (amount <= 0) return;
            self.Stocks.TryGetValue(resourceType, out int current);
            self.Stocks[resourceType] = System.Math.Max(0, current - amount);
        }

        public static void AddBuilding(this VillageMapComponent self, int configId, Unity.Mathematics.float3 pos, int state)
        {
            self.Buildings.Add(new VillageBuildingData
            {
                ConfigId = configId,
                Position = pos,
                State    = state,
            });
        }

        /// <summary>等待建造时间后将建筑状态更新为已建成</summary>
        public static async ETTask ScheduleBuildComplete(this VillageMapComponent self, int configId, Unity.Mathematics.float3 pos, int buildTimeMs)
        {
            await self.Scene().Root().GetComponent<TimerComponent>().WaitAsync(buildTimeMs);
            if (self.IsDisposed) return;

            for (int i = 0; i < self.Buildings.Count; i++)
            {
                var b = self.Buildings[i];
                if (b.ConfigId == configId && b.Position.Equals(pos) && b.State == (int)BuildingState.UnderConstruction)
                {
                    self.Buildings[i] = new VillageBuildingData
                    {
                        ConfigId = b.ConfigId,
                        Position = b.Position,
                        State    = (int)BuildingState.Built,
                    };
                    break;
                }
            }
        }
    }
}
