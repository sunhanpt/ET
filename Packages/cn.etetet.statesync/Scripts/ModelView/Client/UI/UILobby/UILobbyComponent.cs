
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILobbyComponent : Entity, IAwake
	{
		public GameObject continueGame;
		public GameObject newGame;
		public GameObject quitGame;
		public Text text;
	}
}
