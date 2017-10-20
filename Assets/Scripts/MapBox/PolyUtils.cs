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
		Vector2 ortho = Cross ( ( b - a ).normalized, d ) > 0 ? new Vector2 ( d.y, -d.x ) : new Vector2 ( -d.y, d.x );
		float denom = Vector2.Dot ( aToB, ortho );

		// here's a good place to check if denom == 0 meaning ray and line are parallel

		float t1 = Mathf.Abs ( Cross ( aToB, aToO ) ) / denom;
//		float t1 = Mathf.Abs ( aToB.x * aToO.y - aToO.x * aToB.y ) / denom;
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
		Vector2 ortho = Cross ( ( b - a ).normalized, d ) > 0 ? new Vector2 ( d.y, -d.x ) : new Vector2 ( -d.y, d.x );
		float denom = Vector2.Dot ( aToB, ortho );

		// here's a good place to check if denom == 0 meaning ray and line are parallel

		float t1 = Mathf.Abs ( Cross ( aToB, aToO ) ) / denom;
//		float t1 = Mathf.Abs ( aToB.x * aToO.y - aToO.x * aToB.y ) / denom;
		float t2 = Vector2.Dot ( aToO, ortho ) / denom;

		// if intersect, return the intersection point
		if ( t2 >= 0 && t2 <= 1 && t1 >= 0 )
			return o + d * t1;

		return new Vector2 ( Mathf.Infinity, Mathf.Infinity );
	}

	public static float Cross (Vector2 lhs, Vector2 rhs)
	{
		return lhs.x * rhs.y - lhs.y * rhs.x;
	}
}