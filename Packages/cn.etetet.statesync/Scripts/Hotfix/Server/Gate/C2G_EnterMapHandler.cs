namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_EnterMapHandler : MessageSessionHandler<C2G_EnterMap, G2C_EnterMap>
	{
		protected override async ETTask Run(Session session, C2G_EnterMap request, G2C_EnterMap response)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().Player;

			// 模拟经营：无需 Unit/GateMap，直接通知 MapServer 玩家进入
			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.Zone(), "Village");

			// PlayerSessionComponent 自身挂有 MailBoxComponent(GateSession)，是消息路由到客户端的 Actor
			PlayerSessionComponent playerSessionComponent = player.GetComponent<PlayerSessionComponent>();

			G2M_PlayerEnter enterMsg = G2M_PlayerEnter.Create();
			enterMsg.PlayerId           = player.Id;
			enterMsg.GateSessionActorId = playerSessionComponent.GetActorId();

			await session.Fiber().Root.GetComponent<MessageSender>().Call(startSceneConfig.ActorId, enterMsg);

			response.PlayerId = player.Id;
		}
	}
}

