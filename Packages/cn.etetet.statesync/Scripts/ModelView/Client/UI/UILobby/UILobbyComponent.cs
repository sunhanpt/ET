
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILobbyComponent : Entity, IAwake
	{
		public GameObject continueGame;
		public GameObject NewGame;
		public GameObject QuitGame;
		public Text text;
	}
}
