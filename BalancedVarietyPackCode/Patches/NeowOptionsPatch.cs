using System.Reflection;
using BalancedVarietyPack.BalancedVarietyPackCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Patches;


[HarmonyPatch(typeof(Neow), methodName: "CurseOptions", MethodType.Getter)]
internal static class NeowOptionsPatch
{
    private static readonly MethodInfo GetRelicOption =
        AccessTools.Method(
            typeof(AncientEventModel),
            name: "RelicOption",
            parameters: [
                typeof(RelicModel), typeof(string), typeof(string)
            ]);
    
    [HarmonyPostfix]
    private static void AddBloodyCrownOption(
        Neow __instance,
        ref IEnumerable<EventOption> __result)
    {
        RelicModel relic = ModelDb.Relic<BloodyCrown>().ToMutable();

        EventOption option = (EventOption) GetRelicOption.Invoke(
            __instance,
            parameters: [
                relic, "INITIAL", "NEOW.pages.DONE.CURSED.description"
            ])!;

        __result = __result.Append(option);
    }
}
