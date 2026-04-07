﻿using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 村民 Unit 创建后加载 prefab 并挂载动画组件
    /// </summary>
    [Event(SceneType.Village)]
    public class AfterVillagerCreate_CreateView : AEvent<Scene, AfterVillagerCreate>
    {
        protected override async ETTask Run(Scene scene, AfterVillagerCreate args)
        {
            Unit villager = args.VillagerUnit;
            var villagerComponent = villager.GetComponent<VillagerComponent>();
            var config = villagerComponent.ConfigId;
            var villagerConfig = VillagerConfigCategory.Instance.Get(config);
            string assetsName = villagerConfig.PrefabPath;
            GameObject bundleGameObject = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");

            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            go.transform.position = villager.Position;
            villager.AddComponent<GameObjectComponent>().GameObject = go;
            villager.AddComponent<AnimatorComponent>();
        }
    }
}

