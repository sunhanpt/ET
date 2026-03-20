namespace ET.Client
{
    public static partial class SceneChangeHelper
    {
        // 场景切换协程（模拟经营版：无 Player Unit，收到 M2C_StartSceneChange 即切换）
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的
            Scene currentScene = SceneFactory.Create(sceneInstanceId, sceneName, SceneType.Village, currentScenesComponent);
            currentScenesComponent.Scene = currentScene;
            // 等待场景资源加载完成（SceneChangeStart 订阅者内有异步加载逻辑）
            await EventSystem.Instance.PublishAsync(root, new SceneChangeStart());
            // 场景加载完毕，通知订阅者
            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());
            // 通知等待场景切换的协程
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
        }
    }
}

