using UnityEngine;

namespace Other
{
    public static class VectorExtensions
    {
        public static float Average(this Vector2Int vector)
        {
            return (vector.x + vector.y) / 2f;
        } 
        
        public static float Average(this Vector2 vector)
        {
            return (vector.x + vector.y) / 2f;
        } 
    }
}