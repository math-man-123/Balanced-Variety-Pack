using System.Collections;
using System.Reflection;
using BalancedVarietyPack.BalancedVarietyPackCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Patches;


// Darv's options are stored in a private field with no patchable getter
// hence early prefix points are used to insert new options directly
[HarmonyPatch(typeof(Darv))]
internal static class DarvOptionsPatch
{
    private static bool _inserted;

    // this is for gameplay insertion
    [HarmonyPatch(methodName: "GenerateInitialOptions")]
    [HarmonyPrefix]
    private static void GenerateInitialOptionsPrefix() => InsertNewOptions();

    // this is for compendium insertion
    [HarmonyPatch(methodName: "AllPossibleOptions", MethodType.Getter)]
    [HarmonyPrefix]
    private static void GetAllPossibleOptionsPrefix() => InsertNewOptions();
    
    private static void InsertNewOptions()
    {
        // only insert once into _validRelicSets
        if (_inserted) return;

        Type validRelicSetStruct =
            AccessTools.Inner(typeof(Darv), "ValidRelicSet");

        // grab constructor reflection of private ValidRelicSet struct
        ConstructorInfo? newValidRelicSet = AccessTools.Constructor(
            validRelicSetStruct,
            [
                typeof(Func<Player, bool>),
                typeof(RelicModel[])
            ]);
        if (newValidRelicSet is null) return;
        
        // corresponds to new Darv.ValidRelicSet(bloodyCrown.CanBePickedUp, bloodyCrown)
        BloodyCrown bloodyCrown = ModelDb.Relic<BloodyCrown>();
        object bloodyCrownRelicSet = newValidRelicSet.Invoke(
        [
            (Func<Player, bool>) bloodyCrown.CanBePickedUp,
            new RelicModel[] { bloodyCrown }
        ]);

        // grab the private field _validRelicSets if possible
        IList? validRelicSets = (IList?) AccessTools
            .Field(typeof(Darv), "_validRelicSets")
            .GetValue(null);
        if (validRelicSets is null) return;
        
        validRelicSets.Add(bloodyCrownRelicSet);
        _inserted = true;
    }
}

