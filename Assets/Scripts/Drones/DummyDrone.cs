using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneInterface;
using DroneControllers;

public class DummyDrone : MonoBehaviour, IDrone
{
    public float FlightTime()
    {
        return flightTime;
    }

	public bool Frozen
	{
		get { return rb.isKinematic; }
		set { rb.isKinematic = value; }
	}

    float flightTime;
	public float moveSpeed = 50;
	public float turnSpeed = 360;

	Transform tr;
	Rigidbody rb;
	bool armed;
	double homeLat;
	double homeLon;
	double lat;
	double lon;

	void Awake ()
	{
		Simulation.ActiveDrone = this;
		tr = transform;
		rb = GetComponent<Rigidbody> ();
		rb.useGravity = false;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

        flightTime = 0.0f;
	}
	
	void LateUpdate ()
	{
		Vector3 input = new Vector3 ( Input.GetAxis ( "Horizontal" ), Input.GetAxis ( "Thrust" ), Input.GetAxis ( "Vertical" ) );
		input *= moveSpeed;

		rb.velocity = tr.rotation * input;
		float yaw = Input.GetAxis ( "Yaw" );
		if ( yaw != 0 )
			tr.Rotate ( Vector3.up * yaw * turnSpeed * Mathf.Deg2Rad );
	}

	public void Goto (double d, double dd, double ddd)
	{
	}

	public void Hover (double d)
	{
	}

	public void Arm (bool b)
	{
		armed = b;
	}

	public void TakeControl (bool b)
	{
	}

	public void SetHeading(double heading)
	{

	}

	public void SetAttitude (double d, double dd, double ddd, double dddd)
	{
	}

	/// <summary>
	/// Set whether the drone is using a remote controller.
	/// </summary>
	public void ControlRemotely(bool remote) {}


	/// <summary>
	/// The drone will fly with the following angular rates rollRate, pitchRate, yawRate (radians/second) and collective thrust (m/s^2)
	/// </summary>
	public void SetAttitudeRate(double pitchRate, double yawRate, double rollRate, double thrust) {}

	/// <summary>
	/// The drone will fly at the commanded velocity in EUN frame and heading (radians).
	/// </summary>
	public void SetVelocity(double vx, double vy, double vz, double heading) {}

	/// <summary>
	/// TODO: Make sure this is correct
	/// Command the following throttle (possible RPM) to the motors directly
	/// </summary>
	public void SetMotors(float throttle, float pitchRate, float yawRate, float rollRate) {}

	/// <summary>
	/// Set the home position
	/// </summary>
	public void SetHome(double longitude, double latitude, double altitude) {}

	/// <summary>
	/// Place the drone at a specific world position
	/// </summary>
	public void Place (UnityEngine.Vector3 location) {transform.position = location;}

    public void InitializeVehicle(Vector3 location, Vector3 velocity, Vector3 euler) { return; }

	public Vector3 Forward { get { return Vector3.ProjectOnPlane ( tr.forward, Vector3.up ).normalized; } }
	// local coordinates (x, y, z) in Unity.
	public Vector3 CoordsUnity() { return tr.position; }

	public Vector3 CoordsLocal(){ return tr.position; }

	public double Latitude(){ return lat; }

	public double Longitude(){ return lon; }

	public double Altitude(){ return tr.position.y; }

	public double HomeLatitude(){ return 0; }

	public double HomeLongitude(){ return 0; }

	/// <summary>
	/// Returns whether the drone is using a remote controller.
	/// For example, a PID controller from a client python script.
	/// </summary>
	public bool ControlledRemotely() { return false; }

	/// <summary>
	/// Returns whether the drone is armed or disarmed.
	/// </summary>
	public bool Armed() { return armed; }

	/// <summary>
	/// Returns whether the drone is being driven guided (autonomous) or unguided (manual)
	/// </summary>
	public bool Guided() { return false; }

	/// <summary>
	/// Corresponds to velocity along the x axis.
	/// </summary>
	public double NorthVelocity() { return 0; }

	/// <summary>
	/// Corresponds to velocity along the y axis.
	/// </summary>
	public double EastVelocity() { return 0; }

	/// <summary>
	/// Correponds to the velocity in the downward direciton (+down)
	/// </summary>
	public double DownVelocity() { return 0; }

	/// <summary>
	/// Corresponds to velocity along the z axis.
	/// </summary>
	public double VerticalVelocity() { return 0; }

	/// <summary>
	/// Returns the rotation around the z-axis in radians.
	/// </summary>
	public double Roll() { return 0; }

	/// <summary>
	/// Returns the rotation around the y-axis in radians.
	/// </summary>
	public double Yaw() { return 0; }

