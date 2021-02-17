using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using ZorbsAlternativeStart.Extensions;
using ZorbsAlternativeStart.Helpers;
using Random = System.Random;

namespace ZorbsAlternativeStart.Patches {
    [HarmonyPatch(typeof(UniverseGen))]
    public class PatchUniverseGen {

        private static Random seededRandom;
        private static string arrangementString = string.Empty;

        [HarmonyPostfix]
        [HarmonyPatch("CreateGalaxy")]
        // Move planets at runtime. This is the safest way (for your save files) to modify the planets, but also the buggiest.
        static void CreateGalaxy(ref GameDesc gameDesc, ref GalaxyData __result) {
            GalaxyDataHelper.RepairBirthIds(ref __result);

            // Seed rng from galaxy seed for deterministic randomness
            int seed = gameDesc.galaxySeed;
            seededRandom = new Random(gameDesc.galaxySeed);
            arrangementString = null;
            
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
                    if (AlternativeStartPlugin.zDebuggingMode) {
                        StarDataHelper.LogStarPlanetInfo(star);
                    }
                    float newLevel = seededRandom.Next(2, 50) / 100f;
                    EStarType needtype = EStarType.MainSeqStar;
                    ESpectrType needSpectr = ESpectrType.G;
                    if (gameDesc.galaxySeed == 16161616) {
                        Debug.LogWarning($"Using anomaly seed {gameDesc.galaxySeed}");
                        newLevel = 1f;
                        needtype = EStarType.BlackHole;
                        needSpectr = ESpectrType.X;
                    }
                    ModifyStar(ref star, newLevel, needtype, needSpectr);
                    arrangementString += $"{(int)star.type}{star.spectr.ToString()[0]}";
                    StartModification(ref star);
                    FinalizeModification(ref star);
                    // Log new system configuration
                    if ( AlternativeStartPlugin.zDebuggingMode ) {
                        StarDataHelper.LogStarPlanetInfo(star);
                    }
                }
            }

