namespace ET
{
    /// <summary>
    /// 资源节点耗尽后，等待再生时间（默认 30 秒）后自动复活
    /// </summary>
    [Event(SceneType.Village)]
    public class ResourceNodeExhausted_Respawn : AEvent<Scene, ResourceNodeExhausted>
    {
        // 再生等待时间（毫秒）
        private const int RespawnDelayMs = 30_000;

        protected override async ETTask Run(Scene scene, ResourceNodeExhausted args)
        {
            await scene.Root().GetComponent<TimerComponent>().WaitAsync(RespawnDelayMs);

            // 检查节点是否仍然存在
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit node = unitComponent?.Get(args.NodeUnitId);
            if (node == null || node.IsDisposed) return;

            ResourceNodeComponent resComp = node.GetComponent<ResourceNodeComponent>();
            if (resComp == null || resComp.IsDisposed) return;

            resComp.Respawn();
        }
    }
}
