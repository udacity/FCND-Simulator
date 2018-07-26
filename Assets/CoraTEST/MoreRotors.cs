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
	public Transform blurredTail;

	public bool autoRotate;
	public bool rotateTail;

//	public float rotationSpeed = 90;
//	public float tailRotationSpeed = 1200;

	[SerializeField]
	float rpm;
	[SerializeField]
	float frameRPM;
	public float blurRPM = 500;
	public float maxRPM = 2500;
	public float throttleAlpha = 0.1f;

	MeshRenderer[] rotorRenderers;
	MeshRenderer[] blurredRotorRenderers;

//	FastNoise fn;

	void Awake ()
	{
//		fn = new FastNoise ();
//		if ( autoRotate )
//		{
//			cwRotors.ForEach ( x => x.GetComponent<MeshRenderer> ().enabled = false );
//			ccwRotors.ForEach ( x => x.GetComponent<MeshRenderer> ().enabled = false );
//		}
		rotorRenderers = new MeshRenderer[cwRotors.Length * 2];
		for ( int i = 0; i < cwRotors.Length; i++ )
		{
			rotorRenderers [ i ] = cwRotors [ i ].GetComponent<MeshRenderer> ();
			rotorRenderers [ i + cwRotors.Length ] = ccwRotors [ i ].GetComponent<MeshRenderer> ();
		}
		blurredRotorRenderers = new MeshRenderer[blurredRotors.Length];
		for ( int i = 0; i < blurredRotors.Length; i++ )
			blurredRotorRenderers [ i ] = blurredRotors [ i ].GetComponent<MeshRenderer> ();

		rotorRenderers.ForEach ( x => x.enabled = false );
		blurredRotorRenderers.ForEach ( x => x.enabled = false );
	}

	void LateUpdate ()
	{
		if ( rpm > blurRPM )
			frameRPM = -0.5f + ( rpm - blurRPM ) / ( maxRPM - blurRPM );
		else
			frameRPM = rpm * 6f * Time.deltaTime;
		Rotate ( frameRPM, 0 );
	}

	void Rotate (float speed, float tailSpeed)
	{
		Vector3 right = Vector3.up * speed;
		Vector3 left = -Vector3.up * speed;
		for ( int i = 0; i < cwRotors.Length; i++ )
		{
			cwRotors [ i ].Rotate ( right, Space.Self );
			ccwRotors [ i ].Rotate ( left, Space.Self );
		}
		if ( rotateTail )
			tailRotor.Rotate ( -Vector3.right * tailSpeed );
	}

	public void SetRPM (float r)
	{
		rpm = ( 1 - throttleAlpha ) * rpm + throttleAlpha * r;
		bool rotorsEnabled = rotorRenderers [ 0 ].enabled;
		if ( rpm > blurRPM )
		{
			if ( rotorsEnabled )
			{
				rotorRenderers.ForEach ( x => x.enabled = false );
				blurredRotorRenderers.ForEach ( x => x.enabled = true );
			}
		} else
		{
			if ( !rotorsEnabled )
			{
				rotorRenderers.ForEach ( x => x.enabled = true );
				blurredRotorRenderers.ForEach ( x => x.enabled = false );
			}
		}
	}
}