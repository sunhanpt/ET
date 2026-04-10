namespace ET.Server
{
    /// <summary>
    /// 客户端发送建造请求 → Gate 转发给 MapServer
    /// </summary>
    [MessageSessionHandler(SceneType.Gate)]
    public class C2M_BuildHandler : MessageSessionHandler<C2M_Build, M2C_BuildResponse>
    {
        protected override async ETTask Run(Session session, C2M_Build request, M2C_BuildResponse response)
        {
            Player player = session.GetComponent<SessionPlayerComponent>().Player;
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.Zone(), "Village");

            G2M_Build buildReq = G2M_Build.Create();
            buildReq.PlayerId        = player.Id;
            buildReq.BuildingConfigId = request.BuildingConfigId;
            buildReq.X               = request.X;
            buildReq.Y               = request.Y;
            buildReq.Z               = request.Z;

            G2M_BuildResponse buildResp = (G2M_BuildResponse)await session.Fiber().Root
                .GetComponent<MessageSender>().Call(startSceneConfig.ActorId, buildReq);

            response.Error   = buildResp.Error;
            response.Message = buildResp.Message;
        }
    }
}
