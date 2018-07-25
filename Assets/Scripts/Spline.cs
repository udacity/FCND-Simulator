using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ControlPoint
{
	public Vector3 position;
	public Vector3 leftHandle;
	public Vector3 rightHandle;
	public bool hasLeftHandle;
	public bool hasRightHandle;

	public ControlPoint ()
	{	
	}

	public ControlPoint (Vector3 pos)
	{
		position = pos;
		hasLeftHandle = false;
		hasRightHandle = false;
	}

	public void AddLeftHandle (Vector3 handlePos)
	{
		hasLeftHandle = true;
		leftHandle = handlePos;
	}

	public void AddRightHandle (Vector3 handlePos)
	{
		hasRightHandle = true;
		rightHandle = handlePos;
	}
}

[System.Serializable]
public class Spline
{
	public int Count { get { return controlPoints.Count; } }

	public List<ControlPoint> controlPoints;

	public Spline ()
	{
		controlPoints = new List<ControlPoint> ();
	}

	public void AddControlPoint (ControlPoint p)
	{
		controlPoints.Add ( p );
	}

	public void InsertControlPoint (int index, ControlPoint p)
	{
		controlPoints.Insert ( index, p );
	}

	public void RemoveControlPoint (int index)
	{
		controlPoints.RemoveAt ( index );
	}

	public void Clear ()
	{
		controlPoints.Clear ();
	}

	public ControlPoint GetPoint (int index)
	{
		int count = controlPoints.Count;
		if ( index >= count )
			index %= count;
		while ( index < 0 )
			index += count;
		
		return controlPoints [ index ];
	}

	public Vector3 Sample (float t)
	{
		int count = controlPoints.Count;
		if ( count == 0 )
			return Vector3.zero;
		if ( count == 1 )
			return controlPoints [ 0 ].position;
		
		ControlPoint p1;
		ControlPoint p2;

		for ( int i = 0; i < count - 1; i++ )
		{
			float t1 = 1f * i / count;
			float t2 = 1f * ( i + 1 ) / count;
			if ( t1 >= t && t2 <= t )
			{
				p1 = controlPoints [ i ];
				p2 = controlPoints [ i + 1 ];
				float tt = Mathf.InverseLerp ( t1, t2, t );
				return Sample ( p1, p2, tt );
//				return GetPoint ( p1.position, p1.rightHandle, p2.leftHandle, p2.position, tt );
			}
		}

		return Vector3.zero;
	}

	public Vector3 Sample (ControlPoint p1, ControlPoint p2, float t)
	{
		return GetPoint ( p1.position, p1.rightHandle, p2.leftHandle, p2.position, t );
	}

	Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		t = Mathf.Clamp01 ( t );
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * oneMinusT * p0 +
			3f * oneMinusT * oneMinusT * t * p1 +
			3f * oneMinusT * t * t * p2 +
			t * t * t * p3;
	}

	public Vector3 GetClosestPoint (Vector3 position)
	{
		ControlPoint cp1 = controlPoints [ 0 ];
		ControlPoint cp2 = controlPoints [ 0 ];
		float distance = Mathf.Infinity;
		for ( int i = 0; i < controlPoints.Count; i++ )
		{
			float dist = ( controlPoints [ i ].position - position ).sqrMagnitude;
			if ( dist < distance )
			{
				distance = dist;
				cp1 = controlPoints [ i ];
				if ( i == controlPoints.Count - 1 )
				{
					cp1 = controlPoints [ i - 1 ];
					cp2 = controlPoints [ i ];
				} else
					cp2 = controlPoints [ i + 1 ];
			}
		}

		distance = Mathf.Infinity;
		Vector3 curvePoint = Vector3.zero;

		int samples = 10;
		for ( int i = 0; i < samples; i++ )
		{
			float t = 1f * i / ( samples - 1 );
			
			Vector3 sample = Sample ( cp1, cp2, t );
			float dist = ( position - sample ).sqrMagnitude;
			if ( dist < distance )
			{
				distance = dist;
				curvePoint = sample;
			}
		}
		return curvePoint;
	}

	public Vector3 GetFirstDerivative (ControlPoint cp1, ControlPoint cp2, float t)
	{
		Vector3 p0 = cp1.position;
		Vector3 p1 = cp1.rightHandle;
		Vector3 p2 = cp2.leftHandle;
		Vector3 p3 = cp2.position;

		t = Mathf.Clamp01 ( t );
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * ( p1 - p0 ) +
			6f * oneMinusT * t * ( p2 - p1 ) +
			3f * t * t * ( p3 - p2 );
	}

	public Vector3 GetVelocity (Vector3 position, Vector3 derivative)
	{
		return derivative - position;
	}

	public Vector3 GetDirection (Vector3 position, Vector3 derivative)
	{
		return GetVelocity ( position, derivative ).normalized;
	}
}