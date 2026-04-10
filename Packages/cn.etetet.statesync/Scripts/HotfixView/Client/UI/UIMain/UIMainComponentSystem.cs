using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIMainComponent))]
    [FriendOf(typeof(UIMainComponent))]
    [FriendOf(typeof(StorehouseComponent))]
    [FriendOf(typeof(VillageComponent))]
    [FriendOf(typeof(PopulationComponent))]
    public static partial class UIMainComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIMainComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            self.woodText        = rc.Get<GameObject>("WoodText").GetComponent<Text>();
            self.stoneText       = rc.Get<GameObject>("StoneText").GetComponent<Text>();
            self.foodText        = rc.Get<GameObject>("FoodText").GetComponent<Text>();
            self.populationText  = rc.Get<GameObject>("PopulationText").GetComponent<Text>();

            self.buildStorehouseBtn = rc.Get<GameObject>("BuildStorehouseBtn");
            self.buildLumbermillBtn = rc.Get<GameObject>("BuildLumbermillBtn");
            self.buildQuarryBtn     = rc.Get<GameObject>("BuildQuarryBtn");
            self.buildFarmBtn       = rc.Get<GameObject>("BuildFarmBtn");
            self.buildHouseBtn      = rc.Get<GameObject>("BuildHouseBtn");

            // 绑定建造按钮
            self.buildStorehouseBtn.GetComponent<Button>().onClick.AddListener(() => { self.OnBuildBtnClick(BuildingType.Storehouse).NoContext(); });
            self.buildLumbermillBtn.GetComponent<Button>().onClick.AddListener(() => { self.OnBuildBtnClick(BuildingType.Lumbermill).NoContext(); });
            self.buildQuarryBtn.GetComponent<Button>().onClick.AddListener(    () => { self.OnBuildBtnClick(BuildingType.Quarry).NoContext(); });
            self.buildFarmBtn.GetComponent<Button>().onClick.AddListener(      () => { self.OnBuildBtnClick(BuildingType.Farm).NoContext(); });
            self.buildHouseBtn.GetComponent<Button>().onClick.AddListener(     () => { self.OnBuildBtnClick(BuildingType.House).NoContext(); });

            // 刷新初始显示
            self.RefreshResourceDisplay();
            self.RefreshPopulationDisplay();
        }

        [EntitySystem]
        private static void Destroy(this UIMainComponent self)
        {
        }

        /// <summary>刷新资源显示文本</summary>
        public static void RefreshResourceDisplay(this UIMainComponent self)
        {
            Scene scene = self.Root().CurrentScene();
            StorehouseComponent store = scene?.GetComponent<StorehouseComponent>();
            if (store == null) return;

            int wood  = store.GetAmount(ResourceType.Wood);
            int stone = store.GetAmount(ResourceType.Stone);
            int food  = store.GetAmount(ResourceType.Food);
            int cap   = store.Capacity;

            self.woodText.text  = $"木材: {wood}";
            self.stoneText.text = $"石矿: {stone}";
            self.foodText.text  = $"食物: {food} / {cap}";
        }

        /// <summary>刷新人口显示文本</summary>
        public static void RefreshPopulationDisplay(this UIMainComponent self)
        {
            Scene scene = self.Root().CurrentScene();
            VillageComponent village  = scene?.GetComponent<VillageComponent>();
            PopulationComponent pop   = scene?.GetComponent<PopulationComponent>();
            if (village == null) return;

            int current = village.VillagerIds.Count;
            int max     = pop != null ? pop.MaxPopulation : current;
            self.populationText.text = $"人口: {current} / {max}";
        }

        /// <summary>点击建造按钮，向服务端发送建造请求</summary>
        public static async ETTask OnBuildBtnClick(this UIMainComponent self, int buildingType)
        {
            // 从 BuildingConfig 中找到对应 buildingType 的首条配置
            BuildingConfig cfg = BuildingConfigCategory.Instance.GetByBuildingType(buildingType);
            if (cfg == null)
            {
                Log.Warning($"找不到 BuildingType={buildingType} 的配置");
                return;
            }

            Scene root = self.Root();
            var request = C2M_Build.Create();
            request.BuildingConfigId = cfg.Id;
            // 默认建造位置由服务端分配，客户端传入一个占位 0 坐标
            request.X = 0;
            request.Y = 0;
            request.Z = 0;

            M2C_BuildResponse response = await root.GetComponent<ClientSenderComponent>().Call(request) as M2C_BuildResponse;
            if (response == null || response.Error != 0)
            {
                Log.Warning($"建造失败: {response?.Message}");
                return;
            }

            Log.Info($"建造成功: {cfg.Name}");
        }
    }
}
