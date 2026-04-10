using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIBuildComponent))]
    public static partial class UIBuildComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIBuildComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.buildBackBtn = rc.Get<GameObject>("BackBtn");
            self.buildBackBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnBack(); });
            
            self.ScrollView = rc.Get<GameObject>("ScrollView");
        }

		
        public static void OnBack(this UIBuildComponent self)
        {
            EventSystem.Instance.PublishAsync(self.Root(), new BuildMenuClose()).NoContext();
        }
    }
}