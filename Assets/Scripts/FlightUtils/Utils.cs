using UnityEngine;
using System.IO;
using MavLink;


namespace FlightUtils
{
    public static class Utils
    {

        /// <summary>
        /// Meters to Latitude conversion constant.
        /// </summary>
        public const float Meter2Latitude = 1.0f / 111111.0f;

        /// <summary>
        /// Meters to Longitude conversion constant.
        /// </summary>
        public const float Meter2Longitude = 1.0f / (0.8f * 111111.0f);

        /// <summary>
        /// Converts Hertz to the number of milliseconds
        /// it would take to render 1 frame.
        /// Ex: 10 Hertz -> 10 FPS -> 100 milliseconds
        ///
        /// This is useful for sleeping a C# Task for
        /// a certain time.
        /// <summary>
        public static int HertzToMilliSeconds(float hz)
        {
            return (int)(1000f / hz);
        }

        /// <summary>
        /// Convert global coordinates to local coordinates
        /// </summary>
        public static Vector3 GlobalToLocalCoords(double longitude, double latitude, double altitude, double homeLongitude, double homeLatitude) {
            var localPosition = Vector3.zero;
            localPosition.x = (float)(latitude - homeLatitude) / Meter2Latitude;
            localPosition.y = (float)(longitude - homeLongitude) / Meter2Longitude;
            localPosition.z = (float)(-altitude);
            return localPosition;
        }

        /// <summary>
        /// Convert local coordinates to global coordinates
        /// </summary>
        // TODO: finish this
        // public static Vector3 LocalToGlobalCoords() {
        //     var globalPosition = Vector3.zero;
        //     return globalPosition;
        // }
    }
}


