using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using BattleTech;
using Harmony;
using UnityEngine;


//code taken and modified from: https://github.com/Mpstark/LessPilotInjuries
namespace LessHeadInjuries
{

    public static class LessHeadInjuries
    {
        public static float ArmorHeadHitIgnoreDamageBelow = 10;
        public static float StructHeadHitIgnoreDamageBelow = 5;
        public static HashSet<Pilot> IgnoreNextHeadHit = new HashSet<Pilot>();

        public static void Init()
        {
            var harmony = HarmonyInstance.Create("Battletech.realitymachina.LessHeadInjuries");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void Reset()
        {
            IgnoreNextHeadHit = new HashSet<Pilot>();
        }
    }

    [HarmonyPatch(typeof(BattleTech.GameInstance), "LaunchContract", new Type[] { typeof(Contract), typeof(string) })]
    public static class BattleTech_GameInstance_LaunchContract_Patch
    {
        static void Postfix()
        {
            // reset on new contracts
            LessHeadInjuries.Reset();
        }
    }

    [HarmonyPatch(typeof(BattleTech.Mech), "DamageLocation")]
    public static class BattleTech_Mech_DamageLocation_Patch
    {
        static void Prefix(Mech __instance, int originalHitLoc, WeaponHitInfo hitInfo, ArmorLocation aLoc, Weapon weapon, float totalDamage, int hitIndex, AttackImpactQuality impactQuality)
        {
            if (aLoc == ArmorLocation.Head)
            {
                //we do some quick calculation of damage to see if it's an armor hit or an structure hit
                float currentArmor = __instance.GetCurrentArmor(aLoc);
                float remainingDamage = currentArmor - totalDamage;

                if (remainingDamage <= 0 && totalDamage < LessHeadInjuries.ArmorHeadHitIgnoreDamageBelow)
                {
                    //remainign damage less or equal to zero mean no structure penetration. Treat as an armour hit.
                    LessHeadInjuries.IgnoreNextHeadHit.Add(__instance.pilot);
                }
                else if (remainingDamage > 0 && totalDamage < LessHeadInjuries.StructHeadHitIgnoreDamageBelow)
                {
                    LessHeadInjuries.IgnoreNextHeadHit.Add(__instance.pilot);
                }

            }
        }
    }

    [HarmonyPatch(typeof(BattleTech.Pilot), "SetNeedsInjury")]
    public static class BattleTech_Pilot_SetNeedsInjury_Patch
    {
        static bool Prefix(Pilot __instance, InjuryReason reason)
        {
            if (reason == InjuryReason.HeadHit && LessHeadInjuries.IgnoreNextHeadHit.Contains(__instance))
            {
                LessHeadInjuries.IgnoreNextHeadHit.Remove(__instance);
                return false;
            }

            return true;
        }
    }

}
