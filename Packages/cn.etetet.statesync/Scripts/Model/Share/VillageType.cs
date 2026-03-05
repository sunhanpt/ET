namespace ET
{
    /// <summary>
    /// 资源类型枚举，与 ResourceConfig.ResourceType 对应
    /// </summary>
    public static class ResourceType
    {
        public const int Wood  = 1; // 木材
        public const int Stone = 2; // 石矿
        public const int Food  = 3; // 食物
    }

    /// <summary>
    /// 建筑类型枚举，与 BuildingConfig.BuildingType 对应
    /// </summary>
    public static class BuildingType
    {
        public const int Storehouse = 1; // 仓库
        public const int Lumbermill = 2; // 伐木场
        public const int Quarry     = 3; // 采石场
        public const int Farm       = 4; // 农场
        public const int House      = 5; // 村民小屋
    }
}

