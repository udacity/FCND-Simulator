using UnityEngine;

namespace FlightUtils
{
    /// <summary>
    /// Converts between coordinate frames, supported:
    ///     NED, ENU, EUN (Unity)
    ///  
    /// D - negative altitude
    /// U - positive altitude
    /// </summary>
    public static class ReferenceFramesExtension
    {
        /// <summary>
        /// EUN -> NED
        /// </summary>
        public static Vector3 EUNToNED(this Vector3 v)
        {
            var east = v.x;
            var up = v.y;
            var north = v.z;
            return new Vector3(north, east, -up);
        }

        /// <summary>
        /// EUN -> ENU
        /// </summary>
        public static Vector3 EUNToENU(this Vector3 v)
        {
            var east = v.x;
            var up = v.y;
            var north = v.z;
            return new Vector3(east, north, up);
        }
        
        /// <summary>
        /// NED -> EUN
        /// </summary>
        public static Vector3 NEDToEUN(this Vector3 v)
        {
            var north = v.x;
            var east = v.y;
            var down = v.z;
            return new Vector3(east, -down, north);
        }

        /// <summary>
        /// ENU -> EUN
        /// </summary>
        public static Vector3 ENUToEUN(this Vector3 v)
        {
            var east = v.x;
            var north = v.y;
            var up = v.z;
            return new Vector3(east, up, north);
        }

        /// <summary>
        /// NED -> ENU
        /// </summary>
        public static Vector3 NEDToENU(this Vector3 v)
        {
            var north = v.x;
            var east = v.y;
            var down = v.z;
            return new Vector3(east, north, -down);
        }

        /// <summary>
        /// ENU -> NED
        /// </summary>
        public static Vector3 ENUToNED(this Vector3 v)
        {
            var east = v.x;
            var north = v.y;
            var up = v.z;
            return new Vector3(north, east, -up);
        }
    }
}