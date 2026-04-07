using System;
using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.Village)]
    public class SceneChangeStart_LoadScene: AEvent<Scene, SceneChangeStart>
    {
        protected override async ETTask Run(Scene root, SceneChangeStart args)
        {
            try
            {
                Scene currentScene = root.CurrentScene();

                ResourcesLoaderComponent resourcesLoaderComponent = root.GetComponent<ResourcesLoaderComponent>();
            
                // 加载场景资源
                await resourcesLoaderComponent.LoadSceneAsync($"Packages/cn.etetet.gameres/Scenes/{currentScene.Name}.unity", LoadSceneMode.Single);
                
                
                // 切换到map场景

                //await SceneManager.LoadSceneAsync(currentScene.Name);

                currentScene.AddComponent<OperaComponent>();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        }
    }
}