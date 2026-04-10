using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(StateSyncInner.M2A_Reload)]
    [ResponseType(nameof(A2M_Reload))]
    public partial class M2A_Reload : MessageObject, IRequest
    {
        public static M2A_Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2A_Reload>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(StateSyncInner.A2M_Reload)]
    public partial class A2M_Reload : MessageObject, IResponse
    {
        public static A2M_Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<A2M_Reload>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(StateSyncInner.G2M_PlayerEnter)]
    [ResponseType(nameof(G2M_PlayerEnterResponse))]
    public partial class G2M_PlayerEnter : MessageObject, IRequest
    {
        public static G2M_PlayerEnter Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2M_PlayerEnter>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long PlayerId { get; set; }

        [MemoryPackOrder(2)]
        public ActorId GateSessionActorId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PlayerId = default;
            this.GateSessionActorId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(StateSyncInner.G2M_PlayerEnterResponse)]
    public partial class G2M_PlayerEnterResponse : MessageObject, IResponse
    {
        public static G2M_PlayerEnterResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2M_PlayerEnterResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(StateSyncInner.G2M_PlayerLeave)]
    public partial class G2M_PlayerLeave : MessageObject, IMessage
    {
        public static G2M_PlayerLeave Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2M_PlayerLeave>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long PlayerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PlayerId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(StateSyncInner.G2M_GetVillageSnapshot)]
    [ResponseType(nameof(G2M_GetVillageSnapshotResponse))]
    public partial class G2M_GetVillageSnapshot : MessageObject, IRequest
    {
        public static G2M_GetVillageSnapshot Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2M_GetVillageSnapshot>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long PlayerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PlayerId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(StateSyncInner.G2M_GetVillageSnapshotResponse)]
    public partial class G2M_GetVillageSnapshotResponse : MessageObject, IResponse
    {
        public static G2M_GetVillageSnapshotResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2M_GetVillageSnapshotResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public List<VillageResourceInfo> ResourceNodes { get; set; } = new();

        [MemoryPackOrder(4)]
        public List<VillageBuildingInfo> Buildings { get; set; } = new();

        [MemoryPackOrder(5)]
        public List<VillagerInfo> Villagers { get; set; } = new();

        [MemoryPackOrder(6)]
        public List<VillageResourceStock> Stocks { get; set; } = new();

        [MemoryPackOrder(7)]
        public int StorageCapacity { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ResourceNodes.Clear();
            this.Buildings.Clear();
            this.Villagers.Clear();
            this.Stocks.Clear();
            this.StorageCapacity = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(StateSyncInner.G2M_Build)]
    [ResponseType(nameof(G2M_BuildResponse))]
    public partial class G2M_Build : MessageObject, IRequest
    {
        public static G2M_Build Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2M_Build>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long PlayerId { get; set; }

        [MemoryPackOrder(2)]
        public int BuildingConfigId { get; set; }

        [MemoryPackOrder(3)]
        public float X { get; set; }

        [MemoryPackOrder(4)]
        public float Y { get; set; }

        [MemoryPackOrder(5)]
        public float Z { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PlayerId = default;
            this.BuildingConfigId = default;
            this.X = default;
            this.Y = default;
            this.Z = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(StateSyncInner.G2M_BuildResponse)]
    public partial class G2M_BuildResponse : MessageObject, IResponse
    {
        public static G2M_BuildResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2M_BuildResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Recycle(this);
        }
    }

    public static class StateSyncInner
    {
        public const ushort M2A_Reload = 21002;
        public const ushort A2M_Reload = 21003;
        public const ushort G2M_PlayerEnter = 21004;
        public const ushort G2M_PlayerEnterResponse = 21005;
        public const ushort G2M_PlayerLeave = 21006;
        public const ushort G2M_GetVillageSnapshot = 21007;
        public const ushort G2M_GetVillageSnapshotResponse = 21008;
        public const ushort G2M_Build = 21009;
        public const ushort G2M_BuildResponse = 21010;
    }
}