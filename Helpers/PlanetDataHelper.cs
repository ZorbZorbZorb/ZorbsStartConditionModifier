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
            //int oldIndex = planet.index;
            //int oldOrbitIndex = planet.orbitIndex;
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
            //planet.index = otherPlanet.index;
            //planet.orbitIndex = otherPlanet.orbitIndex;
            planet.orbitAround = otherPlanet.orbitAround;
            planet.orbitAroundPlanet = otherPlanet.orbitAroundPlanet;
            planet.orbitRadius = otherPlanet.orbitRadius;
            planet.orbitPhase = otherPlanet.orbitPhase;
            planet.orbitInclination = otherPlanet.orbitInclination;
            planet.orbitLongitude = otherPlanet.orbitLongitude;
            planet.sunDistance = otherPlanet.sunDistance;
            planet.star = otherPlanet.star;
            planet.luminosity = otherPlanet.luminosity;

            //otherPlanet.index = oldIndex;
            //otherPlanet.orbitIndex = oldOrbitIndex;
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
        public static List<PlanetData> GetMoons(PlanetData planet) {
            List<PlanetData> moons = new List<PlanetData>();
            for ( int i = 0; i < planet.star.planets.Length; i++ ) {
                PlanetData moon = planet.star.planets[i];
                if ( moon.orbitAroundPlanet == planet ) {
                    moons.Add(moon);
                }
            }
            return moons;
        }
        public static List<PlanetData> GetPlanetsWithMoons(StarData star) {
            return star.planets.Where(x => star.planets.Where(y => y.orbitAround == x.id).Count() > 0).ToList();
        }
        public static void ReIndexPlanets(ref StarData star, int startingPlanetIndex = 1, int step = 1) {
            Debug.Log($"alternative start -- Started Reindexing star {star.id} planets");
            PlanetData[] planets = star.planets.Where(x => x.orbitAroundPlanet == null).OrderBy(x => x.orbitRadius).ToArray();
            Dictionary<PlanetData, List<PlanetData>> planetsAndMoons = new Dictionary<PlanetData, List<PlanetData>>();

            // Debug info
            //Debug.Log("planets: " + string.Join(",", planets.Select(x => x.id.ToString()).ToArray()));
            //Debug.Log("planets with moons: " + string.Join(",", planetsAndMoons.Select(x => x.Key.id.ToString()).ToArray()));
            //planetsAndMoons.ToList().ForEach(x => Debug.Log($"moons of {x.Key.id}: {string.Join(",", x.Value.Select(y => y.id.ToString()).ToArray())}"));

            int orbitIndex = startingPlanetIndex;
            //int index = 1;
            for ( int i = 0; i < planets.Length; i++ ) {
                PlanetData planet = planets[i];
                //planet.index = index++;
                planet.orbitIndex = orbitIndex++;
                //Debug.LogWarning($"{planet.id} is a planet and received OI {planet.orbitIndex}");
                List<PlanetData> moons = GetMoons(planet);
                int moonIndex = 1;
                while(true) {
                    if (moons.Count() < 1) {
                        break;
                    }
                    PlanetData moon = moons.OrderBy(x => x.orbitRadius).First();
                    moons.Remove(moon);
                    //moon.index = index++;
                    moon.orbitIndex = moonIndex++;
                    //Debug.LogWarning($"{moon.id} is a moon of {planet.id} and received OI {moon.orbitIndex}");
                }
            }
            Debug.Log($"alternative start -- Finished Reindexing star {star.id} planets");
        }
        public static void ReOrbitPlanets(ref StarData star, Random seededRandom) {
            Debug.Log($"alternative start -- Reorbiting star {star.id} planets");

            for ( int i = 0; i < star.planets.Length; i++ ) {
                double num2 = seededRandom.NextDouble();
                double num3 = seededRandom.NextDouble();
                double num5 = seededRandom.NextDouble();
                float num15 = Mathf.Pow(1.2f, (float)( num2 * ( num3 - 0.5 ) * 0.5 ));
                float num16 = 0f;

                PlanetData planetData = star.planets[i];

                // Calculate orbit radius
                if ( planetData.orbitAround == 0 ) {
                    num16 = StarGen.orbitRadius[planetData.orbitIndex] * star.orbitScaler;
                    float num17 = ( num15 - 1f ) / Mathf.Max(1f, num16) + 1f;
                    num16 *= num17;
                }
                else {
                    num16 = (float)( (double)( ( 1600f * (float)planetData.orbitIndex + 200f ) * Mathf.Pow(star.orbitScaler, 0.3f) * Mathf.Lerp(num15, 1f, 0.5f) + planetData.orbitAroundPlanet.realRadius ) / 40000.0 );
                }

                // Correct radius, inclination and longitude
                planetData.orbitRadius = num16;
                planetData.orbitInclination = (float)( seededRandom.NextDouble() * 16.0 - 8.0 );
                if ( planetData.orbitAround > 0 ) {
                    planetData.orbitInclination *= 2.2f;
                }
                planetData.orbitLongitude = (float)( num5 * 360.0 );

                // Correct orbital period
                if ( planetData.orbitAroundPlanet == null ) {
                    planetData.orbitalPeriod = Math.Sqrt(39.478417604357432 * (double)num16 * (double)num16 * (double)num16 / ( 1.3538551990520382E-06 * (double)star.mass ));
                }
                else {
                    planetData.orbitalPeriod = Math.Sqrt(39.478417604357432 * (double)num16 * (double)num16 * (double)num16 / 1.0830842106853677E-08);
                }

                // Correct sun distance
                planetData.sunDistance = ( ( planetData.orbitAround != 0 ) ? planetData.orbitAroundPlanet.orbitRadius : planetData.orbitRadius );

                // Correct luminosity
                if (planetData.orbitAroundPlanet == null) {
                    planetData.luminosity = Mathf.Pow(planetData.star.lightBalanceRadius / ( planetData.sunDistance + 0.01f ), 0.6f);
                    if ( planetData.luminosity > 1f ) {
                        planetData.luminosity = Mathf.Log(planetData.luminosity) + 1f;
                        planetData.luminosity = Mathf.Log(planetData.luminosity) + 1f;
                        planetData.luminosity = Mathf.Log(planetData.luminosity) + 1f;
                    }
                }
            }

            // Fix moon luminosity.
            star.planets.Where(x => x.orbitAroundPlanet != null).ToList().ForEach(x => x.luminosity = x.orbitAroundPlanet.luminosity);

        }
    }
}
