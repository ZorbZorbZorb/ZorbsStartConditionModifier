using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Random = System.Random;

namespace ZorbsAlternativeStart.Extensions {
    public static class ArrayExtensions {
        public static T ChooseRandomUsingDSPRand<T>(this T[] array, double random) =>
            array[(int)( random * (double)array.Length ) % array.Length];
        public static T ChooseRandomUsingDSPRand<T>(this List<T> list, double random) {
            int count = list.Count();
            return list[(int)( random * (double)count ) % count];
        }
        public static T ChooseRandom<T>(this IEnumerable<T> array, Random random) => 
            array.ElementAt(random.Next(0, array.Count()));
    }
}
