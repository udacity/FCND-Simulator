using UnityEngine;

namespace ReferenceFrames
{
    // x -> east/long
    // z -> north/lat
    // y -> altitude
    public static class ReferenceFramesExtension
    {
        /// <summary>
        /// (x, y, z) -> (z, x, -y)
        /// </summary>
        public static Vector3 UnityToNED(this Vector3 v)
        {
            return new Vector3(v.z, v.x, -v.y);
        }

        /// <summary>
        /// (x, y, z) -> (x, z, y)
        /// </summary>
        public static Vector3 UnityToENU(this Vector3 v)
        {
            return new Vector3(v.x, v.z, v.y);
        }
        
        /// <summary>
        /// (z, x, -y) -> (x, y, z)
        /// </summary>
        public static Vector3 NEDToUnity(this Vector3 v)
        {
            return new Vector3(v.y, -v.z, v.x);
        }

        /// <summary>
        /// (x, z, y) -> (x, y, z)
        /// </summary>
        public static Vector3 ENUToUnity(this Vector3 v)
        {
            return new Vector3(v.x, v.z, v.y);
        }

        /// <summary>
        /// (z, x, -y) -> (x, z, y)
        /// </summary>
        public static Vector3 NEDToENU(this Vector3 v)
        {
            return new Vector3(v.y, v.x, -v.z);
        }

        /// <summary>
        /// (x, z, y) -> (z, x, -y)
        /// </summary>
        public static Vector3 ENUToNed(this Vector3 v)
        {
            return new Vector3(v.y, v.x, -v.z);
        }
    }
}