using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace ZorbsAlternativeStart.Patches {
    [HarmonyPatch(typeof(PlanetGen))]
    public class LegacyPatchPlanetGen {
        /// <summary>
        /// Patches set planet theme for the starting star system. 
        /// Sets any non-gas non-birth planets as desert planets.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch("SetPlanetTheme")]
        static void SetPlanetTheme(ref PlanetData planet, ref StarData star, ref GameDesc game_desc) {
            // Trick the game to spawn a barren planet as the other planets
            if ( planet.star.index == 0 && planet.type != EPlanetType.Ocean && planet.type != EPlanetType.Gas ) {
                Debug.Log($"alternatestart -- Forcing planet {planet.star.id}.{planet.id} to desert");
                planet.type = EPlanetType.Desert;
            }
        }
    }
}
