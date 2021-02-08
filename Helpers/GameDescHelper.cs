using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ZorbsAlternateStart.Helpers {
    public static class GameDescHelper {
        // Print all planet themes
        public static void PrintAllThemes(GameDesc game_desc) {
            int[] themeIds = game_desc.themeIds;
            int num = themeIds.Length;
            Debug.Log("Printing themes:");
            for ( int i = 0; i < num; i++ ) {
                Debug.Log(game_desc.themeIds[i]);
            }
        }
    }
}
