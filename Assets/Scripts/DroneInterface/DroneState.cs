using System;
using UnityEngine;

namespace DroneInterface
{
    /// <summary>
    /// This interface describes the minimal information that must be retrievable
    /// pertaining to a drone's state.
    /// </summary>
    public interface IDroneState
    {
        Vector3 Forward { get; }
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
        /// Returns angular velocity in Radians/sec
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

        Vector3 LinearAcceleration();

        /// <summary>
        /// TODO: flesh this out more, determine if it's necessary.
        /// Returns whether the drone is executing a command (we possibly return the info about the command being executed).
        /// I'm not sure this is a required method but it seems it could be useful.
        /// </summary>
        bool ExecutingCommand();
    }
}