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

        /// <summery>
        /// Right handed Euler Angle (roll,pitch,yaw) to Right handed quaternion (N,E,D)
        /// https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Euler_Angles_to_Quaternion_Conversion
        /// </summery>
        public static Vector4 ToRHQuaternion(this Vector3 euler)
        {
            Vector4 q;
            // Abbreviations for the various angular functions
            float cy = Mathf.Cos(euler.z * 0.5f);
            float sy = Mathf.Sin(euler.z * 0.5f);
            float cr = Mathf.Cos(euler.x * 0.5f);
            float sr = Mathf.Sin(euler.x * 0.5f);
            float cp = Mathf.Cos(euler.y * 0.5f);
            float sp = Mathf.Sin(euler.y * 0.5f);

            q.w = cy * cr * cp + sy * sr * sp;
            q.x = cy * sr * cp - sy * cr * sp;
            q.y = cy * cr * sp + sy * sr * cp;
            q.z = sy * cr * cp - cy * sr * sp;
            return q;
        }

        /// <summery>
        /// Right handed quaternion (N,E,D) to Right handed Euler Angle (roll,pitch,yaw)
        /// https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Quaternion_to_Euler_Angles_Conversion
        /// </summery>
        public static Vector3 ToRHEuler(this Vector4 q)
        {
            // roll (x-axis rotation)
            float sinr = 2.0f * (q.w * q.x + q.y * q.z);
            float cosr = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);
            float roll = Mathf.Atan2(sinr, cosr);

            // pitch (y-axis rotation)
            float sinp = 2.0f * (q.w * q.y - q.z * q.x);
            float pitch;
            if (Mathf.Abs(sinp) >= 1)
                pitch = Mathf.Sign(sinp)*Mathf.PI/2.0f; // use 90 degrees if out of range
            else
                pitch = Mathf.Asin(sinp);

            // yaw (z-axis rotation)
            float siny = 2.0f * (q.w * q.z + q.x * q.y);
            float cosy = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);
            float yaw = Mathf.Atan2(siny, cosy);

            return new Vector3(roll, pitch, yaw);
        }

    }
}