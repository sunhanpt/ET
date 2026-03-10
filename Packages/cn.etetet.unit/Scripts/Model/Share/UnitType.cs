namespace ET
{
    public static partial class UnitType
    {
        public const int Player = PackageType.Unit * 1000 + 0;      // 主要控制玩家当前操作视野，操作中心点
        public const int Villager = PackageType.Unit * 1000 + 1;
        public const int Building = PackageType.Unit * 1000 + 2;
        public const int Monster = PackageType.Unit * 1000 + 3;
    }
}