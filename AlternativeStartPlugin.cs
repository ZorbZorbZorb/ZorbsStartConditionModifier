using BepInEx;
using HarmonyLib;
using System.Security;
using System.Security.Permissions;
using ZorbsAlternativeStart.Patches;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ZorbsAlternativeStart {
    [BepInPlugin("org.bepinex.plugins.alternatestart", "Zorbs alternative start mod", "1.3.1")]
    [BepInProcess("DSPGAME.exe")]
    public class AlternativeStartPlugin : BaseUnityPlugin {
        public static bool zDebuggingMode = true;
        internal void Awake() {
            Harmony harmony = new Harmony("zorb.dsp.plugins.alternativestart");
            
            Harmony.CreateAndPatchAll(typeof(PatchPlanetGen), harmony.Id);
            //Harmony.CreateAndPatchAll(typeof(PatchStarGen), harmony.Id);
            Harmony.CreateAndPatchAll(typeof(PatchUniverseGen), harmony.Id);
        }
    }
}
