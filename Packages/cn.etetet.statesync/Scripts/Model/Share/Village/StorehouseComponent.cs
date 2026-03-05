using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 挂在 Scene 上，管理全局资源仓库存量
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class StorehouseComponent : Entity, IAwake, IDestroy
    {
        // resourceType -> 当前数量
        public Dictionary<int, int> Resources = new();
        // 总仓库容量上限
        public int Capacity = 100;
    }
}

