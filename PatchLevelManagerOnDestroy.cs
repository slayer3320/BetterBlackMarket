using HarmonyLib;
using EventReports;

namespace BetterBlackMarket
{
    [HarmonyPatch(typeof(LevelManager), "OnDestroy")]
    public class PatchLevelManagerOnDestroy
    {
        static void Postfix()
        {
            LevelManager.OnEvacuated -= PatchLevelManagerStart.OnEvacuated;
        }
    }
}

