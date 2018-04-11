using System;
using UnityEngine;

namespace DroneInterface
{
    /// <summary>
    /// This interface describes the minimal information that must be retrievable
    /// pertaining to a drone's state.
    /// </summary>
    public interface IDroneSensors
    {
        /// <summary>
        /// The body angular rate measurements from the gyro in radians/s
        /// </summary>
        /// <returns></returns>
        Vector3 GyroRates();

        /// <summary>
        /// The linear acceleration measurements from the IMU in m/s^2
        /// </summary>
        Vector3 IMUAcceleration();

        /// <summary>
        /// The compass heading in radians
        /// </summary>
        /// <returns></returns>
        float CompassHeading();

        /// <summary>
        /// The body 3-axis magnetometer measurement in Gauss.
        /// </summary>
        /// <returns></returns>
        Vector3 CompassMagnetometer();

        /// <summary>
        /// The barometeric pressure altitude in m (positive up)
        /// </summary>
        /// <returns></returns>
        float BarometerAltitude();

        /// <summary>
        /// The vehicle's attitude estimated from the compass, IMU and gyro
        /// </summary>
        /// <returns></returns>
        Vector3 AttitudeEstimate();
        
        /// <summary>
        /// The vehicle latitude in degrees
        /// </summary>
        /// <returns></returns>
        double GPSLatitude();

        /// <summary>
        /// The vehicle longitude in degrees
        /// </summary>
        /// <returns></returns>
        double GPSLongitude();

        /// <summary>
        /// The vehicle altitude in m, relative to sea level (positive up)
        /// </summary>
        double GPSAltitude();

        /// <summary>
        /// The home latitude in degrees, used to calculate a local position
        /// </summary>
        double HomeLatitude();

        /// <summary>
        /// The home longitude in degrees, used to calculate a local position
        /// </summary>
        double HomeLongitude();

        /// <summary>
        /// The home altitude in m, from sea level  (positive up)
        /// </summary>
        double HomeAltitude();

        /// <summary>
        /// Local NED position in m, relative to the home position
        /// </summary>
        Vector3 LocalPosition();

        /// <summary>
        /// Local NED velocity in m/s
        /// </summary>
        Vector3 GPSVelocity();

        /// <summary>
        /// Sets the home position used in the local position calculation
        /// </summary>
        /// <param name="longitude">longitude in degrees</param>
        /// <param name="latitude">latitude</param>
        /// <param name="altitude">altitude in m, relative to seal level</param>
        void SetHomePosition(double longitude, double latitude, double altitude);
        
    }
}