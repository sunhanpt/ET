namespace ET.Server
{
    /// <summary>
    /// 模拟经营：Gate 请求村庄快照，Map 端直接返回 VillageMapComponent 数据
    /// </summary>
    [MessageHandler(SceneType.Map)]
    public class G2M_GetVillageSnapshotHandler : MessageHandler<Scene, G2M_GetVillageSnapshot, G2M_GetVillageSnapshotResponse>
    {
        protected override async ETTask Run(Scene scene, G2M_GetVillageSnapshot request, G2M_GetVillageSnapshotResponse response)
        {
            VillageMapComponent village = scene.GetComponent<VillageMapComponent>();
            if (village == null)
            {
                response.Error   = ErrorCode.ERR_WithoutException + 1;
                response.Message = "VillageMapComponent not found";
                return;
            }

            // 复用 M2C_EnterVillage 重载的辅助方法，通过临时对象中转
            M2C_EnterVillage temp = M2C_EnterVillage.Create();
            village.FillSnapshot(temp);
            response.StorageCapacity = temp.StorageCapacity;
            response.ResourceNodes.AddRange(temp.ResourceNodes);
            response.Buildings.AddRange(temp.Buildings);
            response.Villagers.AddRange(temp.Villagers);
            response.Stocks.AddRange(temp.Stocks);
            temp.Dispose();

            await ETTask.CompletedTask;
        }
    }
}

