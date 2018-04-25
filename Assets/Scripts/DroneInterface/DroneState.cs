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
        
    }
}