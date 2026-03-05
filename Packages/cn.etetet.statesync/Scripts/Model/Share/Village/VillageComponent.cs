using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 挂在 Scene 上，管理场景内所有资源节点和建筑的 Unit 列表
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class VillageComponent : Entity, IAwake, IDestroy
    {
        // 资源节点 UnitId 列表
        public List<long> ResourceNodeIds = new();
        // 建筑 UnitId 列表
        public List<long> BuildingIds = new();
        // 村民 UnitId 列表
        public List<long> VillagerIds = new();
    }
}

