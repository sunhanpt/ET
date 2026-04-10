using Unity.Mathematics;

namespace ET.Server
{
    /// <summary>
    /// MapServer 接收建造请求，执行权威建造逻辑并更新 VillageMapComponent
    /// </summary>
    [MessageHandler(SceneType.Map)]
    public class G2M_BuildHandler : MessageHandler<Scene, G2M_Build, G2M_BuildResponse>
    {
        protected override async ETTask Run(Scene scene, G2M_Build request, G2M_BuildResponse response)
        {
            VillageMapComponent mapComp = scene.GetComponent<VillageMapComponent>();
            if (mapComp == null)
            {
                response.Error   = ErrorCode.ERR_Error;
                response.Message = "VillageMapComponent not found";
                return;
            }

            BuildingConfig cfg = BuildingConfigCategory.Instance.Get(request.BuildingConfigId);

            // 检查资源是否足够
            if (!mapComp.CanAfford(cfg))
            {
                response.Error   = ErrorCode.ERR_Error;
                response.Message = $"资源不足，无法建造: {cfg.Name}";
                return;
            }

            // 扣除资源
            mapComp.ConsumeStock(ResourceType.Wood,  cfg.CostWood);
            mapComp.ConsumeStock(ResourceType.Stone, cfg.CostStone);
            mapComp.ConsumeStock(ResourceType.Food,  cfg.CostFood);

            // 记录建筑（初始为建造中状态，BuildTime 后变为已建成）
            float3 pos = new float3(request.X, request.Y, request.Z);
            mapComp.AddBuilding(request.BuildingConfigId, pos, (int)BuildingState.UnderConstruction);

            // 异步等待建造时间后改变状态（fire-and-forget）
            mapComp.ScheduleBuildComplete(request.BuildingConfigId, pos, cfg.BuildTime).NoContext();

            await ETTask.CompletedTask;
        }
    }
}
