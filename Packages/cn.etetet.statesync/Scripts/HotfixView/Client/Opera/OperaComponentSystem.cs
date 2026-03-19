using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(OperaComponent))]
    public static partial class OperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this OperaComponent self)
        {
        }

        [EntitySystem]
        private static void Update(this OperaComponent self)
        {
            // 热重载（开发调试用）
            if (Input.GetKeyDown(KeyCode.R))
            {
                CodeLoader.Instance.Reload();
            }
        }
    }
}

