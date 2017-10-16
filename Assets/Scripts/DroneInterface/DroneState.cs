using System;
using FlightUtils;
interface IDroneState
{
    // Returns latitude, longitude 
    GPS GetGPS();

    // Returns the drone altitude
    float Altitude();

    // Returns whether the drone is armed or disarmed
    bool Armed();

    // Returns whether the drone is being driven guided (autonomous) or unguided (manual)
    bool Guided();

    // Returns whether the drone is executing a command (we possibly return the info about the command being executed).
    // I'm not sure this is a required method but it seems it could be useful.
    bool ExecutingCommand();
    // ...
}