﻿﻿using System.Net;

namespace ET.Server
{
    [Invoke(SceneType.Map)]
    public class FiberInit_Map: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            // 模拟经营：玩家会话映射（无 Unit，通过 GateSession 转发消息）
            root.AddComponent<PlayerSessionMapComponent>();
            // 村庄数据（服务端权威数据源）
            root.AddComponent<VillageMapComponent>();

            await ETTask.CompletedTask;
        }
    }
}