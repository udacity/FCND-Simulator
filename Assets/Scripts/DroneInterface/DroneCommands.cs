namespace DroneInterface
{
    /// <summary>
    /// This interface describes the minimal set of commands a drone must be able to perform.
    /// </summary>
	public interface IDroneCommands
    {
        /// <summary>
        /// The drone will navigate to the 3D point (local_north, local_east, local_down)
        /// </summary>
        void Goto(double latitude, double longitude, double altitude);

        /// <summary>
        /// The drone will fly up or down to the desired relative altitude
        /// </summary>
        void Hover(double altitude);

        /// <summary>
        /// Arm/disarm the drone
        /// </summary>
        void Arm(bool arm);

        /// <summary>
        /// Set the drone to guided (autonomous control) or unguided (manual control) mode
        /// </summary>
        void TakeControl(bool guided);

        /// <summary>
        /// The drone will fly at the commanded orientation (pitch, yaw, roll) and a vertical velocity
        /// </summary>
        void SetAttitude(double pitch, double yaw, double roll, double velocity);

        /// <summary>
        /// The drone will fly with the following angular rates rollRate, pitchRate, yawRate (radians/second) and collective thrust (m/s^2)
        /// </summary>
        void SetAttitudeRate(double pitchRate, double yawRate, double rollRate, double thrust);

        /// <summary>
        /// The drone will fly at the commanded velocity in EUN frame and heading
        /// </summary>
        void SetVelocity(double vx, double vy, double vz, double heading);

        /// <summary>
        /// Command the following throttle (possible RPM) to the motors directly
        /// </summary>
        void SetMotors(float throttle, float pitchRate, float yawRate, float rollRate);

        /// <summary>
        /// Set the home position
        /// </summary>
        void SetHome(double longitude, double latitude, double altitude);
    }
}