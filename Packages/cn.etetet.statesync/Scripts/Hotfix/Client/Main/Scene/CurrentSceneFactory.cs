namespace ET.Client
{
    public static class SceneFactory
    {
        public static Scene Create(long id, string name, int sceneType, CurrentScenesComponent currentScenesComponent)
        {
            Scene currentScene = EntitySceneFactory.CreateScene(currentScenesComponent, id, IdGenerater.Instance.GenerateInstanceId(), sceneType, name);
            currentScenesComponent.Scene = currentScene;
            
            EventSystem.Instance.Publish(currentScene, new AfterCreateCurrentScene());
            return currentScene;
        }
    }
}