using UnityEngine;
using System.IO;
using MavLink;


namespace FlightUtils
{
    public static class Utils
    {
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

        // public static Directory<Quaternion, MAV_SENSOR_ORIENTATION> foo;
    }
}


