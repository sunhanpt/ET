# 模拟经营游戏 功能说明文档

> 基于 **ET 框架**（Fiber / Entity-Component 架构）  
> 项目分支：`Demo`  
> 最后更新：2026-04-09

---

## 目录

1. [项目架构概述](#1-项目架构概述)
2. [登录与大厅系统](#2-登录与大厅系统)
3. [资源系统](#3-资源系统)
4. [资源采集系统](#4-资源采集系统)
5. [村民 AI 系统](#5-村民-ai-系统)
6. [建筑系统](#6-建筑系统)
7. [仓库系统](#7-仓库系统)
8. [人口系统](#8-人口系统)
9. [建造指令系统（C/S）](#9-建造指令系统cs)
10. [资源节点再生系统](#10-资源节点再生系统)
11. [数值系统](#11-数值系统)
12. [配置表系统](#12-配置表系统)
13. [网络通信与快照同步](#13-网络通信与快照同步)
14. [UI 系统](#14-ui-系统)
15. [视图与动画系统](#15-视图与动画系统)
16. [服务端架构](#16-服务端架构)
17. [文件结构速查](#17-文件结构速查)

---

## 1. 项目架构概述

```
客户端                          服务端
┌────────────────────┐         ┌──────────────────────────────────────┐
│  Root Scene        │  TCP    │  Realm（登录鉴权）                    │
│  ├─ UIComponent    │◄───────►│  Gate（接入 / 消息转发）              │
│  └─ CurrentScene   │         │  Map（村庄权威逻辑 / Actor 消息）      │
│     ├─ VillageComponent      └──────────────────────────────────────┘
│     ├─ StorehouseComponent
│     ├─ PopulationComponent
│     └─ UnitComponent
└────────────────────┘
```

| 层次 | 说明 |
|---|---|
| **Model** | 纯数据定义（`Entity` + 字段），无逻辑 |
| **Hotfix** | 热更新逻辑，挂载 `[EntitySystem]` / `[Event]` |
| **ModelView** | 客户端 UI 组件定义（依赖 UnityEngine.UI） |
| **HotfixView** | 客户端视图逻辑，加载 Prefab、绑定动画 |
| **Server** | 服务端专属逻辑（Gate Handler、Map Handler） |

---

## 2. 登录与大厅系统

### 登录流程

```
AppStartInitFinish → 打开 UILogin
→ 用户输入账号密码点击登录
→ LoginHelper.Login → Realm 鉴权 → Gate 接入
→ LoginFinish 事件 → 打开 UILobby
```

### UILobby（大厅）

| 按钮 | 功能 |
|---|---|
| 继续游戏 | 调用 `EnterMapHelper.EnterMapAsync`，进入 Village 场景 |
| 新游戏 | （预留） |
| 退出游戏 | （预留） |

### 相关文件

- `Scripts/ModelView/Client/UI/UILogin/UILoginComponent.cs`
- `Scripts/HotfixView/Client/UI/UILogin/UILoginComponentSystem.cs`
- `Scripts/ModelView/Client/UI/UILobby/UILobbyComponent.cs`
- `Scripts/HotfixView/Client/UI/UILobby/UILobbyComponentSystem.cs`

---

## 3. 资源系统

### 资源类型（`ResourceType`）

| 值 | 名称 | 用途 |
|---|---|---|
| 1 | Wood（木材） | 建造建筑 |
| 2 | Stone（石矿） | 建造建筑 |
| 3 | Food（食物） | 建造建筑；（未来可作维持村民费用） |

### 数据来源

- **配置表**：`ResourceConfig`（Excel → `.bytes`）  
  字段：`Id`, `ResourceType`, `Name`, `GatherTime`, `GatherAmount`, `MaxAmount`, `PrefabPath`, `GatherMotion`, `WalkToOffset`

- **初始节点**（服务端 `VillageMapComponent.Awake`）：  
  木材 ×3、石矿 ×2、食物 ×2，分布在村庄周围固定坐标

---

## 4. 资源采集系统

### 采集节点组件（`ResourceNodeComponent`）

| 字段 | 说明 |
|---|---|
| `ConfigId` | 对应 ResourceConfig.Id |
| `CurrentAmount` | 当前剩余储量 |
| `IsExhausted` | 是否已耗尽（`CurrentAmount <= 0`） |
| `GathererUnitId` | 当前占用采集者 Id（同时只允许一人） |

### 关键方法

```csharp
bool TryOccupy(long unitId)  // 尝试占用，失败则跳过
int  Gather()                // 采集一次，返回采集量，耗尽时发布 ResourceNodeExhausted 事件
void Release()               // 释放占用
void Respawn()               // 复活：重置储量，发布 ResourceNodeRespawned 事件
```

---

## 5. 村民 AI 系统

### 村民状态机（`VillagerState`）

```
Idle ──┐
       ▼
  WalkToRes → Gathering → WalkToStore → Storing
       ▲___________________________________|
```

| 状态 | 说明 |
|---|---|
| `Idle` | 待机（未分配任务） |
| `WalkToRes` | 步行前往资源节点 |
| `Gathering` | 正在采集资源 |
| `WalkToStore` | 步行前往仓库 |
| `Storing` | 存入资源 |

### AI 主循环（`RunGatherLoopAsync`）

1. 调用 `VillageComponent.FindNearestResource` 找最近可用节点
2. 占用节点 → 步行过去（`MoveComponent.MoveToAsync`）
3. 循环调用 `ResourceNodeComponent.Gather`，直至携满或节点耗尽
4. 步行至最近仓库（`VillageComponent.FindNearestStorehouse`）
5. 调用 `StorehouseComponent.Deposit` 存入资源
6. 回到步骤 1，无限循环

### 村民数值（`NumericComponent`）

| NumericType | 默认值 | 说明 |
|---|---|---|
| `Speed` | 6 | 移动速度（单位/秒） |
| `GatherSpeedBase` | 10000 | 采集速度百分比基值（= 100%） |
| `CarryCapacityBase` | 5 | 最大携带量 |

---

## 6. 建筑系统

### 建筑类型（`BuildingType`）

| 值 | 名称 | 效果 |
|---|---|---|
| 1 | Storehouse（仓库） | 增加仓库存储上限（`StorageCapacity` 字段） |
| 2 | Lumbermill（伐木场） | （预留：加速木材采集） |
| 3 | Quarry（采石场） | （预留：加速石矿采集） |
| 4 | Farm（农场） | （预留：加速食物采集） |
| 5 | House（村民小屋） | 每建成一座增加人口上限 +5 |

### 建造流程

```
玩家点击建造按钮
→ C2M_Build 发往 Gate → G2M_Build 转发给 MapServer
→ MapServer CanAfford 检查 → 扣除资源
→ 记录 VillageBuildingData（UnderConstruction）
→ TimerComponent.WaitAsync(BuildTime) 异步等待
→ 建造完成：State → Built，发布 BuildingFinished 事件
```

### 建筑组件（`BuildingComponent`）

| 字段 | 说明 |
|---|---|
| `ConfigId` | 对应 BuildingConfig.Id |
| `State` | `UnderConstruction` / `Built` |
| `CurrentWorkers` | 当前工人数（预留） |

### 建造 Helper

| 方法 | 说明 |
|---|---|
| `BuildAsync` | 扣资源 + 等待建造时间（玩家主动建造） |
| `BuildFreeAsync` | 不扣资源直接建造（初始化用） |
| `RestoreAsync` | 从快照直接恢复建筑（不等待） |

---

## 7. 仓库系统

### 仓库组件（`StorehouseComponent`）

- 挂在 **Scene** 上，全局唯一
- `Dictionary<int, int> Resources`：resourceType → 当前数量
- `Capacity`：总容量上限（默认 100，每建一座仓库增加 `StorageCapacity`）

### 关键方法

```csharp
int  Deposit(int resourceType, int amount)     // 存入，返回实际存入量，发布 ResourceChanged 事件
bool Consume(int resourceType, int amount)     // 消耗，成功返回 true，发布 ResourceChanged 事件
int  GetAmount(int resourceType)               // 查询存量
bool CanAfford(BuildingConfig cfg)             // 判断建造资源是否充足
void RestoreFromSnapshot(int type, int amount) // 快照恢复（不触发事件）
void SetCapacity(int capacity)                 // 设置容量上限
```

---

## 8. 人口系统

### 人口组件（`PopulationComponent`）

- 挂在 **Scene** 上，全局唯一
- `MaxPopulation`：初始值 5，每建成一座 House +5

### 规则

- `VillageFactory.CreateVillager` 在创建前检查 `PopulationComponent.IsFull()`
- 若已达上限，输出警告日志并返回 `null`
- 快照恢复使用 `CreateVillagerForce`，**不受人口上限限制**
- House 建成时自动触发 `AddHouseCapacity(5)`，并发布 `PopulationCapacityChanged` 事件
- UIMain 监听 `PopulationCapacityChanged`，实时刷新人口显示

---

## 9. 建造指令系统（C/S）

### 消息流

```
[Client] C2M_Build { BuildingConfigId, X, Y, Z }
    ↓ Gate 转发
[Gate Handler] C2M_BuildHandler
    ↓ Actor RPC
[Map Handler] G2M_BuildHandler
    ├─ CanAfford 检查（读 VillageMapComponent.Stocks）
    ├─ ConsumeStock 扣除资源
    ├─ AddBuilding 记录建筑（UnderConstruction）
    └─ ScheduleBuildComplete（异步等待 BuildTime → 改为 Built）
    ↓
[Gate 返回] M2C_BuildResponse { Error, Message }
```

### 新增协议（`StateSyncOuter_C_11001.proto`）

```protobuf
message C2M_Build // ISessionRequest
{
    int32 RpcId = 1;
    int32 BuildingConfigId = 2;
    float X = 3;
    float Y = 4;
    float Z = 5;
}

message M2C_BuildResponse // ISessionResponse
{
    int32 RpcId = 1;
    int32 Error = 2;
    string Message = 3;
}
```

---

## 10. 资源节点再生系统

### 再生流程

```
ResourceNodeComponent.Gather()
→ CurrentAmount == 0 → 发布 ResourceNodeExhausted { NodeUnitId }
    ├─ [View] ResourceNodeExhausted_HideView → SetActive(false)
    └─ [Logic] ResourceNodeExhausted_Respawn
         → TimerComponent.WaitAsync(30_000ms)
         → ResourceNodeComponent.Respawn()
              → CurrentAmount = Config.MaxAmount
              → 发布 ResourceNodeRespawned { NodeUnitId }
                  └─ [View] ResourceNodeRespawned_ShowView → SetActive(true)
```

### 参数

| 参数 | 值 | 说明 |
|---|---|---|
| 再生等待时间 | 30,000 ms（30 秒） | 可在 `ResourceNodeExhausted_Respawn.RespawnDelayMs` 修改 |

---

## 11. 数值系统

### 五段式计算公式

```
final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100
```

### 村庄相关数值类型（`NumericType.Village.cs`）

| 枚举值 | ID | 说明 |
|---|---|---|
| `GatherSpeed` | 2000 | 采集速度加成（百分比） |
| `CarryCapacity` | 2001 | 携带上限 |

---

## 12. 配置表系统

### 读取方式

Excel → 导出 `.bytes` 二进制 → 运行时通过 `Singleton<XxxCategory>` 读取

### 村庄相关配置表

| 配置表 | 关键字段 |
|---|---|
| `BuildingConfig` | Id, BuildingType, Name, MaxWorkers, CostWood/Stone/Food, BuildTime, PrefabPath, StorageCapacity |
| `ResourceConfig` | Id, ResourceType, Name, GatherTime, GatherAmount, MaxAmount, PrefabPath, GatherMotion, WalkToOffset |
| `VillagerConfig` | Id, Name, Level, PrefabPath |

### 配置模式（`c` / `s` / `cs`）

| 模式 | 说明 |
|---|---|
| `c` | 仅客户端使用 |
| `s` | 仅服务端使用 |
| `cs` | 客户端和服务端共用 |

---

## 13. 网络通信与快照同步

### 进村流程

```
[Client] C2M_EnterVillage
→ [Gate] C2M_EnterVillageHandler → G2M_GetVillageSnapshot 发往 MapServer
→ [Map] G2M_GetVillageSnapshotHandler → VillageMapComponent.FillSnapshot
→ [Gate 返回] M2C_EnterVillage { ResourceNodes, Buildings, Villagers, Stocks, StorageCapacity }
→ [Client] EnterMapFinish_InitVillage
    ├─ StorehouseComponent.RestoreFromSnapshot
    ├─ VillageFactory.CreateResourceNode
    ├─ BuildingHelper.RestoreAsync
    └─ VillageFactory.CreateVillagerForce + AssignGatherTask
```

### 快照数据结构

| Proto 消息 | 说明 |
|---|---|
| `VillageResourceInfo` | ConfigId, Position, CurrentAmount |
| `VillageBuildingInfo` | ConfigId, Position, State |
| `VillagerInfo` | ConfigId, Position, GatheringResourceType |
| `VillageResourceStock` | ResourceType, Amount |

---

## 14. UI 系统

### UI 类型列表

| UIType | 层级 | 触发时机 | 说明 |
|---|---|---|---|
| `UILoading` | Mid | 场景加载时 | 加载过渡界面 |
| `UILogin` | Mid | AppStartInitFinish | 账号密码登录 |
| `UILobby` | Mid | LoginFinish | 游戏大厅 |
| `UIHelp` | Top | 按钮点击 | 帮助说明 |
| `UIMain` | Mid | EnterMapFinish | 游戏内 HUD |

### UIMain（游戏内 HUD）

| 控件 | 说明 |
|---|---|
| WoodText | 显示当前木材存量 |
| StoneText | 显示当前石矿存量 |
| FoodText | 显示食物存量 / 仓库总上限 |
| PopulationText | 显示当前人口 / 人口上限 |
| BuildStorehouseBtn | 建造仓库 |
| BuildLumbermillBtn | 建造伐木场 |
| BuildQuarryBtn | 建造采石场 |
| BuildFarmBtn | 建造农场 |
| BuildHouseBtn | 建造村民小屋 |

### 事件驱动刷新

| 事件 | 响应处理器 | 效果 |
|---|---|---|
| `ResourceChanged` | `ResourceChanged_RefreshUIMain` | 刷新资源文本 |
| `PopulationCapacityChanged` | `PopulationCapacityChanged_RefreshUIMain` | 刷新人口文本 |

---

## 15. 视图与动画系统

### 单元视图事件

| 事件 | View 处理器 | 效果 |
|---|---|---|
| `AfterVillagerCreate` | `AfterVillagerCreate_CreateView` | 加载 Prefab + 挂 AnimatorComponent |
| `AfterBuildingCreate` | `AfterBuildingCreate_CreateView` | 加载 Prefab，Scale = 0.1（建造中） |
| `BuildingFinished` | `BuildingFinished_UpdateView` | Scale → 1.0（建造完成） |
| `AfterResourceNodeCreate` | `AfterResourceNodeCreate_CreateView` | 加载 Prefab |
| `ResourceNodeExhausted` | `ResourceNodeExhausted_HideView` | SetActive(false) |
| `ResourceNodeRespawned` | `ResourceNodeRespawned_ShowView` | SetActive(true) |
| `VillagerGatherStart` | Villager_View | 播放采集动画（AnimatorComponent） |
| `VillagerGatherStop` | Villager_View | 停止采集动画 |

### 动画状态（`GatherMotion` 字段）

由 `ResourceConfig.GatherMotion` 指定动画名（如 `"ChopTree"`、`"MineRock"`），在 View 层通过 `AnimatorComponent.SetTrigger` 触发。

---

## 16. 服务端架构

### 进程角色

| 进程 | SceneType | 职责 |
|---|---|---|
| Realm | Realm | 登录鉴权，颁发 Token |
| Gate | Gate | TCP 接入，Session 管理，消息转发 |
| Map | Map | 村庄权威逻辑（移动/采集/建造）|

### 核心服务端组件

| 组件 | 位置 | 说明 |
|---|---|---|
| `VillageMapComponent` | Map Scene Root | 权威数据（ResourceNodes/Buildings/Villagers/Stocks） |
| `PlayerSessionMapComponent` | Player | 记录玩家 Gate Session ActorId |
| `UnitComponent` | Map Scene | Unit 管理 |

### 消息 Handler 列表

| Handler | SceneType | 说明 |
|---|---|---|
| `C2M_EnterVillageHandler` | Gate | 请求村庄快照 |
| `G2M_GetVillageSnapshotHandler` | Map | 返回村庄快照 |
| `C2M_BuildHandler` | Gate | 转发建造请求 |
| `G2M_BuildHandler` | Map | 执行权威建造逻辑 |
| `G2M_PlayerEnterHandler` | Map | 玩家进入 Map |
| `G2M_PlayerLeaveHandler` | Map | 玩家离开 Map |

---

## 17. 文件结构速查

```
Packages/cn.etetet.statesync/
├── Proto/
│   ├── StateSyncOuter_C_11001.proto    # 客户端↔Gate 协议
│   └── StateSyncInner_S_21001.proto    # Gate↔Map 内部协议
│
├── Scripts/
│   ├── Model/Share/Village/
│   │   ├── VillageComponent.cs         # 场景内 Unit 列表管理
│   │   ├── StorehouseComponent.cs      # 全局仓库数据
│   │   ├── PopulationComponent.cs      # 人口上限管理        [新增]
│   │   ├── VillagerComponent.cs        # 村民 AI 状态
│   │   ├── BuildingComponent.cs        # 建筑状态
│   │   ├── ResourceNodeComponent.cs    # 资源节点状态
│   │   └── VillageEventType.cs         # 村庄事件类型定义
│   │
│   ├── Model/Share/
│   │   └── VillageType.cs              # ResourceType / BuildingType 枚举
│   │
│   ├── Model/Server/Village/
│   │   └── VillageMapComponent.cs      # 服务端权威数据
│   │
│   ├── Hotfix/Share/Village/
│   │   ├── VillageComponentSystem.cs   # FindNearestResource/Storehouse
│   │   ├── StorehouseComponentSystem.cs
│   │   ├── PopulationComponentSystem.cs [新增]
│   │   ├── VillagerComponentSystem.cs  # AI 主循环
│   │   ├── BuildingComponentSystem.cs  # 含 ApplyHouseBonus  [修改]
│   │   ├── BuildingHelper.cs           # Build/Restore       [修改]
│   │   ├── ResourceNodeComponentSystem.cs # 含 Respawn       [修改]
│   │   ├── ResourceNodeExhausted_Respawn.cs [新增]
│   │   ├── VillageFactory.cs           # 含人口检查          [修改]
│   │   └── BuildingConfigCategoryExtension.cs [新增]
│   │
│   ├── Hotfix/Server/Map/Village/
│   │   ├── VillageMapComponentSystem.cs # 含 CanAfford/Build [修改]
│   │   ├── C2M_EnterVillageHandler.cs
│   │   ├── G2M_GetVillageSnapshotHandler.cs
│   │   ├── C2M_BuildHandler.cs          [新增]
│   │   └── G2M_BuildHandler.cs          [新增]
│   │
│   ├── ModelView/Client/UI/
│   │   ├── UIType.cs                    # UIMain 常量
│   │   └── UIMain/
│   │       └── UIMainComponent.cs       [新增]
│   │
│   └── HotfixView/Client/
│       ├── Scene/
│       │   ├── AfterCreateCurrentScene_AddComponent.cs [修改：+PopulationComponent]
│       │   └── EnterMapFinish_InitVillage.cs          [修改：用 CreateVillagerForce]
│       ├── UI/UIMain/
│       │   ├── UIMainEvent.cs                     [新增]
│       │   ├── UIMainComponentSystem.cs            [新增]
│       │   ├── EnterMapFinish_CreateUIMain.cs      [新增]
│       │   ├── ResourceChanged_RefreshUIMain.cs    [新增]
│       │   └── PopulationCapacityChanged_RefreshUIMain.cs [新增]
│       └── Unit/
│           └── ResourceNode_View.cs    # 含 Respawned_ShowView [修改]
│
Packages/cn.etetet.excel/CodeMode/Model/ClientServer/
├── BuildingConfig.cs
├── ResourceConfig.cs
└── VillagerConfig.cs
```

---

## 附录：功能完成度总览

| 功能模块 | 状态 | 备注 |
|---|---|---|
| 登录系统 | ✅ 完整 | |
| 资源系统（木/石/食） | ✅ 完整 | |
| 资源采集系统 | ✅ 完整 | |
| 资源节点再生 | ✅ 完整 | 30 秒后自动复活 |
| 村民 AI（5 状态） | ✅ 完整 | |
| 人口系统 | ✅ 完整 | House 建筑控制上限 |
| 建筑系统（5 种） | ✅ 完整 | |
| 玩家建造指令（C/S） | ✅ 完整 | C2M_Build 协议链路 |
| 仓库系统 | ✅ 完整 | |
| 数值系统 | ✅ 完整 | 五段式公式 |
| 配置表系统 | ✅ 完整 | |
| 网络通信 / 快照同步 | ✅ 完整 | |
| UIMain（游戏内 HUD） | ✅ 完整 | 资源 + 人口 + 建造按钮 |
| 视图与动画 | ✅ 完整 | |
| 存档 / 读档 | ⚠️ 未实现 | 当前为内存快照，无数据库持久化 |
| 多人联机 | ⚠️ 未实现 | 框架支持，业务逻辑未扩展 |
