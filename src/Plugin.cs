using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnlimitedTrinkets
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = null!;
        internal static ConfigEntry<bool> EnableUnlimitedTrinkets = null!;

        // 当前是否因本 Mod 而禁用了排行榜（用于 Finish 时重置）
        internal static bool weDisabledLeaderboards = false;

        private void Awake()
        {
            Logger = base.Logger;

            EnableUnlimitedTrinkets = Config.Bind(
                "General",
                "EnableUnlimitedTrinkets",
                true,
                "When true, removes the budget restriction for selecting Trinkets."
            );

            if (EnableUnlimitedTrinkets.Value)
            {
                var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
                harmony.PatchAll();
                Logger.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Patches applied. Trinket selection limit removed!");
            }
            else
            {
                Logger.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Plugin loaded but disabled via config.");
            }
        }

        // ----------------------------------------------------------------
        // 辅助方法：根据已选 Trinket 列表判断是否超出原版预算
        // 原版规则：Trinkets总cost <= (Bindings总cost + 1)
        // ----------------------------------------------------------------
        internal static bool IsTrinketBudgetExceeded(List<string> trinketNames)
        {
            int trinketCost = 0;
            int bindingBudget = 1; // 原版基础预算为 1

            foreach (string name in trinketNames)
            {
                Trinket asset = CL_AssetManager.GetTrinketAsset(name);
                if (asset == null) continue;

                if (asset.isBinding)
                    bindingBudget += asset.cost;
                else
                    trinketCost += asset.cost;
            }

            return trinketCost > bindingBudget;
        }
    }

    // ====================================================================
    // Patch 1: UI_TrinketPicker.SelectTrinket — Postfix
    //   原版: cost超限则显示"Too expensive!"并禁用Play按钮
    //   修复: 重新启用Play按钮（costText由UpdateTrinketActivation处理）
    // ====================================================================
    [HarmonyPatch(typeof(UI_TrinketPicker), "SelectTrinket")]
    internal static class Patch_SelectTrinket
    {
        static void Postfix(UI_TrinketPicker __instance)
        {
            // 只启用按钮，不清空costText
            // costText的更新由UpdateTrinketActivation Postfix处理
            __instance.playButton.interactable = true;
        }
    }

    // ====================================================================
    // Patch 2: UI_TrinketPicker.UpdateTrinketActivation — Postfix
    //   在UI刷新后:
    //   - 清除 "Too expensive!" 并重新启用按钮
    //   - 若当前组合超出原版预算，在 costText 中提示排行榜已禁用
    // ====================================================================
    [HarmonyPatch(typeof(UI_TrinketPicker), "UpdateTrinketActivation")]
    internal static class Patch_UpdateTrinketActivation
    {
        static void Postfix(UI_TrinketPicker __instance)
        {
            // 永远清除费用错误
            __instance.playButton.interactable = true;

            // 判断当前选择是否超出原版预算
            var selectedNames = new List<string>();
            foreach (var t in __instance.selectedTrinkets)
                selectedNames.Add(t.name);

            if (Plugin.IsTrinketBudgetExceeded(selectedNames))
            {
                // 用橙色提示代替红色错误
                __instance.costText.text = "<color=\"orange\">[Mod] Leaderboards disabled";
            }
            else
            {
                __instance.costText.text = "";
            }
        }
    }

    // ====================================================================
    // Patch 3: M_Gamemode.StartFreshGamemode — Postfix
    //   游戏正式开始时: 若超出原版预算，设置官方禁用标志
    // ====================================================================
    [HarmonyPatch(typeof(M_Gamemode), "StartFreshGamemode")]
    internal static class Patch_StartFreshGamemode
    {
        static void Postfix()
        {
            M_Gamemode gm = CL_GameManager.GetBaseGamemode();
            if (gm == null) return;

            List<string> trinkets = StatManager.saveData.GetGamemodeTrinkets(gm.GetGamemodeName());

            if (Plugin.IsTrinketBudgetExceeded(trinkets))
            {
                CL_Leaderboard.WK_Leaderboard_Core.disableLeaderboards = true;
                Plugin.weDisabledLeaderboards = true;
                Plugin.Logger.LogInfo("[UnlimitedTrinkets] Budget exceeded — leaderboards disabled for this run.");
            }
            else
            {
                // 未超出预算时确保不误禁
                if (Plugin.weDisabledLeaderboards)
                {
                    CL_Leaderboard.WK_Leaderboard_Core.disableLeaderboards = false;
                    Plugin.weDisabledLeaderboards = false;
                }
            }
        }
    }

    // ====================================================================
    // Patch 4: M_Gamemode.Finish — Postfix
    //   局结束后重置我们设置的排行榜禁用标志，防止污染后续局
    // ====================================================================
    [HarmonyPatch(typeof(M_Gamemode), "Finish")]
    internal static class Patch_Finish
    {
        static void Postfix()
        {
            if (Plugin.weDisabledLeaderboards)
            {
                CL_Leaderboard.WK_Leaderboard_Core.disableLeaderboards = false;
                Plugin.weDisabledLeaderboards = false;
                Plugin.Logger.LogInfo("[UnlimitedTrinkets] Run ended — leaderboard disable flag reset.");
            }
        }
    }
}
