using HarmonyLib;
using Duckov.BlackMarkets;
using Duckov.BlackMarkets.UI;
using TMPro;

namespace BetterBlackMarket
{
    [HarmonyPatch(typeof(BlackMarketView), "Update")]
    public class PatchBlackMarketViewUpdate
    {
        static void Postfix(BlackMarketView __instance)
        {
            if(ModBehaviour.RefreshType == ModBehaviour.RefreshTypeEnum.Round)
            {
                Traverse.Create(__instance).Field("refreshETAText").GetValue<TextMeshProUGUI>().text 
                = "撤离成功后补充刷新次数";
            }
        }
    }
}
