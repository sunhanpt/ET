﻿namespace ET.Server
{
    /// <summary>
    /// 模拟经营：客户端（经 Gate）请求村庄初始快照，Gate 侧处理，通过 Actor 向 MapServer 取数据
    /// </summary>
    [MessageSessionHandler(SceneType.Gate)]
    public class C2M_EnterVillageHandler : MessageSessionHandler<C2M_EnterVillage, M2C_EnterVillage>
    {
        protected override async ETTask Run(Session session, C2M_EnterVillage request, M2C_EnterVillage response)
        {
            Player player = session.GetComponent<SessionPlayerComponent>().Player;

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.Zone(), "Village");

            // 通过 Actor RPC 向 MapServer 请求快照数据
            G2M_GetVillageSnapshot snapshotReq = G2M_GetVillageSnapshot.Create();
            snapshotReq.PlayerId = player.Id;
            G2M_GetVillageSnapshotResponse snapshotResp =
                (G2M_GetVillageSnapshotResponse)await session.Fiber().Root
                    .GetComponent<MessageSender>().Call(startSceneConfig.ActorId, snapshotReq);

            if (snapshotResp.Error != 0)
            {
                response.Error   = snapshotResp.Error;
                response.Message = snapshotResp.Message;
                return;
            }

            response.StorageCapacity = snapshotResp.StorageCapacity;
            response.ResourceNodes.AddRange(snapshotResp.ResourceNodes);
            response.Buildings.AddRange(snapshotResp.Buildings);
            response.Villagers.AddRange(snapshotResp.Villagers);
            response.Stocks.AddRange(snapshotResp.Stocks);

            await ETTask.CompletedTask;
        }
    }
}
