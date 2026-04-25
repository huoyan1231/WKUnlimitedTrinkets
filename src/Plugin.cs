using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using WK_huoyan1231COMLib;

namespace UnlimitedTrinkets
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("huoyan1231.whiteknuckle.comlib")]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = null!;
        internal static ConfigEntry<bool> EnableUnlimitedTrinkets = null!;

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
    //   - 预算判断委托给 COMLib 的 LeaderboardManager
    // ====================================================================
    [HarmonyPatch(typeof(UI_TrinketPicker), "UpdateTrinketActivation")]
    internal static class Patch_UpdateTrinketActivation
    {
        static void Postfix(UI_TrinketPicker __instance)
        {
            // 永远清除费用错误
            __instance.playButton.interactable = true;

            // 判断当前选择是否超出原版预算（委托给 COMLib）
            var selectedNames = new List<string>();
            foreach (var t in __instance.selectedTrinkets)
                selectedNames.Add(t.name);

            if (LeaderboardManager.IsTrinketBudgetExceeded(selectedNames))
            {
                // 用橙色提示代替红色错误
                __instance.costText.text = "<color=\"orange\">Leaderboards disabled";
            }
            else
            {
                __instance.costText.text = "";
            }
        }
    }

    // ====================================================================
    // Patch 3: M_Gamemode.StartFreshGamemode — Postfix
    //   游戏正式开始时: 若超出原版预算，通过 COMLib 禁用排行榜
    // ====================================================================
    [HarmonyPatch(typeof(M_Gamemode), "StartFreshGamemode")]
    internal static class Patch_StartFreshGamemode
    {
        static void Postfix()
        {
            M_Gamemode gm = CL_GameManager.GetBaseGamemode();
            if (gm == null) return;

            List<string> trinkets = StatManager.saveData.GetGamemodeTrinkets(gm.GetGamemodeName());

            if (LeaderboardManager.IsTrinketBudgetExceeded(trinkets))
            {
                LeaderboardManager.DisableForThisRun(MyPluginInfo.PLUGIN_GUID);
                Plugin.Logger.LogInfo("[UnlimitedTrinkets] Budget exceeded — leaderboard disable requested via COMLib.");
            }
            else
            {
                // 未超出预算时撤销之前可能存在的禁用请求
                LeaderboardManager.TryRestore(MyPluginInfo.PLUGIN_GUID);
            }
        }
    }

    // ====================================================================
    // Patch 4: M_Gamemode.Finish — Postfix
    //   局结束后撤销本 Mod 的排行榜禁用请求（COMLib 会统一 ResetAll）
    // ====================================================================
    [HarmonyPatch(typeof(M_Gamemode), "Finish")]
    internal static class Patch_Finish
    {
        static void Postfix()
        {
            // COMLib 的 Finish Patch 会调用 ResetAll()，此处撤销本 Mod 请求即可
            LeaderboardManager.TryRestore(MyPluginInfo.PLUGIN_GUID);
        }
    }
}
