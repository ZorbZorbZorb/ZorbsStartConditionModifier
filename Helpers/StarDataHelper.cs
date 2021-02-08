using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ZorbsAlternateStart.Helpers {
    public static class StarDataHelper {
        public static void LogStarPlanetInfo(StarData star) {
            PlanetData[] planets = star.planets;
            string logString = $"Star {star.id} planet info:";
            for ( int i = 0; i < planets.Length; i++ ) {
                PlanetData planet = planets[i];
                logString += "\r\n";
                logString += $"planet id:{planet.id} ";
                logString += $"planet star id:{ planet.star.id} ";
                logString += $"orbitIndex:{ planet.orbitIndex} ";
                logString += $"typeString:{planet.typeString} ";
                logString += $"orbitsAround:{planet.orbitAround} ";
                logString += $"orbitAroundPlanet:{planet?.orbitAroundPlanet?.id}";
            }
            Debug.Log(logString);
        }
    }
}
