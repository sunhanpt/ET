using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 资源节点创建后，加载并实例化对应 prefab
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterResourceNodeCreate_CreateView : AEvent<Scene, AfterResourceNodeCreate>
    {
        protected override async ETTask Run(Scene scene, AfterResourceNodeCreate args)
        {
            Unit node = args.NodeUnit;
            ResourceNodeComponent resComp = node.GetComponent<ResourceNodeComponent>();
            ResourceConfig cfg = resComp.Config();

            GameObject bundleGo = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(cfg.PrefabPath);
            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = Object.Instantiate(bundleGo, globalComponent.Unit, true);
            go.transform.position = node.Position;
            node.AddComponent<GameObjectComponent>().GameObject = go;
        }
    }

    /// <summary>
    /// 资源节点耗尽后隐藏 GameObject
    /// </summary>
    [Event(SceneType.Current)]
    public class ResourceNodeExhausted_HideView : AEvent<Scene, ResourceNodeExhausted>
    {
        protected override async ETTask Run(Scene scene, ResourceNodeExhausted args)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit node = unitComponent.Get(args.NodeUnitId);
            if (node == null)
            {
                await ETTask.CompletedTask;
                return;
            }
            GameObjectComponent goComp = node.GetComponent<GameObjectComponent>();
            if (goComp?.GameObject != null)
                goComp.GameObject.SetActive(false);

            await ETTask.CompletedTask;
        }
    }
}

