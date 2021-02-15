﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using ZorbsAlternativeStart.Helpers;

namespace ZorbsAlternativeStart.Patches {
    [HarmonyPatch(typeof(UniverseGen))]
    public class PatchUniverseGen {
        private static readonly bool enabled = true;

        [HarmonyPostfix]
        [HarmonyPatch("CreateGalaxy")]
        // Move planets at runtime. This is the safest way (for your save files) to modify the planets, but also the buggiest.
        static void CreateGalaxy(ref GameDesc gameDesc, ref GalaxyData __result) {
            GalaxyDataHelper.RepairBirthIds(ref __result);

            if ( !enabled ) {
                return;
            }

            GalaxyData galaxy = __result;
            List<PlanetData> gasPlanets = new List<PlanetData>();
            List<PlanetData> otherPlanets = new List<PlanetData>();
            List<PlanetData> moons = new List<PlanetData>();
            List<PlanetData> newMoons = new List<PlanetData>();
            PlanetData birthPlanet = null;
            int birthPlanetId = __result.birthPlanetId;
            bool tooManyPlanets = false;
            bool peacockSystem = false;

            // Find starting system
            for ( int i = 0; i < __result.stars.Length; i++ ) {
                StarData star = __result.stars[i];
                // If starting star
                if ( star.index == 0 ) {
                    if (AlternativeStartPlugin.zDebuggingMode)
                        StarDataHelper.LogStarPlanetInfo(star);
                    StartModification(ref star);
                    FinalizeModification(ref star);
                    // Log new system configuration
                    if ( AlternativeStartPlugin.zDebuggingMode )
                        StarDataHelper.LogStarPlanetInfo(star);
                }
            }

            void StartModification(ref StarData star) {
                for ( int j = 0; j < star.planets.Length; j++ ) {
                    PlanetData planet = star.planets[j];
                    // Sort planets into lists
                    if ( birthPlanetId == planet.id ) {
                        birthPlanet = planet;
                        continue;
                    }
                    else if ( planet.type == EPlanetType.Gas ) {
                        gasPlanets.Add(planet);
                        continue;
                    }
                    else if ( planet.orbitAroundPlanet != null ) {
                        moons.Add(planet);
                        continue;
                    }
                    else {
                        otherPlanets.Add(planet);
                    }
                }


                // Additional checks for modded games, and extremly rare starting system configurations.
                if ( birthPlanet == null ) {
                    Debug.LogError("alternatestart -- Failed to find birth planet");
                    return;
                }
                if ( otherPlanets.Count() + moons.Count() > 2 ) {
                    tooManyPlanets = true;
                    Debug.LogWarning("alternatestart -- Modded starting system detected");
                }
                if ( otherPlanets.Count() == 0 && moons.Count() == 0 ) {
                    Debug.LogWarning("alternatestart -- Not enough planets to change starting system");
                    return;
                }
                if ( otherPlanets.Count() == 0 && moons.Count() > 0 ) {
                    peacockSystem = true;
                    Debug.LogWarning("alternatestart -- Peacock system detected");
                    if ( tooManyPlanets ) {
                        Debug.LogWarning("alternatestart -- System is very strange");
                        if ( gasPlanets.Count() == 0 ) {
                            Debug.LogError("alternatestart -- Not possible to modify starting system. No other planets except birth planet.");
                            return;
                        }

                        return;
                    }
                }

                // Prior run detection;
                if ( birthPlanet.orbitAroundPlanet == null ) {
                    Debug.LogWarning("alternatestart -- System was already modified");
                    return;
                }

                Debug.Log("alternatestart -- Started system modification");
                if ( peacockSystem ) {
                    MoveSystemPeacock();
                }
                else {
                    MoveSystemNormal();
                }
            }

            void FinalizeModification(ref StarData star) {
                CorrectOrbits();
                for ( int i = 0; i < gasPlanets.Count(); i++ ) {
                    PlanetData gasPlanet = gasPlanets[i];
                    RevokeSingularities(ref gasPlanet);
                }
                if ( newMoons.Count > 1 ) {
                    // The starting planet having multiple moons and horizontal rotation is rare and might be cool.

                    //birthPlanet.singularity = EPlanetSingularity.MultipleSatellites;
                    //Debug.Log($"alternatestart -- Added plural satellites singularity to planet {birthPlanet.id}");
                }
            }

            void MoveSystemPeacock() {
                birthPlanet.orbitRadius = 0.6f;
                birthPlanet.orbitInclination *= 1.2f;
                birthPlanet.orbitAroundPlanet = null;
                birthPlanet.orbitAround = 0;
                birthPlanet.orbitIndex = 1;

                for ( int i = 0; i < moons.Count(); i++ ) {
                    PlanetData moon = moons[i];
                    PlanetDataHelper.StealMoon(ref birthPlanet, ref moon);
                    newMoons.Add(moon);
                    if ( newMoons.Count() >= 2 ) {
                        Debug.Log("alternatestart -- Finished swapping planets");
                        CorrectOrbits();
                        return;
                    }
                }
            }

            void MoveSystemNormal() {
                PlanetData lowest = otherPlanets.OrderBy(x => x.orbitRadius).First();
                //Debug.Log($"alternatestart -- The lowest orbit radius planet was {lowest.id}");
                PlanetDataHelper.SwapPlanets(ref lowest, ref birthPlanet);
                PlanetDataHelper.StealMoon(ref birthPlanet, ref lowest);
                newMoons.Add(lowest);
                for ( int i = 0; i < moons.Count(); i++ ) {
                    PlanetData moon = moons[i];
                    if ( moon.orbitAround != birthPlanet.id && !newMoons.Contains(moon) ) {
                        PlanetDataHelper.StealMoon(ref birthPlanet, ref moon);
                        newMoons.Add(moon);
                        if ( newMoons.Count() >= 2 ) {
                            CorrectOrbits();
                            return;
                        }
                    }

                }
                Debug.Log("alternatestart -- Finished swapping planets");
            }

            void CorrectOrbits() {
                // Order the moons by orbit radius, lowest to highest
                Debug.Log("alternatestart -- Started correcting orbits");
                List<PlanetData> orderedMoons = new List<PlanetData>();
                for ( int i = 0; i < newMoons.Count(); i++ ) {
                    PlanetData lowest = newMoons[i];
                    for ( int j = 0; j < newMoons.Count(); j++ ) {
                        if ( orderedMoons.Contains(newMoons[j]) ) {
                            continue;
                        }
                        if ( lowest.orbitRadius > newMoons[j].orbitRadius ) {
                            lowest = newMoons[j];
                        }
                    }
                    orderedMoons.Add(lowest);
                }

                for ( int i = 0; i < newMoons.Count() - 1; i++ ) {
                    PlanetData moon = newMoons[i];
                    PlanetData nextMoon = newMoons[i + 1];
                    float difference = nextMoon.orbitRadius - moon.orbitRadius;
                    if (AlternativeStartPlugin.zDebuggingMode) {
                        Debug.Log(moon.orbitRadius);
                        Debug.Log(nextMoon.orbitRadius);
                        Debug.Log("    " + difference);
                    }
                }
                Debug.Log("alternatestart -- Finished correcting orbits");
            }

            // Remove any dangling multiple moon singularities from gas giants that no longer deserve them
            void RevokeSingularities(ref PlanetData targetPlanet) {
                int moonCount = 0;
                for ( int i = 0; i < targetPlanet.star.planets.Count(); i++ ) {
                    PlanetData planet = targetPlanet.star.planets[i];
                    if ( planet.orbitAround == targetPlanet.id ) {
                        if ( moonCount++ > 1 ) {
                            return;
                        }
                    }
                }
                if ( targetPlanet.singularity == EPlanetSingularity.MultipleSatellites ) {
                    Debug.Log($"alternatestart -- Removed plural satellites singularity from planet {targetPlanet.id}");
                    targetPlanet.singularity = EPlanetSingularity.None;
                }
            }
        }
    }
}