using System;

namespace DroneInterface
{
    interface IDroneState
    {
        double Latitude();

        double Longitude();

        double Altitude();

        // Returns whether the drone is armed or disarmed
        bool Armed();

        // Returns whether the drone is being driven guided (autonomous) or unguided (manual)
        bool Guided();

        double NorthVelocity();

        double EastVelocity();

        double VerticalVelocity();

        // in degrees
        double Roll();

        // in degrees
        double Yaw();

        // in degrees
        double Pitch();

        // TODO: flesh this out more, determine if it's necessary.
        // Returns whether the drone is executing a command (we possibly return the info about the command being executed).
        // I'm not sure this is a required method but it seems it could be useful.
        bool ExecutingCommand();
        // ...
    }
}