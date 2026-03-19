namespace ET.Server
{
    [EntitySystemOf(typeof(PlayerSessionMapComponent))]
    [FriendOf(typeof(PlayerSessionMapComponent))]
    public static partial class PlayerSessionMapComponentSystem
    {
        [EntitySystem]
        private static void Awake(this PlayerSessionMapComponent self)
        {
            self.PlayerSessions.Clear();
        }

        [EntitySystem]
        private static void Destroy(this PlayerSessionMapComponent self)
        {
            self.PlayerSessions.Clear();
        }

        public static void Add(this PlayerSessionMapComponent self, long playerId, ActorId gateSessionActorId)
        {
            self.PlayerSessions[playerId] = gateSessionActorId;
        }

        public static void Remove(this PlayerSessionMapComponent self, long playerId)
        {
            self.PlayerSessions.Remove(playerId);
        }

        /// <summary>向指定玩家推送消息（经由 GateSession 转发给客户端）</summary>
        public static void SendToPlayer(this PlayerSessionMapComponent self, long playerId, IMessage message)
        {
            if (!self.PlayerSessions.TryGetValue(playerId, out ActorId actorId))
            {
                return;
            }
            self.Root().GetComponent<MessageSender>().Send(actorId, message);
        }

        /// <summary>向所有在线玩家广播消息</summary>
        public static void Broadcast(this PlayerSessionMapComponent self, IMessage message)
        {
            (message as MessageObject).IsFromPool = false;
            MessageSender sender = self.Root().GetComponent<MessageSender>();
            foreach (ActorId actorId in self.PlayerSessions.Values)
            {
                sender.Send(actorId, message);
            }
        }
    }
}

