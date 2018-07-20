using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SplineBuilder : MonoBehaviour 
{
	public bool showTestPosition;
	public Vector3 testPosition;

	[HideInInspector]
	public Spline spline;

	Transform tr;

	void OnEnable ()
	{
		tr = transform;
	}

	public void AddControlPoint ()
	{
		ControlPoint cp = new ControlPoint ();
		if ( spline.Count == 0 )
		{
			Vector3 newPos = Vector3.zero;
			cp.position = newPos;
			cp.AddRightHandle ( newPos + Vector3.forward * 5 );
		} else
		{
			// get the last point
			ControlPoint p1 = spline.GetPoint ( -1 );
			if ( spline.Count > 1 )
			{
				ControlPoint p2 = spline.GetPoint ( -2 );
				Vector3 p2p1 = p1.position - p2.position;
				cp.position = p1.position + p2p1;
				cp.AddLeftHandle ( p2.position + p2p1 * 0.75f );
				p1.AddRightHandle ( p2.position + p2p1 * 0.25f );
			} else
			{
				cp.position = p1.position + Vector3.forward * 5;
				Vector3 p0p1 = cp.position - p1.position;
				cp.AddLeftHandle ( p1.position + p0p1 * 0.75f );
			}
		}

		spline.AddControlPoint ( cp );
	}

	public void InsertControlPoint (int index)
	{
		ControlPoint p1 = spline.GetPoint ( index );
		ControlPoint p2 = spline.GetPoint ( index + 1 );
		ControlPoint cp = new ControlPoint ();
		cp.position = spline.Sample ( p1, p2, 0.5f );
		cp.AddRightHandle ( cp.position + ( p1.position - cp.position ) * 0.25f );
		cp.AddLeftHandle ( cp.position + ( p2.position - cp.position ) * 0.25f );
		spline.InsertControlPoint ( index + 1, cp );
	}

	public void RemoveControlPoint (int index)
	{
		spline.RemoveControlPoint ( index );
	}

	public Vector3 SplineToWorldPosition (Vector3 point)
	{
		return tr.TransformPoint ( point );
	}

	public Vector3 WorldToSplinePosition (Vector3 point)
	{
		return tr.InverseTransformPoint ( point );
	}
}