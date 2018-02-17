using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneInterface;
using DroneControllers;

public class DummyDrone : MonoBehaviour, IDrone
{
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

	public Vector3 Forward { get { return Vector3.ProjectOnPlane ( tr.forward, Vector3.up ).normalized; } }
	// local coordinates (x, y, z) in Unity.
	public Vector3 UnityCoords() { return tr.position; }

	public Vector3 LocalCoords(){ return tr.position; }

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

	/// <summary>
	/// TODO: flesh this out more, determine if it's necessary.
	/// Returns whether the drone is executing a command (we possibly return the info about the command being executed).
	/// I'm not sure this is a required method but it seems it could be useful.
	/// </summary>
	public bool ExecutingCommand() {return false;}
}