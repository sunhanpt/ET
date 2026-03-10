using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 挂在 Map Scene(Root) 上，服务端持有的村庄数据
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class VillageMapComponent : Entity, IAwake, IDestroy
    {
        // ── 资源节点快照 ─────────────────────────────
        public List<VillageResNodeData> ResourceNodes = new();
        // ── 建筑快照 ─────────────────────────────────
        public List<VillageBuildingData> Buildings = new();
        // ── 村民快照 ─────────────────────────────────
        public List<VillagerData> Villagers = new();
        // ── 仓库资源存量 ──────────────────────────────
        public Dictionary<int, int> Stocks = new();
        public int StorageCapacity = 100;
    }

    public struct VillageResNodeData
    {
        public int ConfigId;
        public Unity.Mathematics.float3 Position;
        public int CurrentAmount;
    }

    public struct VillageBuildingData
    {
        public int ConfigId;
        public Unity.Mathematics.float3 Position;
        public int State; // BuildingState 枚举值
    }

    public struct VillagerData
    {
        public Unity.Mathematics.float3 Position;
        public int GatheringResourceType;
    }
}