	/// <summary>
	/// Returns the rotation around the x-axis in radians.
	/// </summary>
	public double Pitch() { return 0; }

	/// <summary>
	/// Returns angular velocity in Radians/sec
	/// </summary>
	public Vector3 AngularVelocity() { return Vector3.zero; }

	/// <summary>
	/// Returns the angular velocity around the body forward axis in Radians/sec (RH+)
	/// </summary>
	public double Rollrate() { return 0; }

	/// <summary>
	/// Returns the angular velocity around the body right axis in Radians/sec (RH+)
	/// </summary>
	public double Pitchrate() { return 0; }

	/// <summary>
	/// Returns the angular velocity around the body down axis in Radians/sec (RH+)
	/// </summary>
	public double Yawrate() { return 0;}

	/// <summary>
	/// Returns angular acceleration in Radians/sec^2
	/// </summary>
	public Vector3 AngularAcceleration() {return Vector3.zero;}

	public Vector3 LinearAcceleration() {return Vector3.zero;}

    public Vector3 LocalVelocity() { return Vector3.zero; }

    public Vector3 EulerAngles() { return Vector3.zero; }

    public Vector4 QuaternionAttitude() { return Vector4.zero; }

	/// <summary>
	/// TODO: flesh this out more, determine if it's necessary.
	/// Returns whether the drone is executing a command (we possibly return the info about the command being executed).
	/// I'm not sure this is a required method but it seems it could be useful.
	/// </summary>
	public bool ExecutingCommand() {return false;}

    public void LocalPositionTarget(Vector3 pos)
    {
        //simpleQuadCtrl.positionTarget = pos;
    }

    public void LocalVelocityTarget(Vector3 vel)
    {
        //simpleQuadCtrl.velocityTarget = vel;
    }

    public void LocalAccelerationTarget(Vector3 acc)
    {
        //simpleQuadCtrl.accelerationTarget = acc;
    }

    public void AttitudeTarget(Vector3 att)
    {
        //simpleQuadCtrl.attitudeTarget = att;
    }

    public void BodyRateTarget(Vector3 br)
    {
        //simpleQuadCtrl.bodyRateTarget = br;
    }

    /// <summary>
    /// Vehicle attitude (roll, pitch, yaw) in radians (RH 3-2-1 transform from world to body)
    /// </summary>
    /// <returns></returns>
    public Vector3 AttitudeEuler()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// Vehicle attitude in quaternions (RH from world to body)
    /// </summary>
    /// <returns></returns>
    public Vector4 AttitudeQuaternion()
    {
        return Vector4.zero;
    }

    /// <summary>
    /// The vehicle NED linear velocity in m/s
    /// </summary>
    public Vector3 VelocityLocal()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The linear velocity in the vehicle frame (front, right, down) in m/s
    /// </summary>
    /// <returns></returns>
    public Vector3 VelocityBody()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The vehicle NED linear acceleration in m/s^2
    /// </summary>
    public Vector3 AccelerationLocal()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The linear acceleration in the vehicle frame (front, right, down) in m/s^2
    /// </summary>
    public Vector3 AccelerationBody()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The angular velocity around the vehicle frame axes (front, right, down) in rad/s
    /// </summary>
    /// <returns></returns>
    public Vector3 AngularRatesBody()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The current body frame control moments being applied to the vehicle in kg*m^2/s^2
    /// </summary>
    public Vector3 MomentBody()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The current body frame control forces being applied to the vehicle in kg*m/s^2
    /// </summary>
    /// <returns></returns>
    public Vector3 ForceBody()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The state of the motors
    /// </summary>
    /// <returns></returns>
    public bool MotorsArmed()
    {
        return false;
    }

    /// <summary>
    /// Arms/disarms the vehicle motors
    /// </summary>
    /// <param name="arm">true=arm, false=disarm</param>
    public void ArmDisarm(bool arm)
    {

    }



    /// IDroneController Methods

    /// <summary>
    /// Enables/disables offboard control
    /// </summary>
    /// <param name="offboard">true=enable offboard, false=disable offboard</param>
    public void SetGuided(bool offboard)
    {

    }

    /// <summary>
    /// Used to enable different modes of control (for example stabilized vs position control)
    /// </summary>
    /// <param name="controlMode"></param>
    public void SetControlMode(int controlMode)
    {

    }

    /// <summary>
    /// Returns an integer corresponding to the mode of control
    /// </summary>
    /// <returns></returns>
    public int ControlMode()
    {
        return 0;
    }

    /// <summary>
    /// Command the vehicle to hover at the current position and altitude
    /// </summary>
    public void CommandHover()
    {

    }

    /// <summary>
    /// Command the vehicle to the altitude, the position/attitude target does nto change
    /// </summary>
    /// <param name="altitude">Altitude in m</param>
    public void CommandAltitude(float altitude)
    {

    }

