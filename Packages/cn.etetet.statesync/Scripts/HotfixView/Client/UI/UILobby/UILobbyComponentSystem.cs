using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UILobbyComponent))]
    public static partial class UILobbyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILobbyComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            self.continueGame = rc.Get<GameObject>("ContinueGame");
            self.NewGame = rc.Get<GameObject>("NewGame");
            self.QuitGame = rc.Get<GameObject>("QuitGame");
            self.continueGame.GetComponent<Button>().onClick.AddListener(() => { self.EnterMap().NoContext(); });
        }
        
        public static async ETTask EnterMap(this UILobbyComponent self)
        {
            Scene root = self.Root();
            await EnterMapHelper.EnterMapAsync(root);
            await UIHelper.Remove(root, UIType.UILobby);
        }
    }
}