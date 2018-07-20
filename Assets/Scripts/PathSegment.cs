using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathSegmentType
{
	Linear,
	Quadratic,
	Cubic,
	Circular
}

[System.Serializable]
public class PathSegment
{
	public PathSegmentType type;
	public ControlPoint start;
	public ControlPoint middle;
	public ControlPoint end;
	public float angle;
	public float radius;
	public int subSegments = 20;

	public Vector3 Sample (float t)
	{
		t = Mathf.Clamp01 ( t );

		switch ( type )
		{
		case PathSegmentType.Linear:
			return SampleLinear ( t );

		case PathSegmentType.Quadratic:
			return SampleQuadratic ( t );

		case PathSegmentType.Cubic:
			return SampleCubic ( t );

		case PathSegmentType.Circular:
			return SampleCircular ( t );
		}

		return Vector3.zero;
	}

	Vector3 SampleLinear (float t)
	{
		return ( 1 - t ) * start.position + t * end.position;
	}

	Vector3 SampleQuadratic (float t)
	{
		float oneMinusT = 1f - t;
		return oneMinusT * oneMinusT * start.position +
			2f * oneMinusT * t * middle.position +
			t * t * end.position;
	}

	Vector3 SampleCubic (float t)
	{
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * oneMinusT * start.position +
			3f * oneMinusT * oneMinusT * t * start.rightHandle +
			3f * oneMinusT * t * t * end.leftHandle +
			t * t * t * end.position;
	}

	Vector3 SampleCircular (float t)
	{
		Vector3 toStart = ( start.position - middle.position ).normalized;
		Vector3 toEnd = ( end.position - middle.position ).normalized;
		Vector3 to180 = -toStart;

		if ( angle <= 180f )
			return middle.position + Vector3.Slerp ( toStart, to180, t * angle / 180 ) * radius;
//			return middle.position + Vector3.Slerp ( toStart, toEnd, t ) * radius;

		float newAngle = t * angle;

		if ( newAngle <= 180f )
//		if ( t <= 0.5f )
			return middle.position + Vector3.Slerp ( toStart, to180, t * 2 * angle / 360 ) * radius;

		float newNewAngle = 180f - newAngle;

		return middle.position + Vector3.SlerpUnclamped ( toStart, to180, -t ) * radius;
//		float remainingAngle = newAngle - 180f;
//		return middle.position + Vector3.RotateTowards ( toStart, to180, remainingAngle * Mathf.Deg2Rad, 1 ) * radius;
//		return middle.position + Vector3.RotateTowards ( toStart, to180,  * Mathf.Deg2Rad, 1 ) * radius; // + Vector3.Slerp ( toStart, to180, -0.25f ) * radius;
//		return middle.position + Vector3.Slerp ( toStart, to180, -( 1f - t ) * ( angle - newAngle ) / 180 ) * radius;

//		float t180 = t * ( 180f / angle );
//
//		Vector3 point = Vector3.Slerp ( toStart, -toStart, t180 );
//		point = middle.position + Vector3.Slerp ( point, toEnd, t - t180 ) * radius;
//		Vector3 point = middle.position + radius * ( toStart * Mathf.Cos ( radAngle ) + toEnd * Mathf.Sin ( radAngle ) );
//		Debug.Log ( point );
//		return point;
//
//		Vector3 lerp = ( 1 - t ) * start.position + t * end.position;
//		return middle.position + ( lerp - middle.position ).normalized * radius;
	}
}