using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using ZorbsAlternativeStart.Extensions;

namespace ZorbsAlternativeStart.Patches {
	[HarmonyPatch(typeof(PlanetGen))]
	public class PatchPlanetGen {
		public static readonly int[] newSpawnPlanetTypes = { 1, 8, 14 };

		[HarmonyPrefix]
		[HarmonyPatch("SetPlanetTheme")]
		static bool SetPlanetTheme(
			ref PlanetData planet, ref StarData star, ref GameDesc game_desc, ref int set_theme, ref int set_algo,
			ref double rand1, ref double rand2, ref double rand3, ref double rand4, ref int theme_seed) {
			// Change the theme algorithm for the starting star system
			if ( star.id == 1 ) {
				if ( planet.type == EPlanetType.Ocean ) {
					int newType;
					// 35% chance for a oceanic jungle
					if (rand1 > 0.65d) {
						newType = 8;
					}
					// 35% chance for a red stone
					else if (rand1 > 0.30d) {
						newType = 14;
                    }
					// 20% chance for a prarie
					else if (rand1 > 0.10d) {
						newType = 15;
                    }
					// 10% chance for a terran
					else {
						newType = 1;
                    }
					ChangePlanetTheme(ref planet, newType, rand2, rand3, rand4);
					return false;
				}
				else if ( planet.type == EPlanetType.Gas ) {

				}
				else {

				}
			}
			return true;
		}

		public static void ChangePlanetTheme(ref PlanetData planet, int type, double rand2, double rand3, double rand4) {
			ThemeProto theme = LDB.themes.Select(type);
			if ( theme == null ) {
				Debug.LogError($"alternative start -- FATAL: Failed to find theme '{type}'");
				return;
			}
			planet.theme = type;
			planet.type = theme.PlanetType;
			planet.ionHeight = theme.IonHeight;
			planet.windStrength = theme.Wind;
			planet.waterHeight = theme.WaterHeight;
			planet.waterItemId = theme.WaterItemId;
			planet.levelized = theme.UseHeightForBuild;
			planet.algoId = theme.Algos[(int)( rand2 * (double)theme.Algos.Length ) % theme.Algos.Length];
			planet.mod_x = (double)theme.ModX.x + rand3 * (double)( theme.ModX.y - theme.ModX.x );
			planet.mod_y = (double)theme.ModY.x + rand4 * (double)( theme.ModY.y - theme.ModY.x );
		}
	}
}
