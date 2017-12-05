using System;
using UnityEngine;

namespace DroneInterface
{
    // This interface describes the minimal information that must be retrievable
    // pertaining to a drone's state.
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

        // Returns whether the drone is armed or disarmed
        bool Armed();

        // Returns whether the drone is being driven guided (autonomous) or unguided (manual)
        bool Guided();

        // Corresponds to velocity along the x axis.
        double NorthVelocity();

        // Corresponds to velocity along the y axis.
        double EastVelocity();

        // Corresponds to velocity along the z axis.
        double VerticalVelocity();

        // In degrees
        double Roll();

        // In degrees
        double Yaw();

        // In degrees
        double Pitch();

        // in Radians/sec
        Vector3 AngularVelocity();

        Vector3 AngularAcceleration();

        Vector3 LinearAcceleration();

        // TODO: flesh this out more, determine if it's necessary.
        // Returns whether the drone is executing a command (we possibly return the info about the command being executed).
        // I'm not sure this is a required method but it seems it could be useful.
        bool ExecutingCommand();
        // ...
    }
}