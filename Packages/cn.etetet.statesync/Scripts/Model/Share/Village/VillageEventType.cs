namespace ET
{
    /// <summary>资源数量变化（存入/消耗后广播）</summary>
    public struct ResourceChanged
    {
        public int ResourceType;
        public int NewAmount;
    }

    /// <summary>资源节点耗尽</summary>
    public struct ResourceNodeExhausted
    {
        public long NodeUnitId;
    }

    /// <summary>村民开始采集（通知View播放动画）</summary>
    public struct VillagerGatherStart
    {
        public long VillagerUnitId;
        /// <summary>动画名称，来自 ResourceConfig.GatherMotion</summary>
        public string GatherMotion;
    }

    /// <summary>村民停止采集</summary>
    public struct VillagerGatherStop
    {
        public long VillagerUnitId;
    }

    /// <summary>建筑建造完成</summary>
    public struct BuildingFinished
    {
        public long BuildingUnitId;
    }
}

