namespace ET
{
    /// <summary>
    /// BuildingConfigCategory 扩展方法
    /// </summary>
    public static class BuildingConfigCategoryExtension
    {
        /// <summary>按 BuildingType 查找第一条配置（用于 UI 建造按钮）</summary>
        public static BuildingConfig GetByBuildingType(this BuildingConfigCategory self, int buildingType)
        {
            foreach (var kv in self.GetAll())
            {
                if (kv.Value.BuildingType == buildingType)
                    return kv.Value;
            }
            return null;
        }
    }
}
