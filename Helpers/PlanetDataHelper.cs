using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace ZorbsAlternativeStart.Helpers {
    public static class PlanetDataHelper {
        public static void SwapPlanets(ref PlanetData planet, ref PlanetData otherPlanet) {
            Debug.Log($"alternative start -- Swapping star.planet {planet.star.id}.{planet.id} with star.planet {otherPlanet.star.id}.{otherPlanet.id}");

            // Handle moons
            TransposeMoons(ref planet, ref otherPlanet);
            TransposeMoons(ref otherPlanet, ref planet);

            // Copy data from the other planet to the orbit index
            int oldIndex = planet.index;
            int oldOrbitIndex = planet.orbitIndex;
            int oldOrbitAround = planet.orbitAround;
            PlanetData oldOrbitAroundPlanet = planet.orbitAroundPlanet;
            float oldLuminosity = planet.luminosity;
            float oldOrbitRadius = planet.orbitRadius;
            float oldOrbitPhase = planet.orbitPhase;
            float oldOrbitInclination = planet.orbitInclination;
            float oldOrbitLongitude = planet.orbitLongitude;
            float oldSunDistance = planet.sunDistance;
            StarData oldStarData = planet.star;

            // Switch planets positions
            planet.index = otherPlanet.index;
            planet.orbitIndex = otherPlanet.orbitIndex;
            planet.orbitAround = otherPlanet.orbitAround;
            planet.orbitAroundPlanet = otherPlanet.orbitAroundPlanet;
            planet.orbitRadius = otherPlanet.orbitRadius;
            planet.orbitPhase = otherPlanet.orbitPhase;
            planet.orbitInclination = otherPlanet.orbitInclination;
            planet.orbitLongitude = otherPlanet.orbitLongitude;
            planet.sunDistance = otherPlanet.sunDistance;
            planet.star = otherPlanet.star;
            planet.luminosity = otherPlanet.luminosity;

            otherPlanet.index = oldIndex;
            otherPlanet.orbitIndex = oldOrbitIndex;
            otherPlanet.orbitAround = oldOrbitAround;
            otherPlanet.orbitAroundPlanet = oldOrbitAroundPlanet;
            otherPlanet.orbitRadius = oldOrbitRadius;
            otherPlanet.orbitPhase = oldOrbitPhase;
            otherPlanet.orbitInclination = oldOrbitInclination;
            otherPlanet.orbitLongitude = oldOrbitLongitude;
            otherPlanet.sunDistance = oldSunDistance;
            otherPlanet.star = oldStarData;
            otherPlanet.luminosity = oldLuminosity;
            
            // switch runtime locations?

            //ReIndexPlanets(planet.star);
            //ReIndexPlanets(otherPlanet.star);

            // Interstellar ops
            if ( planet.star != otherPlanet.star ) {
                Debug.LogWarning("planets star ids do not match");
                // Update stars planets array
                int planetIndex = -1;
                int otherPlanetIndex = -1;
                for ( int i = 0; i < planet.star.planets.Length; i++ ) {
                    if ( planet.star.planets[i].id == planet.id ) {
                        Debug.Log($"checking otherPlanet.star for {planet.star.planets[i].id} == {planet.id} ? {otherPlanet.star.planets[i].id == otherPlanet.id}");
                        Debug.LogWarning($"updating star {planet.star.id} planets array index {i} to be planet id {otherPlanet.id}");
                        planetIndex = i;
                        break;
                    }
                }
                for ( int i = 0; i < otherPlanet.star.planets.Length; i++ ) {
                    Debug.Log($"checking otherPlanet.star for {otherPlanet.star.planets[i].id} == {otherPlanet.id} ? {otherPlanet.star.planets[i].id == otherPlanet.id}");
                    if (otherPlanet.star.planets[i].id == otherPlanet.id) {
                        Debug.LogWarning($"updating star {otherPlanet.star.id} planets array index {i} to be planet id {planet.id}");
                        otherPlanetIndex = i;
                        break;
                    }
                }
                planet.star.planets[planetIndex] = otherPlanet;
                otherPlanet.star.planets[otherPlanetIndex] = planet;
                // Handle loading and unloading of planets
                //if ( planet.star.loaded ) {
                //    otherPlanet.Load();
                //    planet.Unload();
                //}
                //if (otherPlanet.star.loaded){
                //    planet.Load();
                //    otherPlanet.Unload();
                //}
                // TODO: Load and unload moons
            }
        }
        public static void TransposeMoons(ref PlanetData oldMoonLocation, ref PlanetData newMoonLocation) {
            for ( int i = 0; i < oldMoonLocation.star.planets.Length; i++ ) {
                PlanetData possibleMoon = oldMoonLocation.star.planets[i];
                if ( possibleMoon?.orbitAroundPlanet?.id == oldMoonLocation.id ) {
                    Debug.Log($"alternative start -- Transposing star.planet.moon {oldMoonLocation.star.id}.{oldMoonLocation?.orbitAroundPlanet?.id}.{oldMoonLocation.id} to star {newMoonLocation.star.id}");
                    possibleMoon.star = newMoonLocation.star;
                    possibleMoon.orbitAround = newMoonLocation.orbitIndex;
                    // Update solar energy ratio for moon
                    possibleMoon.sunDistance = newMoonLocation.sunDistance;
                    possibleMoon.luminosity = newMoonLocation.luminosity;
                }
            }
        }
        public static void StealMoon(ref PlanetData gru, ref PlanetData moon) {
            Debug.Log($"alternative start -- Stealing star.planet.moon {moon.star.id}.{moon?.orbitAroundPlanet?.id}.{moon.id} for star.planet {gru.star.id}.{gru.id}");
            // Transpose moons
            moon.star = gru.star;
            moon.orbitAround = gru.orbitIndex;
            moon.orbitAroundPlanet = gru;
            moon.sunDistance = gru.sunDistance;
            moon.luminosity = gru.luminosity;
            // TODO: Add a check for this moon orbit index already being occupied
        }
        public static void ReIndexPlanets(ref StarData star) {
            Debug.Log($"alternative start -- Reindexing star {star.id} planets");
            PlanetData[] planets = star.planets.Where(x => x.orbitAround == 0).ToArray();
            PlanetData[] moons = star.planets.Where(x => x.orbitalPeriod != 0).ToArray();
        }
        public static void ReOrbitPlanets(ref StarData star, Random seededRandom) {
            Debug.Log($"alternative start -- Reorbiting star {star.id} planets");

            //for ( int i = 0; i < star.planets.Length; i++ ) {
            //    PlanetData planetData = star.planets[i];
            //    if ( planetData.orbitAround == 0 ) {
            //        num16 = StarGen.orbitRadius[planetData.orbitIndex] * star.orbitScaler;
            //        float num17 = ( num15 - 1f ) / Mathf.Max(1f, num16) + 1f;
            //        num16 *= num17;
            //    }
            //    else {
            //        num16 = (float)( (double)( ( 1600f * (float)planetData.orbitIndex + 200f ) * Mathf.Pow(star.orbitScaler, 0.3f) * Mathf.Lerp(num15, 1f, 0.5f) + planetData.orbitAroundPlanet.realRadius ) / 40000.0 );
            //    }
            //}

        }
    }
}
