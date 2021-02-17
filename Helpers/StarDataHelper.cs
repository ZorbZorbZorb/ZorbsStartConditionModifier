using UnityEngine;

namespace ZorbsAlternativeStart.Helpers {
    public static class StarDataHelper {
        public static void LogStarPlanetInfo(StarData star) {
            PlanetData[] planets = star.planets;
            string logString = $"Star {star.id} planet info:";
            for ( int i = 0; i < planets.Length; i++ ) {
                PlanetData planet = planets[i];
                logString += "\r\n";
                logString += $"id {star.id}.{planet.id} ";
                logString += $"{planet.typeString} ";
                logString += $"type:{planet.type} ";
                logString += $"{planet.singularityString.Translate()} ";
                logString += $"orbitsAround:{planet.orbitAround} ";
                logString += $"orbitRadius:{planet.orbitRadius} ";
                logString += $"orbitIndex:{ planet.orbitIndex} ";
            }
            Debug.Log(logString);
        }
    }
}
