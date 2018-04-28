using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using BattleTech;
using Harmony;

namespace LessHeadInjuries
{


    public class Mech_HarmonyPatch
    {
   
        
        public static void HarmonyPatch()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Battletech.realitymachina.Mech_HarmonyPatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // find the ApplyHeadStructureEffects method of the class BattleTech.Mech
            MethodInfo targetmethod = AccessTools.Method(typeof(BattleTech.Mech), "ApplyHeadStructureEffects");

            // find the static method to call before (i.e. Prefix) the targetmethod
            HarmonyMethod prefixmethod = new HarmonyMethod(typeof(LessHeadInjuries.Mech_HarmonyPatch).GetMethod("ApplyHeadStructureEffects_Prefix"));

            // patch the targetmethod, by calling prefixmethod before it runs, with no postfixmethod (i.e. null)
            harmony.Patch(targetmethod, prefixmethod, null);

        }

        //a prefix method like this can be set to return void or bool
        //use void if you want it to just alter something before the original method
        //here, we use bool since we want to override the original method
        public static bool ApplyHeadStructureEffects_Prefix(Mech __instance, ref ChassisLocations location, ref LocationDamageLevel oldDamageLevel, ref LocationDamageLevel newDamageLevel, ref string sourceID, ref int stackItemUID)
        {
            if (newDamageLevel == oldDamageLevel || newDamageLevel != LocationDamageLevel.Destroyed)
                return true;

            __instance.pilot.SetNeedsInjury(InjuryReason.HeadHit);

            __instance.pilot.LethalInjurePilot(sourceID, stackItemUID, true, "Cockpit Destroyed");
            __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, "PILOT: LETHAL DAMAGE!", FloatieMessage.MessageNature.PilotInjury, true)));

            return true;
        }

    }
}
