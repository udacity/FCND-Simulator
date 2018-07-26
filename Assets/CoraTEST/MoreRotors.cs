using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoreRotors : MonoBehaviour
{
	public Transform[] cwRotors;
	public Transform[] ccwRotors;
	public Transform tailRotor;
	public Transform tailPivot;
	public Transform[] blurredRotors;

	public bool autoRotate;

	public float rotationUpdateFreq = 0.2f;
	public float rotationSpeed = 90;
	public float tailRotationSpeed = 1200;

	FastNoise fn;
//	float[] rotationUpdates;
	int[] blurredSigns;

	void Awake ()
	{
		fn = new FastNoise ();
//		rotationUpdates = new float[blurredRotors.Length];
//		for ( int i = 0; i < rotationUpdates.Length; i++ )
//			rotationUpdates [ i ] = Random.value;
		if ( autoRotate )
		{
			cwRotors.ForEach ( x => x.GetComponent<MeshRenderer> ().enabled = false );
			ccwRotors.ForEach ( x => x.GetComponent<MeshRenderer> ().enabled = false );
		}
		blurredSigns = new int[blurredRotors.Length];
		for ( int i = 0; i < blurredRotors.Length; i++ )
		{
			Vector3 euler = blurredRotors [ i ].localEulerAngles;
			euler.z = Random.value * 360f;
			blurredRotors [ i ].localEulerAngles = euler;
			if ( blurredRotors [ i ].GetComponent<MeshRenderer> ().sharedMaterial.name.Contains ( "blue" ) )
				blurredSigns [ i ] = 1;
			else
				blurredSigns [ i ] = -1;
		}
	}

	void LateUpdate ()
	{
		if ( autoRotate )
		{
			float frameSpeed = rotationSpeed * Time.deltaTime;
			for ( int i = 0; i < blurredRotors.Length; i++ )
			{
				blurredRotors [ i ].Rotate ( Vector3.forward * frameSpeed * blurredSigns [ i ], Space.Self );
//				if ( Time.time > rotationUpdates[i] )
//				{
//					blurredRotors [ i ].Rotate ( Vector3.forward * fn.GetSimplex ( Time.time, 0 ) * 10, Space.Self );
//					rotationUpdates [ i ] = Time.time + rotationUpdateFreq;
//				}
			}
			tailRotor.Rotate ( -Vector3.right * tailRotationSpeed * Time.deltaTime );
//			Vector3 rot = Vector3.up * rotateSpeed * Time.deltaTime;
//			for ( int i = 0; i < cwRotors.Length; i++ )
//				cwRotors [ i ].Rotate ( rot, Space.Self );
//			for ( int i = 0; i < ccwRotors.Length; i++ )
//				ccwRotors [ i ].Rotate ( -rot, Space.Self );
//			if ( tailRotor != null )
//				tailRotor.RotateAround ( tailPivot.position, -tailPivot.right, tailRotorSpeed * Time.deltaTime );
		}
	}

	public void Rotate (float speed, float tailSpeed)
	{
		Vector3 right = Vector3.forward * speed * Time.deltaTime;
		Vector3 left = -Vector3.forward * speed * Time.deltaTime;
		for ( int i = 0; i < cwRotors.Length; i++ )
		{
			cwRotors [ i ].Rotate ( right );
			ccwRotors [ i ].Rotate ( left );
		}
		tailRotor.Rotate ( -Vector3.right * tailSpeed * Time.deltaTime );
	}
}