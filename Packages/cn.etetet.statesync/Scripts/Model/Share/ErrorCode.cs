namespace ET
{
    public static partial class ErrorCode
    {
        // StateSync 包错误码（不抛异常，调用方自行判断）
        public const int ERR_VillageMapComponentNotFound  = ERR_WithoutException + PackageType.StateSync * 1000 + 1;
        public const int ERR_VillageBuildResourceNotEnough = ERR_WithoutException + PackageType.StateSync * 1000 + 2;
    }
}

