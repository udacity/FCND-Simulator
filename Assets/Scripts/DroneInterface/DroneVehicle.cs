using System;
using UnityEngine;

namespace DroneInterface
{
    /// <summary>
    /// This interface describes the minimal information that must be retrievable
    /// pertaining to a drone's state.
    /// </summary>
    public interface IDroneVehicle
    {
        float FlightTime();

		/// <summary>
		/// Freeze the drone vehicle and or query if it's frozen
		/// </summary>
		bool Frozen { get; set; }

        /// <summary>
        /// The local coordinates relative to the Unity map in the Unity frame
        /// </summary>
        Vector3 CoordsUnity();

        /// <summary>
        /// The local coordinates relative to the Unity map in a NED frame
        /// </summary>
        Vector3 CoordsLocal();

        /// <summary>
        /// Vehicle attitude (roll, pitch, yaw) in radians (RH 3-2-1 transform from world to body)
        /// </summary>
        /// <returns></returns>
        Vector3 AttitudeEuler();

        /// <summary>
        /// Vehicle attitude in quaternions (RH from world to body)
        /// </summary>
        /// <returns></returns>
        Vector4 AttitudeQuaternion();

        /// <summary>
        /// The vehicle NED linear velocity in m/s
        /// </summary>
        Vector3 VelocityLocal();

        /// <summary>
        /// The linear velocity in the vehicle frame (front, right, down) in m/s
        /// </summary>
        /// <returns></returns>
        Vector3 VelocityBody();

		/// <summary>
		/// Linear velocity in unity coords
		/// </summary>
		Vector3 VelocityUnity();

        /// <summary>
        /// The vehicle NED linear acceleration in m/s^2
        /// </summary>
        Vector3 AccelerationLocal();

        /// <summary>
        /// The linear acceleration in the vehicle frame (front, right, down) in m/s^2
        /// </summary>
        Vector3 AccelerationBody();

        /// <summary>
        /// The angular velocity around the vehicle frame axes (front, right, down) in rad/s
        /// </summary>
        /// <returns></returns>
        Vector3 AngularRatesBody();

		/// <summary>
		/// Angular velocity in unity coords
		/// </summary>
		/// <returns></returns>
		Vector3 AngularRatesUnity();

        /// <summary>
        /// The current body frame control moments being applied to the vehicle in kg*m^2/s^2
        /// </summary>
        Vector3 MomentBody();

        /// <summary>
        /// The current body frame control forces being applied to the vehicle in kg*m/s^2
        /// </summary>
        /// <returns></returns>
        Vector3 ForceBody();

        /// <summary>
        /// The state of the motors
        /// </summary>
        /// <returns></returns>
        bool MotorsArmed();

        /// <summary>
		/// Place the drone at a specific world position
		/// </summary>
		void Place(Vector3 location);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location">Coordinate in Unity frame</param>
        /// <param name="velocity">Velocity in Unity frame</param>
        /// <param name="euler">rotation in Unity (LH) frame</param>
        void InitializeVehicle(Vector3 location, Vector3 velocity, Vector3 euler);

        /*
        //Vector3 Forward { get; }
        // local coordinates (x, y, z) in Unity.
        Vector3 UnityCoords();

        Vector3 LocalCoords();

        double Latitude();

        double Longitude();

        double Altitude();

        double HomeLatitude();

        double HomeLongitude();

        /// <summary>
        /// Returns whether the drone is using a remote controller.
        /// For example, a PID controller from a client python script.
        /// </summary>
        bool ControlledRemotely();

        /// <summary>
        /// Returns whether the drone is armed or disarmed.
        /// </summary>
        bool Armed();

        /// <summary>
        /// Returns whether the drone is being driven guided (autonomous) or unguided (manual)
        /// </summary>
        bool Guided();

        /// <summary>
        /// Corresponds to velocity along the x axis.
        /// </summary>
        double NorthVelocity();

        /// <summary>
        /// Corresponds to velocity along the y axis.
        /// </summary>
        double EastVelocity();

        /// <summary>
        /// Correponds to the velocity in the downward direciton (+down)
        /// </summary>
        double DownVelocity();

        /// <summary>
        /// Returns the vehicle velocity in North, East, Down Local Frame
        /// </summary>
        Vector3 LocalVelocity();

        /// <summary>
        /// Corresponds to velocity along the z axis.
        /// </summary>
        double VerticalVelocity();

        /// <summary>
        /// Returns the rotation around the z-axis in radians.
        /// </summary>
        double Roll();

        /// <summary>
        /// Returns the rotation around the y-axis in radians.
        /// </summary>
        double Yaw();

        /// <summary>
        /// Returns the rotation around the x-axis in radians.
        /// </summary>
        double Pitch();

        /// <summary>
        /// Returns the vehicle's Euler angles (Roll, Pitch, Yaw) in radians for a 3-2-1 RH rotation
        /// </summary>
        Vector3 EulerAngles();

        /// <summary>
        /// Returns the vehicle's attitude in quaternion for a RH rotation from the local frame to the body frame
        /// </summary>
        /// <returns></returns>
        Vector4 QuaternionAttitude();

        /// <summary>
        /// Returns angular velocity in Radians/sec in the body frame
        /// </summary>
        Vector3 AngularVelocity();

        /// <summary>
        /// Returns the angular velocity around the body forward axis in Radians/sec (RH+)
        /// </summary>
        double Rollrate();

        /// <summary>
        /// Returns the angular velocity around the body right axis in Radians/sec (RH+)
        /// </summary>
        double Pitchrate();

        /// <summary>
        /// Returns the angular velocity around the body down axis in Radians/sec (RH+)
        /// </summary>
        double Yawrate();

        /// <summary>
        /// Returns angular acceleration in Radians/sec^2
        /// </summary>
        Vector3 AngularAcceleration();

        /// <summary>
        /// Returns the linear acceleration in m/s^2 in the body frame
        /// </summary>
        Vector3 LinearAcceleration();

        /// <summary>
        /// Sets the value of the position target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        void LocalPositionTarget(Vector3 v);

        /// <summary>
        /// Sets the value of the velocity target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        void LocalVelocityTarget(Vector3 v);

        /// <summary>
        /// Sets the value of the acceleration target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        void LocalAccelerationTarget(Vector3 v);

        /// <summary>
        /// Sets the value of the attitude target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        void AttitudeTarget(Vector3 v);

        /// <summary>
        /// Sets the value of the body rate target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        void BodyRateTarget(Vector3 v);

        /// <summary>
        /// TODO: flesh this out more, determine if it's necessary.
        /// Returns whether the drone is executing a command (we possibly return the info about the command being executed).
        /// I'm not sure this is a required method but it seems it could be useful.
        /// </summary>
        bool ExecutingCommand();
        */
    }
}