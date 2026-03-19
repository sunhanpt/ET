namespace ET.Server
{
    /// <summary>
    /// 模拟经营：玩家断线/离开，移除会话记录
    /// </summary>
    [MessageHandler(SceneType.Map)]
    public class G2M_PlayerLeaveHandler : MessageHandler<Scene, G2M_PlayerLeave>
    {
        protected override async ETTask Run(Scene scene, G2M_PlayerLeave message)
        {
            scene.GetComponent<PlayerSessionMapComponent>()?.Remove(message.PlayerId);
            await ETTask.CompletedTask;
        }
    }
}

