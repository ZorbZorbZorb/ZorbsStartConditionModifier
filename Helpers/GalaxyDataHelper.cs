using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace ZorbsAlternativeStart.Helpers {
    public static class GalaxyDataHelper {
        public static void RepairBirthIds(ref GalaxyData galaxy) {
            // Repair bad birth star
            if ( galaxy.birthStarId == 0 ) {
                galaxy.birthStarId = 1;
                Debug.LogWarning($"alternatestart -- WARNING: Reassigned birth star id to {galaxy.birthStarId}");
            }
            // Repair bad birth planet
            if ( galaxy.birthPlanetId == 0 ) {
                StarData birthStar = galaxy.StarById(galaxy.birthStarId);
                for ( int i = 0; i < birthStar.planets.Length; i++ ) {
                    // Pick the first or only ocean planet
                    if ( birthStar.planets[i].type == EPlanetType.Ocean ) {
                        galaxy.birthPlanetId = birthStar.planets[i].id;
                        Debug.LogWarning($"alternatestart -- WARNING: Reassigned birth planet id to {galaxy.birthPlanetId}");
                    }
                }
                if ( galaxy.birthPlanetId == 0 ) {
                    // Panic and choose any valid planet
                    int? newBirthPlanet = birthStar.planets.FirstOrDefault(x => x.type != EPlanetType.Gas).id;
                    if ( newBirthPlanet == null ) {
                        throw new Exception("Alternatestart: No birth planet or candidates for birth planet found.");
                    }
                    galaxy.birthPlanetId = (int)newBirthPlanet;
                    Debug.LogError($"alternatestart -- DANGER: Reassigned birth planet id to non-ocean planet {galaxy.birthPlanetId}");
                }
            }
        }
    }
}
