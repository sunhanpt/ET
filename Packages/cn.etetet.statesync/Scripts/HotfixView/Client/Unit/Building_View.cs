using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 建筑 Unit 创建后加载 prefab（建造中状态）
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterBuildingCreate_CreateView : AEvent<Scene, AfterBuildingCreate>
    {
        protected override async ETTask Run(Scene scene, AfterBuildingCreate args)
        {
            Unit building = args.BuildingUnit;
            BuildingComponent bc = building.GetComponent<BuildingComponent>();
            BuildingConfig cfg = bc.Config();

            GameObject bundleGo = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(cfg.PrefabPath);
            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(bundleGo, globalComponent.Unit, true);
            go.transform.position = building.Position;
            building.AddComponent<GameObjectComponent>().GameObject = go;

            // 建造中：半透明或使用特殊材质，此处用缩放动画演示
            go.transform.localScale = Vector3.one * 0.1f;
        }
    }

    /// <summary>
    /// 建筑完成后恢复正常显示
    /// </summary>
    [Event(SceneType.Current)]
    public class BuildingFinished_UpdateView : AEvent<Scene, BuildingFinished>
    {
        protected override async ETTask Run(Scene scene, BuildingFinished args)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit building = unitComponent.Get(args.BuildingUnitId);
            if (building == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            GameObjectComponent goComp = building.GetComponent<GameObjectComponent>();
            if (goComp?.GameObject != null)
                goComp.GameObject.transform.localScale = Vector3.one;

            await ETTask.CompletedTask;
        }
    }
}

