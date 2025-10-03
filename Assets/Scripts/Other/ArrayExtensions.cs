using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Other
{
    public static class ArrayExtensions
    {
        public static void Shuffle(this IList array)
        {
            for (var i = 0; i < array.Count; i++ )
            {
                var random = Random.Range(i, array.Count);
                (array[i], array[random]) = (array[random], array[i]);
            }
        }
        
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> array)
        {
            var list = array.ToList();
            
            for (var i = 0; i < list.Count; i++ )
            {
                var random = Random.Range(i, list.Count);
                (list[i], list[random]) = (list[random], list[i]);
            }
            
            return list;
        }
    }
}