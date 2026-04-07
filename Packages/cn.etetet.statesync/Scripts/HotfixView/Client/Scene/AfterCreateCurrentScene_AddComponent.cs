namespace ET.Client
{
    [Event(SceneType.Village)]
    public class AfterCreateCurrentScene_AddComponent: AEvent<Scene, AfterCreateCurrentScene>
    {
        protected override async ETTask Run(Scene scene, AfterCreateCurrentScene args)
        {
            scene.AddComponent<ResourcesLoaderComponent>();
            scene.AddComponent<UnitComponent>();
            // 村庄模拟经营组件
            scene.AddComponent<VillageComponent>();
            scene.AddComponent<StorehouseComponent>();
            await ETTask.CompletedTask;
        }
    }
}