    /// <summary>
    /// Command the vehicle position. If in Offboard, changes the vehicle control to PositionControl
    /// </summary>
    /// <param name="localPosition">Target local NED position</param>
    public void CommandPosition(Vector3 localPosition)
    {

    }

    /// <summary>
    /// If in PositionControl or VelocityControl mode, command the vehicle heading to the specified
    /// </summary>
    /// <param name="heading">Target vehicle heading in radians</param>
    public void CommandHeading(float heading)
    {

    }

    /// <summary>
    /// Command the vehicle local velocity. If in Offboard, changes the vehicle control VelocityControl
    /// </summary>
    /// <param name="localVelocity">Target local NED velocity in m/s</param>
    public void CommandVelocity(Vector3 localVelocity)
    {

    }

    /// <summary>
    /// Command the vehicle's attitude and thrust
    /// </summary>
    /// <param name="attitude">Euler angles (roll, pitch, yaw) in radians (RH 3-2-1 from world to body)</param>
    /// <param name="thrust">The total commanded thrust from all motors</param>
    public void CommandAttitude(Vector3 attitude, float thrust)
    {

    }

    /// <summary>
    /// Command the vehicle's body rates and thrust
    /// </summary>
    /// <param name="bodyrates">Body frame angular rates (p, q, r) in radians/s</param>
    /// <param name="thrust">The total commanded thrust from all motors</param>
    public void CommandAttitudeRate(Vector3 bodyrates, float thrust)
    {

    }

    /// <summary>
    /// Command the vehicle's body moment and thrust
    /// </summary>
    /// <param name="bodyMoment">Body frame moments in kg*m^2/s^2</param>
    /// <param name="thrust"></param>
    public void CommandMoment(Vector3 bodyMoment, float thrust)
    {

    }

    /// <summary>
    /// Command the vehicle's body moment and thrust
    /// </summary>
    public void CommandControls(float controlX, float controlY, float controlZ, float controlW)
    {
        return;
    }

    /// <summary>
    /// Command a vehicle along a vector defined the position and velocity vectors
    /// </summary>
    /// <param name="localPosition">reference local position NED in m</param>
    /// <param name="localVelocity">reference local velocity NED in m/s</param>
    public void CommandVector(Vector3 localPosition, Vector3 localVelocity)
    {

    }


    /// IDroneSensors Methods

    /// <summary>
    /// The body angular rate measurements from the gyro in radians/s
    /// </summary>
    /// <returns></returns>
    public Vector3 GyroRates()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The linear acceleration measurements from the IMU in m/s^2
    /// </summary>
    public Vector3 IMUAcceleration()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The compass heading in radians
    /// </summary>
    /// <returns></returns>
    public float CompassHeading()
    {
        return 0.0f;
    }

    /// <summary>
    /// The body 3-axis magnetometer measurement in Gauss.
    /// </summary>
    /// <returns></returns>
    public Vector3 CompassMagnetometer()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The barometeric pressure altitude in m (positive up)
    /// </summary>
    /// <returns></returns>
    public float BarometerAltitude()
    {
        return 0.0f;
    }

    /// <summary>
    /// The vehicle's attitude estimated from the compass, IMU and gyro
    /// </summary>
    /// <returns></returns>
    public Vector3 AttitudeEstimate()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// The vehicle latitude in degrees
    /// </summary>
    /// <returns></returns>
    public double GPSLatitude()
    {
        return 0.0d;
    }

    /// <summary>
    /// The vehicle longitude in degrees
    /// </summary>
    /// <returns></returns>
    public double GPSLongitude()
    {
        return 0.0d;
    }

    /// <summary>
    /// The vehicle altitude in m, relative to sea level (positive up)
    /// </summary>
    /// <returns></returns>
    public double GPSAltitude()
    {
        return 0.0d;
    }


    /// <summary>
    /// The home altitude in m, from sea level  (positive up)
    /// </summary>
    /// <returns></returns>
    public double HomeAltitude()
    {
        return 0.0d;
    }

    /// <summary>
    /// Local NED position in m, relative to the home position
    /// </summary>
    /// <returns></returns>
    public Vector3 LocalPosition()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// Local NED velocity in m/s
    /// </summary>
    public Vector3 GPSVelocity()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// Sets the home position used in the local position calculation
    /// </summary>
    /// <param name="longitude">longitude in degrees</param>
    /// <param name="latitude">latitude</param>
    /// <param name="altitude">altitude in m, relative to seal level</param>
    public void SetHomePosition(double longitude, double latitude, double altitude)
    {
        
    }

    public void SetHomePosition()
    {

    }
}