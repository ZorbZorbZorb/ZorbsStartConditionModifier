using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace ZorbsAlternativeStart.Patches {
    [HarmonyPatch(typeof(PlanetGen))]
    public class PatchPlanetGen {
        [HarmonyPrefix]
        [HarmonyPatch("SetPlanetTheme")]
        static bool SetPlanetTheme(
			ref PlanetData planet, ref StarData star, ref GameDesc game_desc, ref int set_theme, ref int set_algo, 
			ref double rand1, ref double rand2, ref double rand3, ref double rand4, ref int theme_seed) {
			// Change the theme algorithm for the starting star system
			if (star.id == 1) {
				if (ModifiedBehavior(ref planet, ref star, ref game_desc, ref set_theme, ref set_algo, ref rand1, ref rand2, ref rand3, ref rand4, ref theme_seed)) {
					// Return to prevent the call of the original method
					return false;
                }
			}
			// If this is not the starting star system, or the modified behavior encountered an error, run the default behavior.
			return true;
		}

		/// <summary>
		/// Modified set theme behavior. 
		/// Returns true on success and false on failure
		/// </summary>
		/// <returns>Success or failure</returns>
		static bool ModifiedBehavior(
			ref PlanetData planet, ref StarData star, ref GameDesc game_desc, ref int set_theme, ref int set_algo,
			ref double rand1, ref double rand2, ref double rand3, ref double rand4, ref int theme_seed) {
			
			// "Flag" is not an appropriate flag name.
			bool flagIsBirthPlanet = false;

			// Set the planets theme
			if ( set_theme > 0 ) {
				planet.theme = set_theme;
			}
			else {
				if ( PlanetGen.tmp_theme == null ) {
					PlanetGen.tmp_theme = new List<int>();
				}
				else {
					PlanetGen.tmp_theme.Clear();
				}
				int[] themeIds = game_desc.themeIds;
				int num = themeIds.Length;

				// Choose possible themes for part 1
				for ( int i = 0; i < num; i++ ) {
					ThemeProto themeProto = LDB.themes.Select(themeIds[i]);
					
					bool flag = false;

					// If spawn planet
					if ( planet.star.index == 0 && planet.type == EPlanetType.Ocean ) {
                        if ( themeProto.Distribute == EThemeDistribute.Birth ) {
							// Add default, ocean jungle or red stone and break the loop
							Debug.Log($"alternative start -- Detected the starting planet to be {planet.id}")l
							PlanetGen.tmp_theme.Add(1);
							PlanetGen.tmp_theme.Add(8);
							PlanetGen.tmp_theme.Add(14);
							flagIsBirthPlanet = true;
							break;
						}
                    }

					else if ( themeProto.PlanetType == planet.type && themeProto.Temperature * planet.temperatureBias >= -0.1f ) {
						if ( planet.star.index == 0 ) {
							if ( themeProto.Distribute == EThemeDistribute.Default ) {
								flag = true;
							}
						}
						else if ( themeProto.Distribute != EThemeDistribute.Birth ) {
							flag = true;
						}
					}
					if ( flag ) {
						for ( int j = 0; j < planet.index; j++ ) {
							if ( planet.star.planets[j].theme == themeProto.ID ) {
								flag = false;
								break;
							}
						}
					}
					if ( flag ) {
						PlanetGen.tmp_theme.Add(themeProto.ID);
					}
				}

				// If no themes are available, add all possible desert themes to the pool
				if ( PlanetGen.tmp_theme.Count == 0 ) {
					//Debug.LogWarning($"a planetgen tmp_theme count was 0 on planet {planet.id}");
					for ( int k = 0; k < num; k++ ) {
						ThemeProto themeProto2 = LDB.themes.Select(themeIds[k]);
						bool flag2 = false;
						if ( themeProto2.PlanetType == EPlanetType.Desert ) {
							flag2 = true;
						}
						if ( flag2 ) {
							for ( int l = 0; l < planet.index; l++ ) {
								if ( planet.star.planets[l].theme == themeProto2.ID ) {
									flag2 = false;
									break;
								}
							}
						}
						if ( flag2 ) {
							PlanetGen.tmp_theme.Add(themeProto2.ID);
						}
					}
				}
				if ( PlanetGen.tmp_theme.Count == 0 ) {
					//Debug.LogWarning($"b planetgen tmp_theme count was 0 on planet {planet.id}");
					for ( int m = 0; m < num; m++ ) {
						ThemeProto themeProto3 = LDB.themes.Select(themeIds[m]);
						if ( themeProto3.PlanetType == EPlanetType.Desert ) {
							PlanetGen.tmp_theme.Add(themeProto3.ID);
						}
					}
				}

				// Set the theme
				planet.theme = PlanetGen.tmp_theme[(int)( rand1 * (double)PlanetGen.tmp_theme.Count ) % PlanetGen.tmp_theme.Count];
			}

			// Check that the theme exists. Return null if theme is invalid.
			ThemeProto themeProto4 = LDB.themes.Select(planet.theme);
			if ( themeProto4 == null ) {
				Debug.LogError("alternative start -- FATAL: Failed to find theme ${planet.theme}. Running default behavior routine to prevent crash / corruption");
				return false;
			}

			// Set algo id for vein generation and determine modx & mody
			if ( set_algo > 0 ) {
				planet.algoId = set_algo;
			}
			else if (flagIsBirthPlanet) {
				// Mirror the birth planets algo id for vein placement
				// Get theme proto for normal birth planet
				ThemeProto themeProtoBirth = LDB.themes.Select(1);
				if ( themeProtoBirth == null ) {
					Debug.LogError("alternative start -- FATAL: Failed to find birth theme. Running default behavior routine to prevent crash / corruption");
					return false;
				}
				planet.algoId = 0;
				if ( themeProtoBirth.Algos != null && themeProtoBirth.Algos.Length > 0 ) {
					planet.algoId = themeProtoBirth.Algos[(int)( rand2 * (double)themeProtoBirth.Algos.Length ) % themeProtoBirth.Algos.Length];
					//planet.mod_x = (double)themeProtoBirth.ModX.x + rand3 * (double)( themeProtoBirth.ModX.y - themeProtoBirth.ModX.x );
					//planet.mod_y = (double)themeProtoBirth.ModY.x + rand4 * (double)( themeProtoBirth.ModY.y - themeProtoBirth.ModY.x );
					planet.mod_x = (double)themeProto4.ModX.x + rand3 * (double)( themeProto4.ModX.y - themeProto4.ModX.x );
					planet.mod_y = (double)themeProto4.ModY.x + rand4 * (double)( themeProto4.ModY.y - themeProto4.ModY.x );
				}
			}
			else {
				planet.algoId = 0;
				if (themeProto4.Algos != null && themeProto4.Algos.Length > 0 ) {
					planet.algoId = themeProto4.Algos[(int)( rand2 * (double)themeProto4.Algos.Length ) % themeProto4.Algos.Length];
					planet.mod_x = (double)themeProto4.ModX.x + rand3 * (double)( themeProto4.ModX.y - themeProto4.ModX.x );
					planet.mod_y = (double)themeProto4.ModY.x + rand4 * (double)( themeProto4.ModY.y - themeProto4.ModY.x );
				}
			}

			planet.type = themeProto4.PlanetType;
			planet.ionHeight = themeProto4.IonHeight;
			planet.windStrength = themeProto4.Wind;
			planet.waterHeight = themeProto4.WaterHeight;
			planet.waterItemId = themeProto4.WaterItemId;
			planet.levelized = themeProto4.UseHeightForBuild;

			// Choose themes for a gas giant
			if ( planet.type == EPlanetType.Gas ) {
				int num2 = themeProto4.GasItems.Length;
				int num3 = themeProto4.GasSpeeds.Length;
				int[] array = new int[num2];
				float[] array2 = new float[num3];
				float[] array3 = new float[num2];
				for ( int n = 0; n < num2; n++ ) {
					array[n] = themeProto4.GasItems[n];
				}
				double num4 = 0.0;
				System.Random random = new System.Random(theme_seed);
				for ( int num5 = 0; num5 < num3; num5++ ) {
					float num6 = themeProto4.GasSpeeds[num5];
					num6 *= (float)random.NextDouble() * 0.190909147f + 0.9090909f;
					array2[num5] = num6 * Mathf.Pow(star.resourceCoef, 0.3f);
					ItemProto itemProto = LDB.items.Select(array[num5]);
					array3[num5] = itemProto.HeatValue;
					num4 += (double)( array3[num5] * array2[num5] );
				}
				planet.gasItems = array;
				planet.gasSpeeds = array2;
				planet.gasHeatValues = array3;
				planet.gasTotalHeat = num4;
			}

			// Return true to declare modified behavior was a success
			return true;
		}
	}
}
