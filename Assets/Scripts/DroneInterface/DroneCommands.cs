namespace DroneInterface
{
    // This interface describes the minimal set of commands a drone must be able to perform.
	public interface IDroneCommands
    {
        // The drone will navigate to the 3D point (local_north, local_east, local_down)
        void Goto(double latitude, double longitude, double altitude);

        // The drone will fly up or down to the desired relative altitude
        void Hover(double altitude);

        // Arm/disarm the drone
        void Arm(bool arm);

        // Set the drone to guided (autonomous control) or unguided (manual control) mode
        void TakeControl(bool guided);

        // The drone will fly at the commanded orientation (roll, pitch, yaw) and a vertical velocity
        void SetAttitude(double roll, double pitch, double yaw, double velocity);

        // The drone will fly with the following angular rates (roll_rate, pitch_rate, yaw_rate) and collective thrust (m/s^2)
        void SetAttitudeRate(double rollRate, double pitchRate, double yawRate, double thrust);

        // The drone will fly at the commanded velocity (north velocity, east velocity, vertical velocity) and heading
        void SetVelocity(double northVelocity, double eastVelocity, double verticalVelocity, double heading);

        // Command the following thrust (possible RPM) to the motors directly
        void SetMotors(float throttle, float pitchRate, float yawRate, float rollRate);
        // ...

        // Set the home position
        void SetHome(double longitude, double latitude, double altitude);
    }
}