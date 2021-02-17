//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using HarmonyLib;
//using UnityEngine;
//using ZorbsAlternativeStart.Extensions;
//using Random = System.Random;

//namespace ZorbsAlternativeStart.Patches {
//    [HarmonyPatch(typeof(StarGen))]    
//    public static class PatchStarGen {
//        private static Random seededRandom;
//        private static ESpectrType[] types = new ESpectrType[] { ESpectrType.F, ESpectrType.G, ESpectrType.K, ESpectrType.M };

//        [HarmonyPostfix]
//        [HarmonyPatch("CreateBirthStar")]
//        public static void CreateBirthStar(ref GalaxyData galaxy, int seed, ref StarData __result) {
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch("CreateStar")]
//        public static void CreateStar(ref GalaxyData galaxy, ref VectorLF3 pos, ref int id, int seed, ref EStarType needtype, ref ESpectrType needSpectr, ref StarData __result) {
//            //Console.WriteLine($"CreateStar() id: {__result.id}\r\n\tage: {__result.age}\r\n\tclassFactor: {__result.classFactor}\r\n\tcolor: {__result.color}\r\n\tluminosity: {__result.luminosity}\r\n\tradius: {__result.radius}\r\n\ttemperature: {__result.temperature}\r\n\tspectr: {__result.spectr}\r\n\tlifetime: {__result.lifetime}\r\n\tlightBalanceRaddius: {__result.lightBalanceRadius}\r\n\ttype: {__result.type}");
//        }
//    }
//}
