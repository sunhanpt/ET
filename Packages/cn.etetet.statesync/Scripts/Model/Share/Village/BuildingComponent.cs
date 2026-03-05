namespace ET
{
    /// <summary>
    /// 建筑组件，挂在 Unit(建筑) 上
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class BuildingComponent : Entity, IAwake<int>, IDestroy
    {
        /// <summary>对应 BuildingConfig.Id</summary>
        public int ConfigId { get; set; }
        /// <summary>建筑状态</summary>
        public BuildingState State { get; set; }
        /// <summary>当前工人数量</summary>
        public int CurrentWorkers { get; set; }

        public BuildingConfig Config() => BuildingConfigCategory.Instance.Get(this.ConfigId);
    }

    public enum BuildingState
    {
        UnderConstruction = 0,
        Built             = 1,
    }
}

