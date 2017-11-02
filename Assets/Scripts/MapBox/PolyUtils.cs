using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyUtils
{
	public static bool RayIntersectsLine (Ray2D ray, Vector2 a, Vector2 b)
	{
		return RayIntersectsLine ( ray.origin, ray.direction.normalized, a, b );
	}

	public static bool RayIntersectsLine (Vector2 o, Vector2 d, Vector2 a, Vector2 b)
	{
		Vector2 aToO = o - a;
		Vector2 aToB = b - a;
		Vector2 ortho = new Vector2 ( -d.y, d.x );
//		Vector2 ortho = Cross ( ( b - a ).normalized, d ) > 0 ? new Vector2 ( d.y, -d.x ) : new Vector2 ( -d.y, d.x );
		float denom = Vector2.Dot ( aToB, ortho );

		// here's a good place to check if denom == 0 meaning ray and line are parallel

		float t1 = Cross ( aToB, aToO ) / denom;
		float t2 = Vector2.Dot ( aToO, ortho ) / denom;

		return t2 >= 0 && t2 <= 1 && t1 >= 0;
	}

	public static Vector2 RayLineIntersectPoint (Ray2D ray, Vector2 a, Vector2 b)
	{
		return RayLineIntersectPoint ( ray.origin, ray.direction.normalized, a, b );
	}

	public static Vector2 RayLineIntersectPoint (Vector2 o, Vector2 d, Vector2 a, Vector2 b)
	{
		d = d.normalized;
		Vector2 aToO = o - a;
		Vector2 aToB = b - a;
		Vector2 ortho = new Vector2 ( -d.y, d.x );
//		Vector2 ortho = Cross ( ( b - a ).normalized, d ) > 0 ? new Vector2 ( d.y, -d.x ) : new Vector2 ( -d.y, d.x );
//		Debug.Log ( "cross is " + Cross ( ( b - a ).normalized, d ) );
		float denom = Vector2.Dot ( aToB, ortho );
//		Debug.Log ( "denom is " + denom );
		// here's a good place to check if denom == 0 meaning ray and line are parallel

		float t1 = Cross ( aToB, aToO ) / denom;
		float t2 = Vector2.Dot ( aToO, ortho ) / denom;

		// if intersect, return the intersection point
		if ( t2 >= 0 && t2 <= 1 && t1 >= 0 )
			return o + d * t1;

		return new Vector2 ( Mathf.Infinity, Mathf.Infinity );
	}

//	public static bool RayIntersectsLine

	public static Vector2? GetRayToLineSegmentIntersection (Vector2 rayOrigin, Vector2 rayDirection, Vector2 point1, Vector2 point2)
	{
		Vector2 v1 = rayOrigin - point1;
		Vector2 v2 = point2 - point1;
		Vector2 v3 = new Vector2 ( -rayDirection.y, rayDirection.x );

		var dot = Vector2.Dot ( v2, v3 );
//		if ( Mathf.Abs ( dot ) < 0.000001 )
//			return null;

		float t1 = Cross ( v2, v1 ) / dot;
		float t2 = ( Vector2.Dot ( v1, v3 ) ) / dot;

		if ( t1 >= 0f && ( t2 >= 0f && t2 <= 1f ) )
			return rayOrigin + rayDirection * t1;
//			return t1;

		return null;
	}

	public static float Cross (Vector2 lhs, Vector2 rhs)
	{
		return lhs.x * rhs.y - lhs.y * rhs.x;
	}

	public static Vector2 Ortho (Vector2 a, Vector2 b)
	{
		Vector2 v = ( b - a ).normalized;
		return new Vector2 ( v.y, -v.x );
	}

	// check if a point is inside a poly in 2d (XZ). for now assumes a simple poly with no holes and uses Even-Odd rule
	public static bool PointInPoly2D (Vector3 point, List<Vector3> poly)
	{
		Vector2 pt = new Vector2 ( point.x, point.z );
		Vector2 direction = Vector2.right;
		int vertCount = poly.Count;
		if ( poly [ 0 ].x == poly [ poly.Count - 1 ].x && poly [ 0 ].z == poly [ poly.Count - 1 ].z )
			vertCount--;
		int intersectCount = 0;

		bool lastEdge = false;

		for ( int i = 0; i < vertCount; i++ )
		{
			int next = ( i + 1 ) % vertCount;
			Vector2 a = new Vector2 ( poly [ i ].x, poly [ i ].z );
			Vector2 b = new Vector2 ( poly [ next ].x, poly [ next ].z );
			if ( RayIntersectsLine ( pt, direction, a, b ) )
			{
//				Debug.DrawRay ( new Vector3 ( pt.x, 2, pt.y ), new Vector3 ( direction.x, 0, direction.y ), Color.blue, 5, false );
//				Debug.DrawLine ( new Vector3 ( a.x, 2, a.y ), new Vector3 ( b.x, 2, b.y ), Color.red, 5, false );
//				Debug.DrawRay ( new Vector3 ( pt.x, 2, pt.y ), Vector3.up, Color.green, 5, false );
				intersectCount++;
				if ( lastEdge )
					intersectCount++;
				lastEdge = true;
			} else
				lastEdge = false;
		}
//		Debug.Log ( "intersectcount " + intersectCount );

		return intersectCount % 2 != 0;
	}
}