using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 游戏内 建造UI
    /// </summary>
    [ComponentOf(typeof(UI))]
    public class UIBuildComponent : Entity, IAwake
    {
        public GameObject buildBackBtn;
        public GameObject ScrollView;
    }
}