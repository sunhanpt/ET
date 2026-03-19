using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 挂在 Map Scene 上，记录在线玩家 PlayerId -> GateSession ActorId 的映射
    /// 用于向客户端推送消息（无 Unit，通过 GateSession 转发）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class PlayerSessionMapComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<long, ActorId> PlayerSessions = new();
    }
}

