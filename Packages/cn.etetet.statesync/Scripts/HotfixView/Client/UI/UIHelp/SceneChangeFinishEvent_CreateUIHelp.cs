namespace ET.Client
{
    [Event(SceneType.Village)]
    public class SceneChangeFinishEvent_CreateUIHelp : AEvent<Scene, SceneChangeFinish>
    {
        protected override async ETTask Run(Scene scene, SceneChangeFinish args)
        {
            await UIHelper.Create(scene, UIType.UIHelp, UILayer.Mid);
        }
    }
}
