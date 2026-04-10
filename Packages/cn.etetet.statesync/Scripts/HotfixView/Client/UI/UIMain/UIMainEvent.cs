using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UIMain)]
    public class UIMainEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            string assetsName = $"Packages/cn.etetet.gameres/Bundles/HotUpdate/System/Main/{UIType.UIMain}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UIMain, gameObject);
            ui.AddComponent<UIMainComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}
