# Village 模拟经营游戏逻辑结构分析报告

## 目录

1. [项目概述](#1-项目概述)
2. [现有逻辑结构分析](#2-现有逻辑结构分析)
3. [架构层次图](#3-架构层次图)
4. [核心组件详解](#4-核心组件详解)
5. [网络通信架构](#5-网络通信架构)
6. [缺失功能分析](#6-缺失功能分析)
7. [总结与建议](#7-总结与建议)

---

## 1. 项目概述

本项目基于 **ET 框架**（Entity-Component 架构 + 异步协程 + Actor 模型），在 cn.etetet.statesync 包中实现了一个模拟经营游戏的 Village（村庄）原型。

### 技术栈

- **框架**：ET9（Unity + .NET Core 双端）
- **架构模式**：Entity-Component System（ECS）
- **网络模型**：Actor 模型 + 状态快照同步
- **AI 驱动**：异步协程状态机（非行为树）
- **配置系统**：Excel 配置表（BuildingConfig、ResourceConfig、VillagerConfig、UnitConfig）

---

## 2. 现有逻辑结构分析

### 2.1 核心实体类型（UnitType）

| 类型 | 常量值 | 说明 |
|------|--------|------|
| Player | Unit*1000+0 | 玩家操作中心点 |
| Villager | Unit*1000+1 | 村民（AI 自动采集） |
| Building | Unit*1000+2 | 建筑（仓库/伐木场等） |
| Monster | Unit*1000+3 | 怪物（预留） |

### 2.2 资源类型（ResourceType）

| 类型 | 常量值 | 说明 |
|------|--------|------|
| Wood | 1 | 木材 |
| Stone | 2 | 石矿 |
| Food | 3 | 食物 |

### 2.3 建筑类型（BuildingType）

| 类型 | 常量值 | 说明 |
|------|--------|------|
| Storehouse | 1 | 仓库（存储资源） |
| Lumbermill | 2 | 伐木场 |
| Quarry | 3 | 采石场 |
| Farm | 4 | 农场 |
| House | 5 | 村民小屋 |

### 2.4 村民 AI 状态机（VillagerState）

`
Idle（空闲）
  ↓ AssignGatherTask(resourceType)
WalkToRes（走向资源点）
  ↓ 到达资源点
Gathering（采集中）
  ↓ 携带满 or 节点耗尽
WalkToStore（走向仓库）
  ↓ 到达仓库
Storing（存储中）
  ↓ 存入完成
Idle（空闲）→ 循环
`

---

## 3. 架构层次图

`
Scene（Village 场景）
├── VillageComponent          # 场景级：管理所有 Unit ID 列表
│   ├── ResourceNodeIds[]     # 资源节点 UnitId 列表
│   ├── BuildingIds[]         # 建筑 UnitId 列表
│   └── VillagerIds[]         # 村民 UnitId 列表
├── StorehouseComponent       # 场景级：全局资源仓库
│   ├── Resources{type->amt}  # 各类资源存量
│   └── Capacity              # 仓库总容量上限
└── UnitComponent             # 场景级：所有 Unit 容器
    ├── Unit(Villager)        # 村民实体
    │   ├── VillagerComponent     # AI 状态 + 携带资源
    │   ├── MoveComponent         # 寻路移动
    │   └── NumericComponent      # 数值（速度/采集速度/携带上限）
    ├── Unit(Building)        # 建筑实体
    │   └── BuildingComponent     # 建筑状态 + 工人数量
    └── Unit(ResourceNode)    # 资源节点实体
        └── ResourceNodeComponent # 剩余储量 + 占用状态
`

---

## 4. 核心组件详解

### 4.1 VillageComponent（场景管理器）

- **挂载位置**：Scene
- **职责**：维护场景内所有资源节点、建筑、村民的 UnitId 列表
- **关键方法**：
  - FindNearestResource(pos, resourceType) — 查找最近可用资源节点
  - FindNearestStorehouse(pos) — 查找最近已建成仓库

### 4.2 VillagerComponent（村民 AI）

- **挂载位置**：Unit（村民）
- **职责**：驱动村民完整的采集→搬运→存储 AI 循环
- **关键字段**：
  - State：当前 AI 状态（5 种）
  - TargetResourceUnitId：目标资源节点
  - CarryResourceType / CarryAmount：携带资源信息
- **关键方法**：
  - AssignGatherTask(resourceType) — 分配采集任务，启动 AI 循环
  - StopTask() — 停止任务，释放节点占用

### 4.3 ResourceNodeComponent（资源节点）

- **挂载位置**：Unit（资源节点）
- **职责**：管理资源节点的储量与占用状态
- **关键字段**：
  - CurrentAmount：当前剩余储量
  - GathererUnitId：当前占用的村民 Id（同时只允许一个）
- **关键方法**：
  - TryOccupy(unitId) — 尝试占用节点
  - Gather() — 采集一次，返回实际采集量
  - Release() — 释放占用

### 4.4 BuildingComponent（建筑）

- **挂载位置**：Unit（建筑）
- **职责**：管理建筑状态与工人数量
- **建筑状态**：UnderConstruction(0) → Built(1)
- **关键方法**：
  - FinishBuild() — 完成建造，触发 BuildingFinished 事件
  - ApplyStorageBonus() — 仓库建筑完成后扩展容量
  - CanAddWorker() — 判断是否可以增加工人

### 4.5 StorehouseComponent（仓库）

- **挂载位置**：Scene
- **职责**：管理全局资源存量
- **关键方法**：
  - Deposit(type, amount) — 存入资源
  - Consume(type, amount) — 消耗资源
  - CanAfford(buildingConfig) — 判断是否有足够资源建造

### 4.6 NumericComponent（数值系统）

- **挂载位置**：Unit
- **Village 相关数值**：
  - Speed：移动速度
  - GatherSpeed / GatherSpeedBase / GatherSpeedAdd / GatherSpeedPct：采集速度（支持 Buff 叠加）
  - CarryCapacity / CarryCapacityBase：携带上限

---

## 5. 网络通信架构

### 5.1 服务器角色

| 角色 | 职责 |
|------|------|
| Realm | 登录认证，分配 Gate 地址和 Key |
| Gate | 长连接接入、Session 管理、消息路由转发 |
| Village Map | 业务权威服，持有村庄权威状态，响应快照请求 |
| Robot | 自动化客户端，压测入口 |

### 5.2 核心消息流程

`
客户端 → Gate: C2M_EnterVillage（请求村庄快照）
Gate → Map:    G2M_GetVillageSnapshot
Map → Gate:    G2M_GetVillageSnapshotResponse（资源节点/建筑/村民/库存）
Gate → 客户端: M2C_EnterVillage（快照数据）
客户端:        根据快照重建本地 Village 场景
`

### 5.3 村庄快照数据结构（Proto）

`protobuf
message M2C_EnterVillage {
    repeated VillageResourceInfo  ResourceNodes  // 资源节点列表
    repeated VillageBuildingInfo  Buildings      // 建筑列表
    repeated VillagerInfo         Villagers      // 村民列表
    repeated VillageResourceStock Stocks         // 仓库存量
    int32 StorageCapacity                        // 仓库容量上限
}
`

### 5.4 服务端权威数据（VillageMapComponent）

服务端 Map 持有村庄权威快照，包含：
- ResourceNodes：资源节点位置、ConfigId、当前储量
- Buildings：建筑位置、ConfigId、建造状态
- Villagers：村民位置、当前采集资源类型
- Stocks：各类资源存量
- StorageCapacity：仓库容量

---

## 6. 缺失功能分析

以下是当前 Village 原型与一个完整模拟经营游戏之间的差距分析：

### 6.1 🔴 核心玩法缺失

#### 6.1.1 人口与村民管理系统
- **缺失**：村民数量上限（由 House 建筑决定）
- **缺失**：村民招募/雇佣流程（消耗食物/金币）
- **缺失**：村民死亡与补充机制
- **缺失**：村民技能成长/经验系统
- **缺失**：村民手动任务分配 UI（玩家指定某村民去采集某资源）

#### 6.1.2 建筑生产系统
- **缺失**：建筑生产功能（伐木场/采石场/农场应能持续产出资源，而非仅靠村民手动采集）
- **缺失**：建筑升级系统（Lv1→Lv2→Lv3，提升产量/容量）
- **缺失**：建筑拆除/重建功能
- **缺失**：建筑建造需要工人分配（当前 BuildingComponent 有 CurrentWorkers 字段但未驱动）
- **缺失**：建筑建造进度条（当前只有等待时间，无进度反馈）

#### 6.1.3 资源节点再生系统
- **缺失**：资源节点耗尽后的再生/刷新机制（当前耗尽后永久消失）
- **缺失**：资源节点多人同时采集支持（当前限制同时只有一个村民）
- **缺失**：资源节点发现/探索机制

#### 6.1.4 科技/研究系统
- **缺失**：科技树（解锁新建筑类型、提升采集效率等）
- **缺失**：研究队列与研究时间

### 6.2 🟡 经济系统缺失

#### 6.2.1 货币系统
- **缺失**：金币/货币概念（当前只有木材/石矿/食物三种资源）
- **缺失**：资源交易/市场系统

#### 6.2.2 仓库系统扩展
- **缺失**：分类仓库（木材仓/粮仓/石料仓分开管理）
- **缺失**：资源运输队列（村民搬运时的路径优化）
- **缺失**：仓库满时的溢出处理策略

#### 6.2.3 消耗系统
- **缺失**：村民日常消耗（食物消耗维持人口）
- **缺失**：建筑维护消耗
- **缺失**：时间驱动的资源消耗 Tick

### 6.3 🟡 玩家交互系统缺失

#### 6.3.1 建造交互
- **缺失**：玩家点击地图选择建造位置的 UI 交互
- **缺失**：建筑放置合法性检测（碰撞/地形）
- **缺失**：建造取消功能

#### 6.3.2 村民控制
- **缺失**：玩家手动选中村民并指派任务的交互
- **缺失**：村民优先级设置（优先采集哪种资源）
- **缺失**：村民巡逻/守卫模式

#### 6.3.3 游戏 UI
- **缺失**：资源面板 UI（实时显示各类资源数量）
- **缺失**：建筑菜单 UI（选择建造哪种建筑）
- **缺失**：村民状态面板 UI
- **缺失**：小地图
- **缺失**：建筑信息面板（点击建筑查看详情/升级）

### 6.4 🟠 存档与持久化缺失

#### 6.4.1 数据库持久化
- **缺失**：村庄状态写入数据库（当前 VillageMapComponent 数据是硬编码初始值，重启丢失）
- **缺失**：定时存档（Auto-Save）
- **缺失**：多存档槽位

#### 6.4.2 离线进度
- **缺失**：玩家离线期间的资源产出计算（Offline Progress）
- **缺失**：离线时间补偿机制

### 6.5 🟠 多玩家/社交系统缺失

#### 6.5.1 多玩家支持
- **缺失**：多玩家共享同一村庄的权限管理
- **缺失**：玩家操作冲突解决（两个玩家同时建造同一位置）
- **缺失**：村庄状态实时同步给多个在线玩家（当前只有进入时的快照，无增量同步）

#### 6.5.2 社交功能
- **缺失**：好友系统
- **缺失**：访问其他玩家村庄
- **缺失**：联盟/公会系统

### 6.6 🔵 战斗/防御系统缺失

#### 6.6.1 防御系统
- **缺失**：敌人入侵事件（Monster 类型已定义但无 AI 逻辑）
- **缺失**：防御建筑（箭塔/城墙）
- **缺失**：村民战斗能力

#### 6.6.2 战斗数值
- **缺失**：攻击力/防御力数值（NumericType 中只有 HP/MaxHP 但无攻击相关）
- **缺失**：战斗伤害计算系统

### 6.7 🔵 任务/成就系统缺失

- **缺失**：主线任务引导（新手教程）
- **缺失**：日常任务/周常任务
- **缺失**：成就系统
- **缺失**：里程碑奖励

### 6.8 🔵 地图/世界系统缺失

- **缺失**：地图扩张机制（解锁新区域）
- **缺失**：地形系统（影响建筑放置和移动速度）
- **缺失**：天气/季节系统（影响资源产出）
- **缺失**：探索/迷雾系统

---

## 7. 总结与建议

### 7.1 现有系统完成度评估

| 模块 | 完成度 | 说明 |
|------|--------|------|
| 实体框架（Unit/Component） | ✅ 完整 | ECS 架构清晰，扩展性强 |
| 村民 AI 采集循环 | ✅ 完整 | 异步状态机，逻辑完整 |
| 资源节点管理 | ✅ 基础完整 | 缺少再生机制 |
| 建筑建造流程 | ✅ 基础完整 | 缺少升级/工人驱动 |
| 仓库资源管理 | ✅ 完整 | 存取/消耗/容量均有 |
| 数值系统 | ✅ 完整 | 支持 Buff 叠加计算 |
| 网络快照同步 | ✅ 完整 | 进入时快照，架构清晰 |
| 客户端 View 层 | 🟡 基础 | 只有 Prefab 加载，无动画状态机 |
| 游戏 UI | 🔴 缺失 | 无资源面板/建造菜单 |
| 数据持久化 | 🔴 缺失 | 数据硬编码，重启丢失 |
| 增量状态同步 | 🔴 缺失 | 只有进入快照，无实时同步 |
| 人口系统 | 🔴 缺失 | 无上限/招募机制 |
| 科技树 | 🔴 缺失 | 无研究系统 |
| 战斗系统 | 🔴 缺失 | Monster 已定义但无逻辑 |

### 7.2 优先级建议（按重要性排序）

**P0 - 必须实现（核心玩法闭环）**
1. 资源面板 UI（玩家需要看到资源数量）
2. 建造菜单 UI + 建造交互（玩家需要能建造建筑）
3. 数据持久化（村庄状态写入 DB，重启不丢失）
4. 增量状态同步（多玩家/断线重连后状态一致）
5. 资源节点再生机制

**P1 - 重要功能（提升游戏深度）**
6. 人口系统（House 建筑控制村民上限）
7. 建筑升级系统
8. 村民手动任务分配 UI
9. 离线进度计算
10. 食物消耗/人口维持机制

**P2 - 扩展功能（丰富游戏内容）**
11. 科技树
12. 敌人入侵/防御系统
13. 任务/成就系统
14. 地图扩张

### 7.3 架构扩展建议

1. **增量同步**：在 VillageMapComponent 中增加脏标记（Dirty Flag），定时将变更推送给在线玩家，而非只在进入时发快照。

2. **数据库持久化**：将 VillageMapComponent 的数据序列化到 MongoDB（ET 框架已集成 MongoBson），在 Awake 时从 DB 加载，定时或关键操作后写回。

3. **事件驱动 UI**：利用现有的 ResourceChanged、BuildingFinished 等事件，在客户端订阅并刷新 UI，无需轮询。

4. **建筑生产 Tick**：为生产类建筑（伐木场/农场）添加 TimerComponent 定时产出资源，与村民手动采集并行。

5. **多村民并发采集**：修改 ResourceNodeComponent 支持多个 GathererUnitId（改为列表），并在 Gather() 中按比例分配产出。
