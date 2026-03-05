# ET框架 Assembly 分类与设置机制

## 一、核心设计思路

ET框架采用 **"少量固定asmdef + 大量asmref"** 的策略：
- 全局只定义几个固定的 `.asmdef`（Assembly Definition），对应最终输出的几个dll
- 各功能包（Package）里的脚本通过 `.asmref`（Assembly Reference）"注入"到对应的asmdef中
- 这样不管有多少功能Package，最终编译出的dll数量是固定的

---

## 二、Assembly 分类（层次结构）

ET框架按 **运行时层** 和 **运行端** 两个维度来分类：

### 维度1：运行时层（Layer）

| Layer | Assembly 名 | 说明 |
|-------|------------|------|
| Core层 | `ET.Core` | 最底层框架核心，不依赖任何业务，常驻内存，不热更 |
| Loader层 | `ET.Loader` | 加载层，负责热更新加载逻辑，不热更 |
| Model层 | `ET.Model` | 数据定义层（实体数据/组件定义），**热更新** |
| Hotfix层 | `ET.Hotfix` | 逻辑热更层（只含逻辑，无View），**热更新** |
| ModelView层 | `ET.ModelView` | 带Unity引擎引用的数据层（客户端），**热更新** |
| HotfixView层 | `ET.HotfixView` | 带Unity引擎引用的逻辑层（客户端），**热更新** |

### 维度2：运行端（Side）

每个Layer下的脚本目录再细分为：
- `Share/` — 服务端+客户端共用代码
- `Client/` — 仅客户端
- `Server/` — 仅服务端

---

## 三、Assembly 设置方式

### 3.1 固定 asmdef（定义Assembly）

只有少数几个 `.asmdef` 文件，位于 `Runtime/` 目录，这是Assembly的"根"：

```
cn.etetet.statesync/Runtime/
├── Model/       ET.Model.asmdef       ← 热更dll，noEngineReferences:true
├── Hotfix/      ET.Hotfix.asmdef      ← 热更dll，noEngineReferences:true
├── ModelView/   ET.ModelView.asmdef   ← 热更dll，含Unity引擎引用
└── HotfixView/  ET.HotfixView.asmdef  ← 热更dll，含Unity引擎引用

cn.etetet.core/Runtime/
└── ET.Core.asmdef                     ← 常驻dll

cn.etetet.loader/Runtime/
└── ET.Loader.asmdef                   ← 常驻dll
```

关键配置项：
```json
// ET.Model.asmdef 示例
{
  "name": "ET.Model",
  "noEngineReferences": true,          // Model/Hotfix 不引用Unity引擎API
  "defineConstraints": [
    "INITED",
    "IS_COMPILING || UNITY_EDITOR"     // 编译期条件，防止非编译期误激活
  ],
  "allowUnsafeCode": true
}
```

### 3.2 Ignore.*.asmdef（禁止单独编译）

每个功能包根目录都有一个 `Ignore.ET.Xxx.asmdef`，带有约束：
```json
{
  "name": "Ignore.ET.Core",
  "defineConstraints": ["IGNORE"]      // IGNORE 宏永远不存在 → 此asmdef永远不激活
}
```
作用：让Package下的脚本文件夹"默认不属于任何assembly"，防止被Unity默认assembly扫描到。

### 3.3 AssemblyReference.asmref（注入到目标Assembly）

各功能包的 `Scripts/` 下每个子目录放一个 `AssemblyReference.asmref`，声明本目录代码归属哪个Assembly：

```
Scripts/
├── Model/
│   ├── Share/   AssemblyReference.asmref → { "reference": "ET.Model" }
│   ├── Client/  AssemblyReference.asmref → { "reference": "ET.Model" }
│   └── Server/  AssemblyReference.asmref → { "reference": "ET.Model" }
├── Hotfix/
│   ├── Share/   AssemblyReference.asmref → { "reference": "ET.Hotfix" }
│   ├── Client/  AssemblyReference.asmref → { "reference": "ET.Hotfix" }
│   └── Server/  AssemblyReference.asmref → { "reference": "ET.Hotfix" }
├── ModelView/
│   └── Client/  AssemblyReference.asmref → { "reference": "ET.ModelView" }
└── HotfixView/
    └── Client/  AssemblyReference.asmref → { "reference": "ET.HotfixView" }
```

---

## 四、依赖关系图

```
ET.HotfixView
  ├── ET.Core
  ├── ET.Loader
  ├── ET.Model
  ├── ET.ModelView
  └── ET.Hotfix
        ├── ET.Core
        ├── ET.Loader
        └── ET.Model
              ├── ET.Core
              ├── ET.SourceGeneratorAttribute
              └── ET.MemoryPack

ET.Loader
  ├── ET.Core
  ├── ET.YooAssets
  └── ET.HybridCLR
```

---

## 五、最终生成的 DLL 数量

### Unity客户端侧（运行时）

| DLL | 类型 | 用途 |
|-----|------|------|
| `ET.Core.dll` | 常驻，不热更 | 框架核心 |
| `ET.Loader.dll` | 常驻，不热更 | 热更新加载器 |
| `ET.Model.dll` | **热更新** | 纯逻辑数据层（无Unity引用） |
| `ET.Hotfix.dll` | **热更新** | 纯逻辑热更层（无Unity引用） |
| `ET.ModelView.dll` | **热更新** | 客户端视图数据层（有Unity引用） |
| `ET.HotfixView.dll` | **热更新** | 客户端视图逻辑层（有Unity引用） |

**客户端运行时共6个核心业务DLL**（不含第三方库dll如ET.MemoryPack、ET.Recast、ET.YooAssets等）

### 服务端（.NET Core）

服务端不走Unity Assembly系统，直接编译为独立dll（见 `Bin/` 目录）：
- `ET.Core.dll`
- `ET.Model.dll`
- `ET.Hotfix.dll`
- `ET.Loader.dll`

### Editor工具类DLL（编辑器Only）

- `ET.Core.Editor.dll`
- `ET.Loader.Editor.dll`
- `ET.StateSync.Editor.dll`
- `ET.Excel.Editor.dll` 等

---

## 六、设计优势总结

1. **零侵入式扩展**：新增功能Package只需在 `Scripts/` 下对应目录放 `asmref` 文件即可，无需修改任何现有 `asmdef`
2. **热更新隔离**：`noEngineReferences:true` 确保 Model/Hotfix 层的DLL可以被HybridCLR加载热更，不依赖Unity引擎API
3. **客户端/服务端代码共享**：Share目录的代码同时编译到客户端dll和服务端dll
4. **编译约束**：`Ignore.*.asmdef` + `IGNORE` 宏防止脚本被Unity默认assembly扫描到导致重复编译
