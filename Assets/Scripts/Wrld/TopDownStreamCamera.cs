using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownStreamCamera : MonoBehaviour
{
	public float height = 200;
	public Transform target;

	Transform tr;

	void Start ()
	{
		tr = transform;
		if ( target == null )
			target = GameObject.Find ( "FollowCam" ).transform;
		if ( target != null )
			tr.position = target.position + Vector3.up * height;
	}
	
	void LateUpdate ()
	{
		if ( target != null )
		{
			Vector3 euler = tr.eulerAngles;
			euler.y = target.eulerAngles.y;
			tr.eulerAngles = euler;
			tr.position = target.position + Vector3.up * height;
		}
	}
}