namespace ET.Server
{
    /// <summary>
    /// 模拟经营：玩家进入地图（无 Unit，直接记录会话并推送场景切换）
    /// </summary>
    [MessageHandler(SceneType.Map)]
    public class G2M_PlayerEnterHandler : MessageHandler<Scene, G2M_PlayerEnter, G2M_PlayerEnterResponse>
    {
        protected override async ETTask Run(Scene scene, G2M_PlayerEnter request, G2M_PlayerEnterResponse response)
        {
            // 记录 playerId -> GateSession ActorId 的映射，断线/推送时使用
            PlayerSessionMapComponent sessionMap = scene.GetComponent<PlayerSessionMapComponent>();
            sessionMap.Add(request.PlayerId, request.GateSessionActorId);

            // 通知客户端开始切换到本场景
            M2C_StartSceneChange m2CStartSceneChange = M2C_StartSceneChange.Create();
            m2CStartSceneChange.SceneInstanceId = scene.InstanceId;
            m2CStartSceneChange.SceneName       = scene.Name;
            scene.GetComponent<MessageSender>().Send(request.GateSessionActorId, m2CStartSceneChange);

            await ETTask.CompletedTask;
        }
    }
}

