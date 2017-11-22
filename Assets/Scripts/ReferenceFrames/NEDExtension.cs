using UnityEngine;

namespace ReferenceFrames
{
    // x -> east/long
    // z -> north/lat
    public static class ReferenceFramesExtension
    {
        // (x, y, z) -> (z, x, -y)
        public static Vector3 UnityToNED(this Vector3 v)
        {
            return new Vector3(v.z, v.x, -v.y);
        }

        // (x, y, z) -> (x, z, y)
        public static Vector3 UnityToENU(this Vector3 v)
        {
            return new Vector3(v.x, v.z, v.y);
        }
        
        // (z, x, -y) -> (x, y, z)
        public static Vector3 NEDToUnity(this Vector3 v)
        {
            return new Vector3(v.y, -v.z, v.x);
        }

        // (x, z, y) -> (x, y, z)
        public static Vector3 ENUToUnity(this Vector3 v)
        {
            return new Vector3(v.x, v.z, v.y);
        }

        // (z, x, -y) -> (x, z, y)
        public static Vector3 NEDToENU(this Vector3 v)
        {
            return new Vector3(v.y, v.x, -v.z);
        }

        // (x, z, y) -> (z, x, -y)
        public static Vector3 ENUToNed(this Vector3 v)
        {
            return new Vector3(v.y, v.x, -v.z);
        }
    }
}