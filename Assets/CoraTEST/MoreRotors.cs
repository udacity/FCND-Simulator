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

	FastNoise fn;
	float[] rotationUpdates;

	void Awake ()
	{
		fn = new FastNoise ();
		rotationUpdates = new float[blurredRotors.Length];
		for ( int i = 0; i < rotationUpdates.Length; i++ )
			rotationUpdates [ i ] = Random.value;
	}

	void LateUpdate ()
	{
		if ( autoRotate )
		{
			for ( int i = 0; i < rotationUpdates.Length; i++ )
			{
				if ( Time.time > rotationUpdates[i] )
				{
					blurredRotors [ i ].Rotate ( Vector3.forward * fn.GetSimplex ( Time.time, 0 ) * 10, Space.Self );
					rotationUpdates [ i ] = Time.time + rotationUpdateFreq;
				}
			}
//			Vector3 rot = Vector3.up * rotateSpeed * Time.deltaTime;
//			for ( int i = 0; i < cwRotors.Length; i++ )
//				cwRotors [ i ].Rotate ( rot, Space.Self );
//			for ( int i = 0; i < ccwRotors.Length; i++ )
//				ccwRotors [ i ].Rotate ( -rot, Space.Self );
//			if ( tailRotor != null )
//				tailRotor.RotateAround ( tailPivot.position, -tailPivot.right, tailRotorSpeed * Time.deltaTime );
		}
	}
}