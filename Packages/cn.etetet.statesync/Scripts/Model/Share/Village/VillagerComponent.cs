namespace ET
{
    /// <summary>
    /// 村民 AI 状态组件，挂在 Unit(村民) 上
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class VillagerComponent : Entity, IAwake<int>, IDestroy
    {
        /// <summary>当前 AI 状态</summary>
        public VillagerState State { get; set; }
        /// <summary>当前目标资源节点 UnitId（0=无）</summary>
        public long TargetResourceUnitId { get; set; }
        /// <summary>当前正在采集的资源类型</summary>
        public int GatheringResourceType { get; set; }
        /// <summary>身上携带的资源类型（回仓时用）</summary>
        public int CarryResourceType { get; set; }
        /// <summary>身上携带的资源数量</summary>
        public int CarryAmount { get; set; }
        
        /// <summary> 配置ID </summary>
        public int ConfigId { get; set; }
    }

    public enum VillagerState
    {
        Idle       = 0, // 空闲
        WalkToRes  = 1, // 走向资源点
        Gathering  = 2, // 采集中
        WalkToStore= 3, // 走向仓库
        Storing    = 4, // 存储中
    }
}

