using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SplineBuilder : MonoBehaviour 
{
//	class ControlPoint
//	{
//		public Vector3 position;
//		public Vector3 leftHandle;
//		public Vector3 rightHandle;
//		public bool hasLeftHandle;
//		public bool hasRightHandle;
//
//		public ControlPoint (Vector3 pos)
//		{
//			position = pos;
//			hasLeftHandle = false;
//			hasRightHandle = false;
//		}
//
//		public void AddLeftHandle (Vector3 handlePos)
//		{
//			hasLeftHandle = true;
//			leftHandle = handlePos;
//		}
//
//		public void AddRightHandle (Vector3 handlePos)
//		{
//			hasRightHandle = true;
//			rightHandle = handlePos;
//		}
//	}

	public LineRenderer line;
	public int subSegments = 1;

	[HideInInspector]
	public Spline spline;

	Transform tr;
	Vector3[] positions;
	ControlPoint[] cps;
	float nextUpdateTime;

	void OnEnable ()
	{
		tr = transform;
	}

//	#if !UNITY_EDITOR
//	void Awake ()
//	{
//		UpdateLine ();
//	}
//	#endif

//	void Update ()
//	{
//		if ( Time.time > nextUpdateTime )
//		{
//			nextUpdateTime = Time.time + 0.2f;
//			UpdateLine ();
//		}
//	}

	public void AddControlPoint ()
	{
		ControlPoint cp = new ControlPoint ();
		if ( spline.Count == 0 )
		{
			Camera cam = SceneView.lastActiveSceneView.camera;
			if ( cam == null )
				cam = SceneView.lastActiveSceneView.camera;
			Vector3 cameraPos = cam.transform.position;
			Vector3 forward = cam.transform.forward;
			Vector3 newPos = cameraPos + forward * 20;
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

	// older

	void UpdateLine ()
	{
		int childCount = tr.childCount;
		var list = new List<Vector3> ( 200 );
		if ( childCount >= 4 )
		{
			Vector3[] verts = new Vector3[childCount];
			for ( int i = 0; i < childCount; i++ )
				verts [ i ] = tr.GetChild ( i ).position;

			Vector3[] controlPoints = GetControlPoints ( verts );
//			Debug.Log ( "" + controlPoints.Length + " points" );
			for ( int i = 0; i < childCount - 1; i++ )
			{
				Vector3 p0 = verts [ i ];
				Vector3 c0 = controlPoints [ 2 * i ];
				Vector3 c1 = controlPoints [ 2 * i + 1 ];
				Vector3 p1 = verts [ i + 1 ];
				for ( int j = 0; j < subSegments; j++ )
					list.Add ( Bezier.GetPoint ( p0, c0, c1, p1, 1f * j / subSegments ) );
//				list.AddRange ( Curve4 ( verts [ i ], controlPoints [ 2 * i ], controlPoints [ 2 * i + 1 ], verts [ i + 1 ] ) );
			}
			
//			for ( int i = 0; i < childCount - 3; i++ )
//			{
//				float t = i / childCount;
//				Vector3 p0 = tr.GetChild ( i ).position;
//				Vector3 p1 = tr.GetChild ( i + 1 ).position;
//				Vector3 p2 = tr.GetChild ( i + 2 ).position;
//				Vector3 p3 = tr.GetChild ( i + 3 ).position;
//				for ( int j = 0; j < subSegments; j++ )
//				{
//					t += 1f * j / subSegments;
//					list.Add ( Bezier.GetPoint ( p0, p1, p2, p3, t ) );
//				}
//			}
//			Vector3 p0 = tr.GetChild ( 0 ).position;
//			for ( int i = 1; i < childCount; i += 3 )
//			{
//				Vector3 p1 = tr.GetChild ( i ).position;
//				Vector3 p2 = tr.GetChild ( i + 1 ).position;
//				Vector3 p3 = tr.GetChild ( i + 2 ).position;
//				for ( int j = 0; j < subSegments; j++ )
//					list.Add ( Bezier.GetPoint ( p0, p1, p2, p3, 1f * j / subSegments ) );
//				p0 = p3;
//			}
		}
		positions = list.ToArray ();
		line.SetPositions ( positions );
		line.positionCount = positions.Length;
	}

	// get all control points for a list of verts
	Vector3[] GetControlPoints (Vector3[] vertices)
	{
		var cpList = new List<ControlPoint> ( vertices.Length );
		for ( int i = 0; i < vertices.Length; i++ )
			cpList.Add ( new ControlPoint ( vertices [ i ] ) );

		Vector3[] centers = new Vector3[vertices.Length - 1];
		float[] edgeLengths = new float[vertices.Length - 1];
		for ( int i = 0; i < vertices.Length - 1; i++ )
		{
			centers [ i ] = ( vertices [ i ] + vertices [ i + 1 ] ) / 2;
			edgeLengths [ i ] = ( vertices [ i + 1 ] - vertices [ i ] ).magnitude;
		}

		Vector3[] innerEdges = new Vector3[centers.Length - 1];
		for ( int i = 0; i < centers.Length - 1; i++ )
		{
			innerEdges [ i ] = centers [ i + 1 ] - centers [ i ];
		}

		List<Vector3> points = new List<Vector3> ();
		// point 0 has no previous edge
		Vector3 p = vertices [ 0 ];
		Vector3 ctrl = centers [ 0 ];
		points.Add ( ctrl );
		cpList [ 0 ].AddRightHandle ( ctrl );

		for ( int i = 1; i < vertices.Length - 1; i++ )
		{
			float l1 = edgeLengths [ i - 1 ];
			float l2 = edgeLengths [ i - 1 ] + edgeLengths [ i ];
			float k = l1 / l2;
			p = vertices [ i ];
			Vector3 m = innerEdges [ i - 1 ];
			ctrl = p - m * k;
			points.Add ( ctrl );
			cpList [ i ].AddLeftHandle ( ctrl );

			ctrl = p + m * ( 1 - k );
			points.Add ( ctrl );
			cpList [ i ].AddRightHandle ( ctrl );
		}

		// point n also has no next edge
		p = vertices [ vertices.Length - 1 ];
		ctrl = centers [ centers.Length - 1 ];
		points.Add ( ctrl );
		cpList [ cpList.Count - 1 ].AddLeftHandle( ctrl );
		cps = cpList.ToArray ();

		return points.ToArray ();
	}

	// return control points for.... ?? i'm not sure which vertices of a polygon
	Vector3[] GetControlPoints (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		// Assume we need to calculate the control
		// points between (x1,y1) and (x2,y2).
		// Then x0,y0 - the previous vertex,
		//      x3,y3 - the next one.

		Vector3 c1 = ( p0 + p1 ) / 2f;
		Vector3 c2 = ( p1 + p2 ) / 2f;
		Vector3 c3 = ( p2 + p3 ) / 2f;

		float len1 = ( p1 - p0 ).magnitude;
		float len2 = ( p2 - p1 ).magnitude;
		float len3 = ( p3 - p2 ).magnitude;

		float k1 = len1 / ( len1 + len2 );
		float k2 = len2 / ( len2 + len3 );

		Vector3 m1 = c1 + ( c2 - c1 ) * k1;
		Vector3 m2 = c2 + ( c3 - c2 ) * k2;

		// Resulting control points. Here smooth_value is mentioned
		// above coefficient K whose value should be in range [0...1].
		float smoothValue = 1f;
		Vector3 ctrl1 = m1 + ( c2 - m1 ) * smoothValue + p1 - m1;
		Vector3 ctrl2 = m2 + ( c2 - m2 ) * smoothValue + p2 - m2;

		return new Vector3[] { ctrl1, ctrl2 };
	}

	List<Vector3> Curve4 (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		List<Vector3> l = new List<Vector3> ();

		// not used....?
//		Vector3 d1 = p1 - p0;
//		Vector3 d2 = p2 - p1;
//		Vector3 d3 = p3 - p2;

		float step1 = 1f / ( subSegments + 1 );
		float step2 = step1 * step1;
		float step3 = step2 * step1;

		float pre1 = 3f * step1;
		float pre2 = 3f * step2;
		float pre4 = 6f * step2;
		float pre5 = 6f * step3;

		Vector3 tmp1 = p0 - p1 * 2f + p3;
		Vector3 tmp2 = ( p1 - p2 ) * 3f - p0 + p3;

		Vector3 f = p0;
		Vector3 df = ( p1 - p0 ) * pre1 + tmp1 * pre2 + tmp2 * step3;
		Vector3 ddf = tmp1 * pre4 + tmp2 * pre5;
		Vector3 dddf = tmp2 * pre5;

		int step = subSegments;

		// Suppose, we have some abstract object Polygon which
		// has method AddVertex(x, y), similar to LineTo in
		// many graphical APIs.
		// Note, that the loop has only operation add!
		while ( step > 0 )
		{
			step--;
			f += df;
			df += ddf;
			ddf += dddf;
			l.Add ( f );
		}
		l.Add ( p3 );
		return l;
	}

//	void OnDrawGizmos ()
//	{
//		if ( cps == null || cps.Length == 0 )
//			return;
//		
//		int count = cps.Length;
//		Gizmos.color = Color.white;
//		ControlPoint cp;
//		for ( int i = 0; i < count; i++ )
//		{
//			cp = cps [ i ];
//			float size = HandleUtility.GetHandleSize ( cp.position ) * 0.1f;
//			Gizmos.DrawSphere ( cp.position, size * 2f );
//			if ( cp.hasLeftHandle )
//			{
//				Handles.color = Color.green;
//				Handles.DrawLine ( cp.position, cp.leftHandle );
//				Handles.CubeHandleCap ( 0, cp.leftHandle, Quaternion.identity, size, EventType.Repaint );
//			}
//			if ( cp.hasRightHandle )
//			{
//				Handles.color = Color.red;
//				Handles.DrawLine ( cp.position, cp.rightHandle );
//				Handles.CubeHandleCap ( 0, cp.rightHandle, Quaternion.identity, size, EventType.Repaint );
//			}
//		}
//	}
}