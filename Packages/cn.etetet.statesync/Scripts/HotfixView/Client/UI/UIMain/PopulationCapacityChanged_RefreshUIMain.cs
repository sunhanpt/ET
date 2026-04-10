namespace ET.Client
{
    /// <summary>
    /// 监听人口上限变化，刷新 UIMain 人口显示
    /// </summary>
    [Event(SceneType.Village)]
    public class PopulationCapacityChanged_RefreshUIMain : AEvent<Scene, PopulationCapacityChanged>
    {
        protected override async ETTask Run(Scene currentScene, PopulationCapacityChanged args)
        {
            await ETTask.CompletedTask;
            Scene root = currentScene.Root();
            UIMainComponent uiMain = root.GetComponent<UIComponent>()?.GetChild<UI>(UIType.UIMain)?.GetComponent<UIMainComponent>();
            uiMain?.RefreshPopulationDisplay();
        }
    }
}
