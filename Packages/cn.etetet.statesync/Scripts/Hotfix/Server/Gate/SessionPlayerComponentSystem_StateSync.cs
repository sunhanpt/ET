namespace ET.Server
{
    /// <summary>
    /// 模拟经营扩展：玩家 Session 断线时通知 MapServer
    /// </summary>
    public static partial class SessionPlayerComponentSystem
    {
        static partial void OnSessionPlayerDestroy(SessionPlayerComponent self)
        {
            Scene root = self.Root();
            if (root.IsDisposed)
            {
                return;
            }

            StartSceneConfig mapConfig = StartSceneConfigCategory.Instance.GetBySceneName(root.Zone(), "Village");
            if (mapConfig == null)
            {
                return;
            }

            G2M_PlayerLeave leave = G2M_PlayerLeave.Create();
            leave.PlayerId = self.Player.Id;
            root.GetComponent<MessageSender>().Send(mapConfig.ActorId, leave);
        }
    }
}

