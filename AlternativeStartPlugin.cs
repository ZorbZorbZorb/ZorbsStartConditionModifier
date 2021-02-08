using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using BepInEx;
using BepInEx.Harmony;
using UnityEngine.UI;
using System.Reflection;
using ZorbsAlternateStart.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ZorbsAlternateStart {
    [BepInPlugin("org.bepinex.plugins.alternatestart", "Zorbs alternative start mod", "1.0.0.0")]
    [BepInProcess("DSPGAME.exe")]
    public class AlternativeStartPlugin : BaseUnityPlugin {
        // Apply all patches
        void Start() {
            Harmony.CreateAndPatchAll(typeof(AlternativeStartPlugin));
        }

        // All elligable home system planets are now desert planets, thats my attack
        [HarmonyPrefix, HarmonyPatch(typeof(PlanetGen), "SetPlanetTheme")]
        static void Patch(ref PlanetData planet, ref StarData star, ref GameDesc game_desc) {
            // Trick the game to spawn a barren planet as the inner most planet
            if ( planet.star.index == 0 && planet.type != EPlanetType.Ocean && planet.type != EPlanetType.Gas ) {
                Debug.Log($"alternatestart -- Forcing planet {planet.star.id}.{planet.id} to desert");
                planet.type = EPlanetType.Desert;
            }
        }

        // This mod works by moving planets at runtime
        [HarmonyPostfix, HarmonyPatch(typeof(UniverseGen), "CreateGalaxy")]
        static void Patch(ref GameDesc gameDesc, ref GalaxyData __result) {
            Debug.Log("alternatestart -- UniverseGen::CreateGalaxy()");

            PlanetData innerPlanet = null;
            PlanetData gasPlanet = null;
            PlanetData gasMoon = null;
            PlanetData outerPlanet = null;

            for ( int i = 0; i < __result.stars.Length; i++ ) {
                StarData star = __result.stars[i];
                // If starting star
                if (star.index == 0) {
                    StarDataHelper.LogStarPlanetInfo(star);
                    List<PlanetData> others = new List<PlanetData>();
                    for ( int j = 0; j < star.planets.Length; j++ ) {
                        PlanetData planet = star.planets[j];
                        // Logic to find the planets of the starting system
                        switch (planet.type) {
                            case EPlanetType.Ocean:
                                gasMoon = planet;
                                break;
                            case EPlanetType.Gas:
                                gasPlanet = planet;
                                break;
                            default:
                            others.Add(planet);
                                break;
                        }
                    }
                    // Shitcode to determine which planet is the inner and which planet is the outer planet.
                    int outerPlanetId = -1;
                    int innerPlanetId = -1;
                    if (others[0].orbitIndex > others[1].orbitIndex) {
                        outerPlanetId = others[0].id;
                        innerPlanetId = others[1].id;
                    }
                    else {
                        outerPlanetId = others[1].id;
                        innerPlanetId = others[0].id;
                    }
                    for ( int j = 0; j < star.planets.Length; j++ ) {
                        PlanetData planet = star.planets[j];
                        if (planet.id == outerPlanetId) {
                            outerPlanet = planet;
                        }
                        if (planet.id == innerPlanetId) {
                            innerPlanet = planet;
                        }
                    }
                    
                    //Move planets within the starter system
                    if (innerPlanet == null) {
                        Debug.LogError("alternatestart -- Failed to find inner planet");
                        return;
                    }
                    if ( outerPlanet == null ) {
                        Debug.LogError("alternatestart -- Failed to find outer planet");
                        return;
                    }
                    if ( gasPlanet == null ) {
                        Debug.LogError("alternatestart -- Failed to find gas planet");
                        return;
                    }
                    if ( gasMoon == null ) {
                        Debug.LogError("alternatestart -- Failed to find gas moon");
                        return;
                    }

                    // Swap the inner planets orbit with the gas giants moon
                    if (innerPlanet?.orbitAroundPlanet?.id != gasPlanet.id) {
                        PlanetDataHelper.SwapPlanets(ref innerPlanet, ref gasMoon);
                    }
                    else {
                        Debug.Log("alternatestart -- Inner planet already swapped with gas moon.");
                    }
                    // Steal the gas giants moon for the inner planet
                    if (innerPlanet?.orbitAroundPlanet?.id != gasMoon.id) {
                        PlanetDataHelper.StealMoon(ref gasMoon, ref innerPlanet);
                    }
                    else {
                        Debug.Log("alternatestart -- Gas moon already orbiting inner planet.");
                    }
                    // Move the gas giant to the outer most orbit
                    if ( gasPlanet.orbitIndex < outerPlanet.orbitIndex ) {
                        PlanetDataHelper.SwapPlanets(ref outerPlanet, ref gasPlanet);
                    }
                    else {
                        Debug.Log("alternatestart -- Gas giant already in outer most orbit.");
                    }
                    // Log new system configuration
                    //StarDataHelper.LogStarPlanetInfo(star);
                }
            }
        }
    }
}
