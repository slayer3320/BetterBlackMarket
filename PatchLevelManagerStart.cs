using HarmonyLib;
using Duckov.BlackMarkets;
using EventReports;


namespace BetterBlackMarket
{
    [HarmonyPatch(typeof(LevelManager), "Start")]
    public class PatchLevelManagerStart
    {
        static void Postfix()
        {
            LevelManager.OnEvacuated += OnEvacuated;
        }

        public static void OnEvacuated(EvacuationInfo evacuationInfo)
        {
            if (ModBehaviour.RefreshType == ModBehaviour.RefreshTypeEnum.Round)
            {
                BlackMarket.Instance.RefreshChance += 5;
            }
        }
    }
}

