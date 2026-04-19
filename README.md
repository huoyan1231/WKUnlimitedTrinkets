# UnlimitedTrinkets / 无限饰品

**White Knuckle BepInEx Mod** — Removes the budget restriction for selecting Trinkets  
**White Knuckle BepInEx 模组** — 移除 Trinkets 选择的费用限制

---

## English

### Features

White Knuckle's Trinkets & Bindings interface has a "budget" system:
- Each Trinket/Binding has a `cost` value
- Total Trinkets cost cannot exceed "selected Bindings total cost + 1"
- When exceeded, the UI shows **Too expensive!** and disables the Play button

This Mod uses Harmony Patches to force-clear the error message and re-enable the Play button after `SelectTrinket` and `UpdateTrinketActivation` methods execute, allowing you to **freely select any number of Trinkets** without budget constraints.

When your selection exceeds the original budget, an orange hint `[Mod] Leaderboards disabled` will be displayed, and the leaderboard will be automatically disabled for that run.

### Installation

1. Ensure **BepInEx 5.x** is installed (via Gale or manual installation)
2. Place `UnlimitedTrinkets.dll` in:
   ```
   <Game Root>/BepInEx/plugins/huoyan1231-UnlimitedTrinkets/
   ```
   Or Gale profile path:
   ```
   D:/GameTools/Gale/DATA/white-knuckle/profiles/Default/BepInEx/plugins/huoyan1231-UnlimitedTrinkets/
   ```
3. Launch the game and freely select Trinkets in the Trinkets & Bindings interface

### Configuration

A config file will be generated in `BepInEx/config/` after first run:
```
huoyan1231.whiteknuckle.unlimitedtrinkets.cfg
```

| Setting | Default | Description |
|---------|---------|-------------|
| `EnableUnlimitedTrinkets` | `true` | Set to `false` to temporarily disable the Mod |

### Notes

- Score Multiplier calculation logic is **not affected** and will display correctly
- Trinkets unlock status is not affected (locked ones remain unselectable)
- Bindings selection is not affected

---

## 中文

### 功能

White Knuckle 的 Trinkets & Bindings 界面有一个"预算"系统：
- 每个 Trinket/Binding 都有一个 `cost` 值
- Trinkets 总费用不能超过"已选 Bindings 总费用 + 1"
- 超出后界面显示 **Too expensive!** 并禁用 Play 按钮

本 Mod 通过 Harmony Patch 在 `SelectTrinket` 和 `UpdateTrinketActivation` 方法执行后，强制清除错误提示并重新启用 Play 按钮，从而让你**自由选择任意数量的 Trinkets**，不受预算约束。

当选择超出原版预算时，会显示橙色提示 `[Mod] Leaderboards disabled`，并自动禁用该局的排行榜。

### 安装

1. 确保已安装 **BepInEx 5.x** (via Gale 或手动安装)
2. 将 `UnlimitedTrinkets.dll` 放入：
   ```
   <游戏根目录>/BepInEx/plugins/huoyan1231-UnlimitedTrinkets/
   ```
   或 Gale profile 路径：
   ```
   D:/GameTools/Gale/DATA/white-knuckle/profiles/Default/BepInEx/plugins/huoyan1231-UnlimitedTrinkets/
   ```
3. 启动游戏，进入 Trinkets & Bindings 界面即可自由选择

### 配置

首次运行后会在 `BepInEx/config/` 生成配置文件：
```
huoyan1231.whiteknuckle.unlimitedtrinkets.cfg
```

| 配置项 | 默认值 | 说明 |
|--------|--------|------|
| `EnableUnlimitedTrinkets` | `true` | 设为 `false` 可临时禁用 Mod |

### 注意事项

- 分数倍率 (Score Multiplier) 的计算逻辑**不受影响**，仍会正确显示
- Trinkets 的解锁状态不受影响（未解锁的仍不可选）
- Bindings 的选择不受影响

---

## Build / 构建

Requires .NET SDK (>= 6.0) / 需要 .NET SDK (>= 6.0)

```bash
dotnet build UnlimitedTrinkets.csproj -c Release
```

Output / 输出：`bin/Release/net471/UnlimitedTrinkets.dll`

---

## License / 许可证

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)**.

本项目采用 **GNU 通用公共许可证第三版 (GPL-3.0)** 授权。

See [LICENSE](LICENSE) file for full text. / 完整许可证文本见 [LICENSE](LICENSE) 文件。

---

## AI Generated Notice / AI 生成声明

**This code was generated with assistance from AI.**

- **AI Model / AI 模型**: Claude 4 (Anthropic)
- **Purpose / 用途**: Game mod development for White Knuckle
- **Generated Date / 生成日期**: 2026-04-19

The AI assisted in writing the Harmony patches, debugging, and documentation. All code has been reviewed and tested by the project maintainer.

本代码在 AI 辅助下生成。AI 协助编写了 Harmony 补丁、调试和文档。所有代码都经过项目维护者的审查和测试。


----------------
有点意思，这个其实是腾讯Workbuddy生成的，后台显示模型是Kimi-h2.5和minimax-m2.7，但是他自己说他是Claude
