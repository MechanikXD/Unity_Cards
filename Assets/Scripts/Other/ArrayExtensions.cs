using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Other
{
    public static class ArrayExtensions
    {
        public static T GetRandom<T>(this IList<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }
        
        public static void Shuffle(this IList array)
        {
            for (var i = 0; i < array.Count; i++ )
            {
                var random = Random.Range(i, array.Count);
                (array[i], array[random]) = (array[random], array[i]);
            }
        }
        
        public static IList<T> Shuffled<T>(this IList<T> array)
        {
            var result = new List<T>(array); // create a copy of the original list
            for (var i = 0; i < result.Count; i++)
            {
                var random = Random.Range(i, result.Count);
                (result[i], result[random]) = (result[random], result[i]);
            }
            return result;
        }
        
        public static void Shuffle<T>(this LinkedList<T> list)
        {
            var values = list.ToList();
            for (var i = 0; i < values.Count; i++)
            {
                var j = Random.Range(i, values.Count);
                (values[i], values[j]) = (values[j], values[i]);
            }

            // Rebuild linked list
            list.Clear();
            foreach (var v in values) list.AddLast(v);
        }
    }
}