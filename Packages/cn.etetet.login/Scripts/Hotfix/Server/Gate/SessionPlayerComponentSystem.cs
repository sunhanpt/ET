﻿namespace ET.Server
{
    [EntitySystemOf(typeof(SessionPlayerComponent))]
    public static partial class SessionPlayerComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this SessionPlayerComponent self)
        {
            // 断线逻辑由各业务包通过 partial 方法扩展（见 statesync 包）
            OnSessionPlayerDestroy(self);
        }

        static partial void OnSessionPlayerDestroy(SessionPlayerComponent self);

        [EntitySystem]
        private static void Awake(this SessionPlayerComponent self)
        {
        }
    }
}

