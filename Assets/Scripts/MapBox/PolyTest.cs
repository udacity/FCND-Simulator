using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PolyTest : MonoBehaviour
{
	public Vector2 origin;
	public Vector2 direction;

	public Vector2 a;
	public Vector2 b;

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawRay ( origin, direction.normalized );

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine ( a, b );

		Gizmos.color = Color.green;
//		if ( PolyUtils.RayIntersectsLine ( origin, direction.normalized, a, b ) )
//			Gizmos.DrawSphere ( Vector3.zero, 0.05f );
		Vector2 intersect = PolyUtils.RayLineIntersectPoint ( origin, direction.normalized, a, b );
		Gizmos.DrawSphere ( intersect, 0.05f );
	}
}