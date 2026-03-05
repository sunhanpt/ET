namespace ET
{
    // 模拟经营游戏数值扩展
    public static partial class NumericType
    {
        // 采集速度加成（百分比基数10000）
        public const int GatherSpeed    = 2000;
        public const int GatherSpeedBase = GatherSpeed * 10 + 1;
        public const int GatherSpeedAdd  = GatherSpeed * 10 + 2;
        public const int GatherSpeedPct  = GatherSpeed * 10 + 3;

        // 携带上限
        public const int CarryCapacity     = 2001;
        public const int CarryCapacityBase = CarryCapacity * 10 + 1;
    }
}

