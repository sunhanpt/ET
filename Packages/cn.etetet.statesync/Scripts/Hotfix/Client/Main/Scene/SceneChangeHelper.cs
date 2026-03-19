namespace ET.Client
{
    public static partial class SceneChangeHelper
    {
        // 场景切换协程（模拟经营版：无 Player Unit，收到 M2C_StartSceneChange 即切换）
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的
            Scene currentScene = CurrentSceneFactory.Create(sceneInstanceId, sceneName, currentScenesComponent);
            // 可以订阅这个事件中创建Loading界面
            EventSystem.Instance.Publish(root, new SceneChangeStart());
            // 等待 SceneChangeStart 事件订阅者（如加载场景资源）完成后通知完成
            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());
            // 通知等待场景切换的协程
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
            await ETTask.CompletedTask;
        }
    }
}

