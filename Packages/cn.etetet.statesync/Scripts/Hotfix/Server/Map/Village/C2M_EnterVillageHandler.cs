namespace ET.Server
{
    /// <summary>
    /// 客户端进入村庄后请求初始快照
    /// </summary>
    [MessageHandler(SceneType.Map)]
    public class C2M_EnterVillageHandler : MessageLocationHandler<Unit, C2M_EnterVillage, M2C_EnterVillage>
    {
        protected override async ETTask Run(Unit unit, C2M_EnterVillage request, M2C_EnterVillage response)
        {
            VillageMapComponent village = unit.Root().GetComponent<VillageMapComponent>();
            if (village == null)
            {
                response.Error = ErrorCode.ERR_WithoutException + 1;
                response.Message = "VillageMapComponent not found";
                return;
            }

            village.FillSnapshot(response);
            await ETTask.CompletedTask;
        }
    }
}

