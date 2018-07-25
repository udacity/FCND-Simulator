using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathSegmentType
{
	Linear,
//	Quadratic,
//	Cubic,
	Circular
}

[System.Serializable]
public class PathSegment
{
	public PathSegmentType type;
	public ControlPoint start;
	public ControlPoint middle;
	public ControlPoint end;
	public Quaternion axis;
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

//		case PathSegmentType.Quadratic:
//			return SampleQuadratic ( t );

//		case PathSegmentType.Cubic:
//			return SampleCubic ( t );

		case PathSegmentType.Circular:
			return SampleCircular ( t );
		}

		return Vector3.zero;
	}

	Vector3 SampleLinear (float t)
	{
		return ( 1 - t ) * start.position + t * end.position;
	}

/*	Vector3 SampleQuadratic (float t)
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
	}*/

	Vector3 SampleCircular (float t)
	{
		float radAngle = angle * t * Mathf.Deg2Rad;
		Vector3 unitPoint = new Vector3 ( Mathf.Sin ( radAngle ), 0, Mathf.Cos ( radAngle ) );
		return middle.position + axis * unitPoint * radius;
	}

	public bool TestPosition (Vector3 position, float testRadius)
	{
		if ( type == PathSegmentType.Linear )
		{
			Vector3 lineCenter = ( start.position + end.position ) / 2;
			Vector3 toPoint = position - lineCenter;
			float angle = Vector3.Angle ( end.position - start.position, toPoint );

			float pointToLineLength = toPoint.magnitude * Mathf.Sin ( angle * Mathf.Deg2Rad );
			if ( pointToLineLength <= testRadius )
				return true;

			return false;

		} else
		{
			Vector3 toPosition = position - middle.position;
			if ( toPosition.magnitude > radius + testRadius )
				return false;

			Vector3 sample = Sample ( 0.5f );
			float samplePositionAngle = Vector3.Angle ( sample - middle.position, toPosition );
			if ( samplePositionAngle <= angle * 0.5f )
				return true;

			return false;
		}
	}
}