            void ModifyStar(ref StarData star, float newLevel, EStarType needtype, ESpectrType needSpectr) {
                Debug.Log($"Started modifying birth star to level {newLevel}f");

                // Possibly allow starting systems to be a giant M-class?

                star.spectr = needSpectr;

                double num3 = seededRandom.NextDouble();
                double num4 = seededRandom.NextDouble();
                double numStarAgeRandomizer = seededRandom.NextDouble();
                double num6 = ( seededRandom.NextDouble() - 0.5 ) * 0.2;
                double num7 = seededRandom.NextDouble() * 0.2 + 0.9;
                double num8 = seededRandom.NextDouble() * 0.4 - 0.2;
                double num9 = Math.Pow(2.0, num8);
                // modification to allow better class stars
                float num10 = Mathf.Lerp(-0.98f, 0.88f, newLevel);
                num10 = ( ( !( num10 < 0f ) ) ? ( num10 + 0.65f ) : ( num10 - 0.65f ) );
                float standardDeviation = 0.33f;
                float num11 = StarGen.RandNormal(num10, standardDeviation, num3, num4);

                switch ( needSpectr ) {
                    case ESpectrType.M:
                    num11 = -3f;
                    break;
                    case ESpectrType.O:
                    num11 = 3f;
                    break;
                }

                num11 = ( ( !( num11 > 0f ) ) ? ( num11 * 1f ) : ( num11 * 2f ) );
                num11 = Mathf.Clamp(num11, -2.4f, 4.65f) + (float)num6 + 1f;

                star.mass = Mathf.Pow(2f, num11);

                double d = 5.0;

                if ( star.mass < 2f ) {
                    d = 2.0 + 0.4 * ( 1.0 - (double)star.mass );
                }
                star.lifetime = (float)( 10000.0 * Math.Pow(0.1, Math.Log10((double)star.mass * 0.5) / Math.Log10(d) + 1.0) * num7 );


                switch ( needtype ) {
                    case EStarType.BlackHole:
                    star.mass = 18f + (float)( num3 * num4 ) * 30f;
                    break;
                    case EStarType.GiantStar:
                    star.lifetime = (float)( 10000.0 * Math.Pow(0.1, Math.Log10((double)star.mass * 0.58) / Math.Log10(d) + 1.0) * num7 );
                    star.age = (float)numStarAgeRandomizer * 0.04f + 0.96f;
                    break;
                    default:
                    if ( (double)star.mass < 0.5 ) {
                        star.age = (float)numStarAgeRandomizer * 0.12f + 0.02f;
                    }
                    else if ( (double)star.mass < 0.8 ) {
                        star.age = (float)numStarAgeRandomizer * 0.4f + 0.1f;
                    }
                    else {
                        star.age = (float)numStarAgeRandomizer * 0.7f + 0.2f;
                    }
                    break;
                }

                if (needtype == EStarType.BlackHole) {
			        star.age = (float)numStarAgeRandomizer * 0.4f + 1f;
                }

                float num12 = star.lifetime * star.age;
                if ( num12 > 5000f ) {
                    num12 = ( Mathf.Log(num12 / 5000f) + 1f ) * 5000f;
                }
                if ( num12 > 8000f ) {
                    float f = num12 / 8000f;
                    f = Mathf.Log(f) + 1f;
                    f = Mathf.Log(f) + 1f;
                    f = Mathf.Log(f) + 1f;
                    num12 = f * 8000f;
                }
                star.lifetime = num12 / star.age;
                float num13 = ( 1f - Mathf.Pow(Mathf.Clamp01(star.age), 20f) * 0.5f ) * star.mass;
                star.temperature = (float)( Math.Pow(num13, 0.56 + 0.14 / ( Math.Log10(num13 + 4f) / Math.Log10(5.0) )) * 4450.0 + 1300.0 );
                double num14 = Math.Log10(( (double)star.temperature - 1300.0 ) / 4500.0) / Math.Log10(2.6) - 0.5;
                if ( num14 < 0.0 ) {
                    num14 *= 4.0;
                }
                if ( num14 > 2.0 ) {
                    num14 = 2.0;
                }
                else if ( num14 < -4.0 ) {
                    num14 = -4.0;
                }
                star.spectr = (ESpectrType)Mathf.RoundToInt((float)num14 + 4f);
                star.color = Mathf.Clamp01(( (float)num14 + 3.5f ) * 0.2f);
                star.classFactor = (float)num14;
                star.luminosity = Mathf.Pow(num13, 0.7f);
                star.radius = (float)( Math.Pow(star.mass, 0.4) * num9 );
                star.acdiskRadius = 0f;
                float p = (float)num14 + 2f;
                star.habitableRadius = Mathf.Pow(1.7f, p) + 0.25f * Mathf.Min(1f, star.orbitScaler);
                star.lightBalanceRadius = Mathf.Pow(1.7f, p);
                star.orbitScaler = Mathf.Pow(1.35f, p);
                if ( star.orbitScaler < 1f ) {
                    star.orbitScaler = Mathf.Lerp(star.orbitScaler, 1f, 0.6f);
                }

                double rn = seededRandom.NextDouble();
                double rt = seededRandom.NextDouble();
                StarGen.SetStarAge(star, star.age, rn, rt);

                star.dysonRadius = star.orbitScaler * 0.28f;
                if ( (double)star.dysonRadius * 40000.0 < (double)( star.physicsRadius * 1.5f ) ) {
                    star.dysonRadius = (float)( (double)( star.physicsRadius * 1.5f ) / 40000.0 );
                }
                star.uPosition = star.position * 2400000.0;

                Debug.Log($"Finished modifying birth star. Now a {star.spectr} class {star.type}...");
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
                    Debug.LogWarning("alternatestart -- System was already modified?");
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
                for ( int i = 0; i < gasPlanets.Count(); i++ ) {
                    PlanetData gasPlanet = gasPlanets[i];
                    RevokeSingularities(ref gasPlanet);
                }
                if ( newMoons.Count > 1 ) {
                    // The starting planet having multiple moons and horizontal rotation is rare and might be cool.

                    //birthPlanet.singularity = EPlanetSingularity.MultipleSatellites;
                    //Debug.Log($"alternatestart -- Added plural satellites singularity to planet {birthPlanet.id}");
                }

                int skip = 1;
                int step = 1;
                switch ( star.spectr ) {
                    case ESpectrType.M:
                    case ESpectrType.K:
                    break;
                    case ESpectrType.G: 
                    case ESpectrType.F:
                    skip = 2;
                    break;
                    case ESpectrType.A:
                    skip = 2;
                    step = 2;
                    break;
                    case ESpectrType.B:
                    skip = 3;
                    step = 2;
                    break;
                    case ESpectrType.O:
                    case ESpectrType.X:
                    skip = 4;
                    step = 2;
                    break;
                }

                PlanetDataHelper.ReIndexPlanets(ref star, skip, step);
                PlanetDataHelper.ReOrbitPlanets(ref star, seededRandom);

                if (star.type == EStarType.BlackHole) {
                    star.planets.Select(x => x.luminosity *= 0.66f);
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
                        Debug.Log("alternatestart -- Birth planet has maximum allowed moons");
                        break;
                    }
                }

