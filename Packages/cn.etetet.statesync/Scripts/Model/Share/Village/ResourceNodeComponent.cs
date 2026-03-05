namespace ET
{
    /// <summary>
    /// 资源节点组件，挂在 Unit(资源节点) 上
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class ResourceNodeComponent : Entity, IAwake<int>, IDestroy
    {
        /// <summary>对应 ResourceConfig.Id</summary>
        public int ConfigId { get; set; }
        /// <summary>当前剩余储量</summary>
        public int CurrentAmount { get; set; }
        /// <summary>是否已耗尽</summary>
        public bool IsExhausted => CurrentAmount <= 0;
        /// <summary>当前正在采集此节点的村民Id（同时只允许一个）</summary>
        public long GathererUnitId { get; set; }

        public ResourceConfig Config() => ResourceConfigCategory.Instance.Get(this.ConfigId);
    }
}

