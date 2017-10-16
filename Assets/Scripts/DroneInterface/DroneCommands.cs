namespace DroneInterface
{
    interface IDroneCommands
    {
        // The drone will navigate to the 3D point (local_north, local_east, local_down)
        void Goto(float latitude, float longitude, float altitude);

        // The drone will fly up or down to the desired relative altitude
        void Hover(float altitude);

        // Arm/disarm the drone
        void Arm(bool arm);

        // Set the drone to guided (autonomous control) or unguided (manual control) mode
        void TakeControl(bool arm);

        // The drone will fly at the commanded orientation (roll, pitch, yaw) and a vertical velocity
        void SetAttitude(float roll, float pitch, float yaw, float velocity);

        // The drone will fly with the following angular rates (roll_rate, pitch_rate, yaw_rate) and collective thrust (m/s^2)
        void SetAttitudeRate(float rollRate, float pitchRate, float yawRate, float thrust);

        // The drone will fly at the commanded velocity (north_velocity, east_velocity, vertical_velocity) and heading
        void SetVelocity(float northVelocity, float eastVelocity, float verticalVelocity, float heading);

        // Command the following thrust (possible RPM) to the motors directly
        // TODO: set appropriate variable names
        void SetMotors(float a, float b, float c, float d);
        // ...
    }
}