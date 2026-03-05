namespace ET.Client
{
    [Event(SceneType.Current)]
    public class AfterCreateCurrentScene_AddComponent: AEvent<Scene, AfterCreateCurrentScene>
    {
        protected override async ETTask Run(Scene scene, AfterCreateCurrentScene args)
        {
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            // 村庄模拟经营组件
            scene.AddComponent<VillageComponent>();
            scene.AddComponent<StorehouseComponent>();
            await ETTask.CompletedTask;
        }
    }
}