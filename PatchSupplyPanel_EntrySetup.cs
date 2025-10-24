using HarmonyLib;
using Duckov.BlackMarkets;
using Duckov.BlackMarkets.UI;
using ItemStatsSystem;


namespace BetterBlackMarket
{
    [HarmonyPatch(typeof(SupplyPanel_Entry), "Setup")]
    public class PatchSupplyPanel_EntrySetup
    {
        static void Postfix(SupplyPanel_Entry __instance, BlackMarket.DemandSupplyEntry target)
        {
            Traverse.Create(__instance).Field("resultDisplay").GetValue<ItemAmountDisplay>()
            .Setup(target.ItemID, Traverse.Create(target).Field("batchCount").GetValue<int>() * 
                    Traverse.Create(target).Property("ItemMetaData").GetValue<ItemMetaData>().defaultStackCount);

            Traverse.Create(__instance).Method("Refresh").GetValue();
        }
    }
}