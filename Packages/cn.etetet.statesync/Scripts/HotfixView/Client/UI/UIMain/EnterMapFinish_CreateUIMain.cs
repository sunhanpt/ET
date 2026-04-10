namespace ET.Client
{
    /// <summary>
    /// 进入村庄地图完成后，自动打开游戏内 HUD
    /// </summary>
    [Event(SceneType.Village)]
    public class EnterMapFinish_CreateUIMain : AEvent<Scene, EnterMapFinish>
    {
        protected override async ETTask Run(Scene root, EnterMapFinish args)
        {
            await UIHelper.Create(root, UIType.UIMain, UILayer.Mid);
        }
    }
}
