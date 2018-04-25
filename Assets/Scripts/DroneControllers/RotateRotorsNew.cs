using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRotorsNew : MonoBehaviour
{
	public Transform frontLeftTopRotor;
	public Transform frontRightTopRotor;
	public Transform rearLeftTopRotor;
	public Transform rearRightTopRotor;
	public Transform frontLeftBottomRotor;
	public Transform frontRightBottomRotor;
	public Transform rearLeftBottomRotor;
	public Transform rearRightBottomRotor;
	public Renderer body;
	public float maxRotorRPM = 2000;
	[SerializeField]
	float curRotorSpeed;




	void Start () {
		//Debug.Log("Rotate the rotors.");
	}

	void LateUpdate ()
	{
		// use this to ignore LOD levels not being shown
		if ( !body.enabled || !body.isVisible )
		{
			return;
		}

		// Spin the rotors
		float rps = maxRotorRPM / 60f;
		float degPerSec = rps * 360f;
		curRotorSpeed = degPerSec;

		// Use quaternions for rotation
		float yaw = (float) ( curRotorSpeed * Time.deltaTime );// * 10 );
		Quaternion q1 = Quaternion.Euler ( Vector3.up * -yaw );
		Quaternion q2 = Quaternion.Euler ( Vector3.up * yaw );
		frontLeftTopRotor.rotation *= q1;
		rearLeftTopRotor.rotation *= q2;
		frontLeftBottomRotor.rotation *= q2;
		rearLeftBottomRotor.rotation *= q1;
		frontRightTopRotor.rotation *= q2;
		rearRightTopRotor.rotation *= q1;
		frontRightBottomRotor.rotation *= q1;
		rearRightBottomRotor.rotation *= q2;
	}
}