                Debug.LogWarning($"alternatestart -- New System arrangement is {birthPlanet.theme}E");
            }

            void MoveSystemNormal() {
                double num = seededRandom.NextDouble();
                double num2 = seededRandom.NextDouble();
                double num3 = seededRandom.NextDouble();
                PlanetData lowest = otherPlanets.OrderBy(x => x.orbitRadius).First();
                PlanetData highest = otherPlanets.OrderBy(x => x.orbitRadius).Last();

                // How should the system be arranged?
                if (num > 0.8f) {
                    arrangementString += 'C';
                    PlanetDataHelper.SwapPlanets(ref highest, ref birthPlanet);
                }
                else if (num > 0.6f) {
                    arrangementString += 'B';
                    PlanetDataHelper.SwapPlanets(ref lowest, ref birthPlanet);
                }
                else {
                    if (gasPlanets.Count() > 0) {
                        arrangementString += 'A';
                        PlanetData gasPlanet = gasPlanets[0];
                        PlanetDataHelper.SwapPlanets(ref lowest, ref birthPlanet);
                        PlanetDataHelper.SwapPlanets(ref gasPlanet, ref birthPlanet);
                    }
                    else {
                        PlanetDataHelper.SwapPlanets(ref lowest, ref birthPlanet);
                        arrangementString += 'D';
                    }
                }
                if (num2 > 0.30d) {
                    arrangementString += 'A';
                    PlanetDataHelper.StealMoon(ref birthPlanet, ref lowest);
                    newMoons.Add(lowest);
                    for ( int i = 0; i < moons.Count(); i++ ) {
                        PlanetData moon = moons[i];
                        if ( moon.orbitAround != birthPlanet.id && !newMoons.Contains(moon) ) {
                            PlanetDataHelper.StealMoon(ref birthPlanet, ref moon);
                            newMoons.Add(moon);
                            if ( newMoons.Count() >= 2 ) {
                                Debug.Log("alternatestart -- Birth planet has maximum allowed moons");
                                break;
                            }
                        }

                    }
                }
                else {
                    arrangementString += 'B';
                }

                // Orphan a gas giant moon?
                if (num3 > 0.6d) {
                    bool demooned = false;
                    for ( int i = 0; i < birthPlanet.star.planets.Length; i++ ) {
                        PlanetData moon = birthPlanet.star.planets[i];
                        if (moon.orbitAround != 0 && moon.orbitalPeriod != birthPlanet.id) {
                            moon.orbitRadius = moon.orbitAroundPlanet.orbitRadius * 1.2f;
                            moon.orbitAroundPlanet = null;
                            moon.orbitAround = 0;
                            demooned = true;
                            arrangementString += 'A';
                            break;
                        }
                    }
                    if (!demooned) {
                        arrangementString += 'B';
                    }
                }
                else {
                    arrangementString += 'B';
                }

                Debug.LogWarning($"alternatestart -- New System arrangement is {arrangementString}");
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
