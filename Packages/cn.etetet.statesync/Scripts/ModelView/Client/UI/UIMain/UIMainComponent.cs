using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 游戏内 HUD：显示资源存量、人口、建造按钮
    /// </summary>
    [ComponentOf(typeof(UI))]
    public class UIMainComponent : Entity, IAwake, IDestroy
    {
        // 资源文本
        public Text woodText;
        public Text stoneText;
        public Text foodText;
        // 人口文本
        public Text populationText;
        // 建造按钮（五种建筑类型）
        public GameObject buildStorehouseBtn;
        public GameObject buildLumbermillBtn;
        public GameObject buildQuarryBtn;
        public GameObject buildFarmBtn;
        public GameObject buildHouseBtn;
    }
}
