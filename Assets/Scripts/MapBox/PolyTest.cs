using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PolyTest : MonoBehaviour
{
	[Range (0, 360)]
	public float angle = 90;

	public Transform origin;
	public Transform start;
	public Transform end;

	public Transform[] poly;
	public Transform testPoint;

	void OnEnable ()
	{
		if ( origin == null )
		{
			origin = GameObject.CreatePrimitive ( PrimitiveType.Sphere ).transform;
			origin.name = "origin";
			origin.localScale = Vector3.one * 0.05f;
			origin.GetComponent<Renderer> ().material.color = Color.yellow;
		}
		if ( start == null )
		{
			start = GameObject.CreatePrimitive ( PrimitiveType.Sphere ).transform;
			start.name = "start";
			start.localScale = Vector3.one * 0.05f;
			start.GetComponent<Renderer> ().material.color = Color.green;
		}
		if ( end == null )
		{
			end = GameObject.CreatePrimitive ( PrimitiveType.Sphere ).transform;
			end.name = "end";
			end.localScale = Vector3.one * 0.05f;
			end.GetComponent<Renderer> ().material.color = new Color ( 0, 0.5f, 0 );
		}
	}

	void OnDrawGizmos ()
	{
		if ( start == null || end == null )
			return;

		// intersect test

		Vector2 direction = new Vector2 ( Mathf.Sin ( angle * Mathf.Deg2Rad ), Mathf.Cos ( angle * Mathf.Deg2Rad ) );
		Gizmos.color = Color.green;
		Gizmos.DrawLine ( start.position, end.position );

		Gizmos.color = Color.magenta;
		Vector2 center = Vector3.Lerp ( start.position, end.position, 0.5f );
		Gizmos.DrawLine ( center, center + PolyUtils.Ortho ( start.position, end.position ) );

		Gizmos.color = Color.white;
		Gizmos.DrawRay ( origin.position, direction.normalized );

		Gizmos.color = Color.yellow;
		Vector2 intersect = PolyUtils.RayLineIntersectPoint ( origin.position, direction.normalized, start.position, end.position );
		Gizmos.DrawSphere ( intersect, 0.05f );
//		Gizmos.color = Color.cyan;
//		Vector2? intersection = PolyUtils.GetRayToLineSegmentIntersection ( origin.position, direction.normalized, start.position, end.position );
//		if ( intersection.HasValue )
//			Gizmos.DrawSphere ( new Vector3 ( intersection.Value.x, intersection.Value.y, 0 ), 0.05f );

		// point in poly test
		if ( poly != null && poly.Length > 0 && testPoint != null )
		{
			List<Tuple<Vector3, Vector3>> edges = new List<Tuple<Vector3, Vector3>> ();
			Vector2 dir = Vector2.right;

			for ( int i = 0; i < poly.Length; i++ )
				edges.Add ( new Tuple<Vector3, Vector3> ( poly [ i ].position, poly [ ( i + 1 ) % poly.Length ].position ) );

			foreach ( var seg in edges )
			{
				if ( PolyUtils.RayIntersectsLine ( testPoint.position, dir, seg.Item1, seg.Item2 ) )
					Gizmos.color = Color.green;
				else
					Gizmos.color = Color.red;
				Gizmos.DrawLine ( seg.Item1, seg.Item2 );
			}
		}

		List<Vector3> points = poly.Select ( ( x ) =>
		{
//			return x.position;
			return new Vector3 ( x.position.x, 0, x.position.y );
		} ).ToList ();

//		if ( PolyUtils.PointInPoly2D ( testPoint.position, points ) )
		if ( PolyUtils.PointInPoly2D ( new Vector3 ( testPoint.position.x, 0, testPoint.position.y ), points ) )
			testPoint.GetComponent<Renderer> ().material.color = Color.blue;
		else
			testPoint.GetComponent<Renderer> ().material.color = Color.white;
	}
}