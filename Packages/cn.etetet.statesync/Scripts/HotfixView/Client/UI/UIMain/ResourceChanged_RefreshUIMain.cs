namespace ET.Client
{
    /// <summary>
    /// 监听仓库资源变化事件，实时刷新 UIMain 资源显示
    /// </summary>
    [Event(SceneType.Village)]
    public class ResourceChanged_RefreshUIMain : AEvent<Scene, ResourceChanged>
    {
        protected override async ETTask Run(Scene currentScene, ResourceChanged args)
        {
            await ETTask.CompletedTask;
            Scene root = currentScene.Root();
            UIMainComponent uiMain = root.GetComponent<UIComponent>()?.GetChild<UI>(UIType.UIMain)?.GetComponent<UIMainComponent>();
            uiMain?.RefreshResourceDisplay();
        }
    }
}
