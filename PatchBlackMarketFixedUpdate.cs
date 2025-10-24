using HarmonyLib;
using Duckov.BlackMarkets;
using System;

namespace BetterBlackMarket
{
    [HarmonyPatch(typeof(BlackMarket), "FixedUpdate")]
    public class PatchBlackMarketFixedUpdate
    {
        static bool Prefix(BlackMarket __instance)
        {
            if (ModBehaviour.RefreshType == ModBehaviour.RefreshTypeEnum.Round)
            {
                Traverse.Create(__instance).Property("LastRefreshedTime").SetValue(DateTime.UtcNow);
                return false;
            }
            return true;
        }
    }
}
