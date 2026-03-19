# ET框架 登录 & 进入游戏 消息时序图

## 一、登录流程

```mermaid
sequenceDiagram
    participant UI as 客户端UI
    participant Main as Main Fiber
    participant NetClient as NetClient Fiber
    participant Realm as Realm Server
    participant Gate as Gate Server

    UI->>Main: LoginHelper.Login(address, account, password)
    Main->>Main: AddComponent<ClientSenderComponent>
    Main->>NetClient: ProcessInner: Main2NetClient_Login
    Note over NetClient: Main2NetClient_LoginHandler

    NetClient->>NetClient: AddComponent<RouterAddressComponent>
    NetClient->>NetClient: 初始化NetComponent(UDP/WebSocket)

    NetClient->>Realm: 建立 Router Session
    NetClient->>Realm: C2R_Login(Account, Password)
    Note over Realm: C2R_LoginHandler

    Realm->>Gate: Actor消息: R2G_GetLoginKey(Account)
    Note over Gate: R2G_GetLoginKeyHandler
    Gate->>Gate: 生成随机Key，存入GateSessionKeyComponent
    Gate-->>Realm: G2R_GetLoginKey(Key, GateId)

    Realm-->>NetClient: R2C_Login(Address=Gate地址, Key, GateId)
    Note over NetClient: Realm Session 关闭(1秒后)

    NetClient->>Gate: 建立新的 Router Session（Gate地址）
    NetClient->>Gate: C2G_LoginGate(Key, GateId)
    Note over Gate: C2G_LoginGateHandler
    Gate->>Gate: 用Key查找Account
    Gate->>Gate: 创建Player，注册Location(Player/GateSession)
    Gate->>Gate: Session绑定SessionPlayerComponent

    Gate-->>NetClient: G2C_LoginGate(PlayerId)
    NetClient-->>Main: NetClient2Main_Login(PlayerId)
    Main->>Main: PlayerComponent.MyId = PlayerId
    Main->>Main: PublishAsync(LoginFinish事件)
```

---

## 二、进入地图流程

```mermaid
sequenceDiagram
    participant UI as 客户端UI
    participant Main as Main Fiber
    participant NetClient as NetClient Fiber
    participant Gate as Gate Server
    participant GateMap as GateMap Scene (Gate内临时)
    participant MapServer as Map Server

    UI->>Main: EnterMapHelper.EnterMapAsync(root)
    Main->>NetClient: ClientSenderComponent.Call(C2G_EnterMap)
    NetClient->>Gate: C2G_EnterMap
    Note over Gate: C2G_EnterMapHandler

    Gate->>Gate: 获取Player(SessionPlayerComponent)
    Gate->>Gate: Player.AddComponent<GateMapComponent>
    Gate->>GateMap: GateMapFactory.Create() 创建临时GateMap Scene
    GateMap->>GateMap: UnitFactory.Create(player.Id, UnitType.Player)
    Note over Gate: 先返回G2C_EnterMap，再执行传送

    Gate-->>NetClient: G2C_EnterMap(PlayerId)
    NetClient-->>Main: G2C_EnterMap响应

    Note over GateMap: TransferAtFrameFinish（等当帧结束）

    GateMap->>GateMap: 序列化Unit及其Components(ITransfer)
    GateMap->>GateMap: LocationProxy.Lock(Unit, OldActorId)
    GateMap->>MapServer: Actor消息: M2M_UnitTransferRequest(Unit bytes, Entitys)
    Note over MapServer: M2M_UnitTransferRequestHandler

    MapServer->>MapServer: 反序列化Unit，AddChild到UnitComponent
    MapServer->>MapServer: 反序列化各Entity并AddComponent
    MapServer->>MapServer: Unit.AddComponent<MailBoxComponent>

    MapServer-->>Main: 推送 M2C_StartSceneChange(SceneInstanceId, SceneName)
    Note over Main: M2C_StartSceneChangeHandler → SceneChangeHelper.SceneChangeTo()
    Main->>Main: 删除旧CurrentScene，创建新CurrentScene
    Main->>Main: Publish(SceneChangeStart事件) [可在此显示Loading]
    Main->>Main: 等待 Wait_CreateMyUnit

    MapServer-->>Main: 推送 M2C_CreateMyUnit(UnitInfo)
    Note over Main: M2C_CreateMyUnitHandler → ObjectWait.Notify(Wait_CreateMyUnit)

    Main->>Main: UnitFactory.Create(currentScene, unitInfo)
    Main->>Main: Publish(SceneChangeFinish事件)
    Main->>Main: ObjectWait.Notify(Wait_SceneChangeFinish)

    MapServer->>MapServer: LocationProxy.UnLock(Unit, OldActorId, NewActorId)

    Note over Main: EnterMapHelper中Wait_SceneChangeFinish完成
    Main->>Main: Publish(EnterMapFinish事件)
```

---

## 三、关键消息汇总表

| 消息 | 方向 | 处理Handler | 说明 |
|------|------|-------------|------|
| `Main2NetClient_Login` | Main→NetClient | `Main2NetClient_LoginHandler` | 跨Fiber触发登录 |
| `C2R_Login` | Client→Realm | `C2R_LoginHandler` | 账号密码登录Realm |
| `R2G_GetLoginKey` | Realm→Gate | `R2G_GetLoginKeyHandler` | Realm向Gate申请Key |
| `G2R_GetLoginKey` | Gate→Realm | (响应) | 返回Key和GateId |
| `R2C_Login` | Realm→Client | (响应) | 返回Gate地址和Key |
| `C2G_LoginGate` | Client→Gate | `C2G_LoginGateHandler` | 携带Key登录Gate，创建Player |
| `G2C_LoginGate` | Gate→Client | (响应) | 返回PlayerId |
| `C2G_EnterMap` | Client→Gate | `C2G_EnterMapHandler` | 请求进入地图 |
| `G2C_EnterMap` | Gate→Client | (响应) | 进入地图确认 |
| `M2M_UnitTransferRequest` | GateMap→Map | `M2M_UnitTransferRequestHandler` | 传送Unit到目标Map |
| `M2C_StartSceneChange` | Map→Client | `M2C_StartSceneChangeHandler` | 通知客户端开始切场景 |
| `M2C_CreateMyUnit` | Map→Client | `M2C_CreateMyUnitHandler` | 通知客户端创建自身Unit |

---

## 四、流程要点说明

1. **双Session设计**：客户端先连Realm获取Gate地址和Key，Realm Session用后即关，再建Gate长连接。
2. **Key验证**：Gate通过`GateSessionKeyComponent`缓存Key，防止非法连接。
3. **GateMap中转**：进入地图不直接创建Unit到MapServer，而是先在Gate上建一个临时GateMap Scene，序列化后传送，使得登录与跨地图传送逻辑统一复用`TransferHelper`。
4. **帧末传送**：`TransferAtFrameFinish`保证`G2C_EnterMap`先返回客户端，再执行传送，避免客户端乱序。
5. **Location锁**：传送前Lock Unit的Location，到达目标Map后UnLock，防止传送过程中消息路由错误。
6. **客户端等待链**：`EnterMapAsync` → 等`Wait_SceneChangeFinish` → `SceneChangeTo`内等`Wait_CreateMyUnit`，形成异步等待链，保证UI流程串行正确。
