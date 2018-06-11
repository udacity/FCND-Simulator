using System;
using UnityEngine;

namespace DroneInterface
{
    /// <summary>
    /// This interface describes the minimal set of commands a drone must be able to perform.
    /// </summary>
	public interface IDroneController
    {
        /// <summary>
        /// Returns true if the vehicle is being controlled from outside the simulator
        /// </summary>
        bool Guided();

        /// <summary>
        /// Enables/disables offboard control
        /// </summary>
        /// <param name="offboard">true=enable offboard, false=disable offboard</param>
        void SetGuided(bool offboard);

        /// <summary>
        /// Used to enable different modes of control (for example stabilized vs position control)
        /// </summary>
        /// <param name="controlMode"></param>
        void SetControlMode(int controlMode);

        /// <summary>
        /// Returns an integer corresponding to the mode of control
        /// </summary>
        /// <returns></returns>
        int ControlMode();

        /// <summary>
        /// Arms/disarms the vehicle motors
        /// </summary>
        /// <param name="arm">true=arm, false=disarm</param>
        void ArmDisarm(bool arm);

        /// <summary>
        /// Command the vehicle to hover at the current position and altitude
        /// </summary>
        void CommandHover();

        /// <summary>
        /// Command the vehicle to the altitude, the position/attitude target does nto change
        /// </summary>
        /// <param name="altitude">Altitude in m</param>
        void CommandAltitude(float altitude);

        /// <summary>
        /// Command the vehicle position. If in Offboard, changes the vehicle control to PositionControl
        /// </summary>
        /// <param name="localPosition">Target local NED position</param>
        void CommandPosition(Vector3 localPosition);

        /// <summary>
        /// If in PositionControl or VelocityControl mode, command the vehicle heading to the specified
        /// </summary>
        /// <param name="heading">Target vehicle heading in radians</param>
        void CommandHeading(float heading);

        /// <summary>
        /// Command the vehicle local velocity. If in Offboard, changes the vehicle control VelocityControl
        /// </summary>
        /// <param name="localVelocity">Target local NED velocity in m/s</param>
        void CommandVelocity(Vector3 localVelocity);

        /// <summary>
        /// Command the vehicle's attitude and thrust
        /// </summary>
        /// <param name="attitude">Euler angles (roll, pitch, yaw) in radians (RH 3-2-1 from world to body)</param>
        /// <param name="thrust">The total commanded thrust from all motors</param>
        void CommandAttitude(Vector3 attitude, float thrust);

        /// <summary>
        /// Command the vehicle's body rates and thrust
        /// </summary>
        /// <param name="bodyrates">Body frame angular rates (p, q, r) in radians/s</param>
        /// <param name="thrust">The total commanded thrust from all motors</param>
        void CommandAttitudeRate(Vector3 bodyrates, float thrust);

        /// <summary>
        /// Command the vehicle's body moment and thrust
        /// </summary>
        /// <param name="bodyMoment">Body frame moments in kg*m^2/s^2</param>
        /// <param name="thrust"></param>
        void CommandMoment(Vector3 bodyMoment, float thrust);

        /// <summary>
        /// Command the vehicle's body moment and thrust
        /// </summary>
        void CommandControls(float controlX, float controlY, float controlZ, float controlW);

        /// <summary>
        /// Command a vehicle along a vector defined the position and velocity vectors
        /// </summary>
        /// <param name="localPosition">reference local position NED in m</param>
        /// <param name="localVelocity">reference local velocity NED in m/s</param>
        void CommandVector(Vector3 localPosition, Vector3 localVelocity);

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

    }
}