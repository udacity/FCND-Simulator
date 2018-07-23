using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (PathBuilder))]
public class PathEditor : Editor
{
	PathBuilder builder;
	int selectedIndex = -1;
	bool isCtrlHeld;
	float handleSize = 0.2f;

	public override void OnInspectorGUI ()
	{
		builder = target as PathBuilder;
		int count = builder.segments.Count;
		PathSegment seg;

		for ( int i = 0; i < count; i++ )
		{
			seg = builder.segments [ i ];
			EditorGUILayout.LabelField ( "Segment " + ( i + 1 ) );
			EditorGUI.indentLevel++;
			EditorGUI.BeginChangeCheck ();
			PathSegmentType newType = (PathSegmentType) EditorGUILayout.EnumPopup ( "Segment Type:", seg.type );
			if ( EditorGUI.EndChangeCheck () )
			{
				Undo.RecordObject ( builder, "Seg Type " + i );
				seg.type = newType;
				EditorUtility.SetDirty ( builder );
			}

			switch ( seg.type )
			{
			case PathSegmentType.Linear:
				LinearSegmentInspector ( seg );
				break;

			case PathSegmentType.Quadratic:
				break;

			case PathSegmentType.Cubic:
				break;

			case PathSegmentType.Circular:
				CircularSegmentInspector ( seg );
				break;
			}
			EditorGUI.indentLevel--;
		}
		
		if ( GUILayout.Button ( "Add Segment" ) )
		{
			Undo.RecordObject ( builder, "Add Segment" );
			builder.AddSegment ( PathSegmentType.Linear );
			EditorUtility.SetDirty ( builder );
		}

		// material
		EditorGUI.BeginChangeCheck ();
		EditorGUILayout.ObjectField ( serializedObject.FindProperty ( "lineMaterial" ) );
//		Material m = EditorGUILayout.ObjectField ( serializedObject.FindProperty ( "lineMaterial" ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Material" );
			serializedObject.ApplyModifiedProperties ();
//			builder.lineMaterial = m;
			EditorUtility.SetDirty ( builder );
		}


		if ( GUILayout.Button ( "Build" ) )
		{
			Vector3[] positions;
			Transform t;
			LineRenderer l;
			Undo.RecordObject ( builder, "Build" );
			int childCount = builder.tr.childCount;
			List<GameObject> children = new List<GameObject> ();
			foreach ( Transform c in builder.tr )
				children.Add ( c.gameObject );
			for ( int i = children.Count - 1; i >= 0; i-- )
				DestroyImmediate ( children [ i ] );
			
			for ( int i = 0; i < count; i++ )
			{
				GameObject g = new GameObject ( "Segment" );
				g.transform.SetParent ( builder.tr );
				l = g.AddComponent<LineRenderer> ();
				l.sharedMaterial = builder.lineMaterial;
				l.widthMultiplier = 0.3f;
				seg = builder.segments [ i ];
				if ( seg.type == PathSegmentType.Linear )
				{
					positions = new Vector3[2] {
						builder.LocalToWorld ( seg.start.position ),
						builder.LocalToWorld ( seg.end.position )
					};
					l.positionCount = 2;
					l.SetPositions ( positions );
				} else
				{
					int posCount = (int) seg.angle + 1;
					positions = new Vector3[posCount];
					for ( int p = 0; p < posCount; p++ )
					{
						float tt = 1f * p / posCount;
//						positions [ p ] = seg.Sample ( tt );
						positions [ p ] = builder.LocalToWorld ( seg.Sample ( tt ) );
//						Debug.Log ( "setting position " + p + " at " + tt + " to " + positions [ p ] );
//						GameObject gg = GameObject.CreatePrimitive ( PrimitiveType.Sphere );
//						gg.transform.position = positions [ p ];
					}
					l.positionCount = posCount;
					l.SetPositions ( positions );
				}
			}
		}

/*		EditorGUI.BeginChangeCheck ();
		bool showTest = EditorGUILayout.ToggleLeft ( "Show Test Position", builder.showTestPosition );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Show Test" );
			builder.showTestPosition = showTest;
			EditorUtility.SetDirty ( builder );
		}*/
	}

	void LinearSegmentInspector (PathSegment seg)
	{
		// start point
		EditorGUI.BeginChangeCheck ();
		Vector3 newPos = EditorGUILayout.Vector3Field ( "Start", builder.LocalToWorld ( seg.start.position ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Start1" );
			seg.start.position = builder.WorldToLocal ( newPos );
			EditorUtility.SetDirty ( builder );
		}

		// end point
		EditorGUI.BeginChangeCheck ();
		newPos = EditorGUILayout.Vector3Field ( "End", builder.LocalToWorld ( seg.end.position ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "End1" );
			seg.end.position = builder.WorldToLocal ( newPos );
			EditorUtility.SetDirty ( builder );
		}
	}

	void CircularSegmentInspector (PathSegment seg)
	{
		// start point
//		GUI.enabled = false;
//		EditorGUI.BeginChangeCheck ();
//		Vector3 newPos = EditorGUILayout.Vector3Field ( "Start", builder.LocalToWorld ( seg.start.position ) );
//		if ( EditorGUI.EndChangeCheck () )
//		{
//			Undo.RecordObject ( builder, "Start1" );
//			seg.start.position = builder.WorldToLocal ( newPos );
//			seg.radius = ( seg.start.position - seg.middle.position ).magnitude;
//			seg.end.position = seg.Sample ( 1f );
//			EditorUtility.SetDirty ( builder );
//		}

		// center point
		EditorGUI.BeginChangeCheck ();
		Vector3 newPos = EditorGUILayout.Vector3Field ( "Center", builder.LocalToWorld ( seg.middle.position ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Middle1" );
			seg.middle.position = builder.WorldToLocal ( newPos );
			seg.radius = ( seg.start.position - seg.middle.position ).magnitude;
			seg.end.position = seg.Sample ( 1f );
			seg.start.position = seg.Sample ( 0 );
			EditorUtility.SetDirty ( builder );
		}

		// euler axis
		EditorGUI.BeginChangeCheck ();
		Vector3 newAxis = EditorGUILayout.Vector3Field ( "Euler Axis", seg.axis.eulerAngles );
//		newPos = EditorGUILayout.Vector3Field ( "Axis", builder.LocalToWorld ( seg.end.position ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Axis" );
			seg.axis = Quaternion.Euler ( newAxis );
//			seg.end.position = builder.WorldToLocal ( newPos );
			EditorUtility.SetDirty ( builder );
		}


		// angle
		EditorGUI.BeginChangeCheck ();
		float angle = EditorGUILayout.Slider ( "Angle", seg.angle, 0, 360 );
//		float angle = EditorGUILayout.FloatField ( "Angle", seg.angle.ToString ( "F2" ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Angle1" );
			seg.angle = angle;
			EditorUtility.SetDirty ( builder );
		}

		// sub-segments
//		EditorGUI.BeginChangeCheck ();
//		int subs = EditorGUILayout.IntField ( "Sub-segments", seg.subSegments );
//		if ( EditorGUI.EndChangeCheck () )
//		{
//			Undo.RecordObject ( builder, "Sub1" );
//			seg.subSegments = subs;
//			EditorUtility.SetDirty ( builder );
//		}

		// radius
		EditorGUI.BeginChangeCheck ();
		float radius = EditorGUILayout.FloatField ( "Radius", seg.radius );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Radius" );
			if ( radius < 0 )
				radius = -radius;
			seg.radius = radius;
			EditorUtility.SetDirty ( builder );
		}

		// arc angle (degrees)
/*		EditorGUI.BeginChangeCheck ();
		float angle = EditorGUILayout.FloatField ( "Arc Angle", seg.angle );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Angle1" );
			seg.angle = angle;
			EditorUtility.SetDirty ( builder );
		}*/

		// radius
/*		EditorGUI.BeginChangeCheck ();
		newPos = EditorGUILayout.Vector3Field ( "End", builder.LocalToWorld ( seg.end.position ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "End1" );
			seg.end.position = builder.WorldToLocal ( newPos );
			EditorUtility.SetDirty ( builder );
		}*/
	}

	void OnSceneGUI ()
	{
		builder = target as PathBuilder;

		if ( Event.current.isKey )
			isCtrlHeld = ( Event.current.keyCode == KeyCode.LeftControl || Event.current.keyCode == KeyCode.RightControl );

		int count = builder.segments.Count;
		PathSegment seg;

		for ( int i = 0; i < count; i++ )
		{
			seg = builder.segments [ i ];
			switch (seg.type)
			{
			case PathSegmentType.Linear:
				DrawLinearPath ( seg );
				break;

			case PathSegmentType.Circular:
				DrawArc ( seg );
				break;
			}
		}
	}

/*	void DrawSelectedPointInspector ()
	{
		GUILayout.Label ( "Selected Point" );
		EditorGUI.BeginChangeCheck ();
		Vector3 point = EditorGUILayout.Vector3Field ( "Position", spline.controlPoints [ selectedIndex ].position );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Move Point" );
			EditorUtility.SetDirty ( builder );
			spline.controlPoints [ selectedIndex ].position = point;
		}
	}*/

	void DrawLinearPath (PathSegment seg)
	{
		Vector3 start = builder.LocalToWorld ( seg.start.position );
		Vector3 end = builder.LocalToWorld ( seg.end.position );
		Handles.color = Color.white;
		Handles.DrawAAPolyLine ( 6, start, end );

		// direction arrow
		Vector3 halfPoint = start + ( end - start ) * 0.5f;
		float size = HandleUtility.GetHandleSize ( halfPoint ) * handleSize;
		Handles.color = Color.red;
		Handles.ArrowHandleCap ( -1, halfPoint, Quaternion.LookRotation ( ( end - start ).normalized ), size * 2, EventType.Repaint );

		// start point
		size = HandleUtility.GetHandleSize ( start ) * handleSize;
		Handles.color = Color.yellow;
		EditorGUI.BeginChangeCheck ();
		Vector3 newPos = Handles.FreeMoveHandle ( start, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Start" );
			seg.start.position = builder.WorldToLocal ( newPos );
			EditorUtility.SetDirty ( builder );
		}

		// end point
		size = HandleUtility.GetHandleSize ( end ) * handleSize;
		EditorGUI.BeginChangeCheck ();
		newPos = Handles.FreeMoveHandle ( end, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "End" );
			seg.end.position = builder.WorldToLocal ( newPos );
			EditorUtility.SetDirty ( builder );
		}
	}

	void DrawArc (PathSegment seg)
	{
		Vector3 start = builder.LocalToWorld ( seg.Sample ( 0f ) );
		Vector3 end = builder.LocalToWorld ( seg.Sample ( 1f ) );
//		Vector3 start = builder.LocalToWorld ( seg.start.position );
//		Vector3 end = builder.LocalToWorld ( seg.end.position );
		Vector3 center = builder.LocalToWorld ( seg.middle.position );
		Vector3 newPos;

		// draw the arc made up of segments
		int segCount = (int) seg.angle;// - 1;
//		int segCount = seg.subSegments - 1;
		Vector3 pos1 = start;
		Vector3 pos2;
		Handles.color = Color.white;
		for ( int i = 0; i < segCount; i++ )
//		for (int i = 0; i < seg.subSegments; i++)
		{
//			float t = 1f * ( i + 1 ) / seg.subSegments;
			float t = 1f * ( i + 1 ) / segCount;
			pos2 = builder.LocalToWorld ( seg.Sample ( t ) );
			Handles.DrawAAPolyLine ( 6, pos1, pos2 );

			pos1 = pos2;
		}

		// draw radius lines
//		float size = HandleUtility.GetHandleSize ( start ) * handleSize;
		Handles.color = Color.yellow;
		float dashSize = 8;
		Handles.DrawDottedLine ( start, center, dashSize );
		Handles.DrawDottedLine ( end, center, dashSize );
//		Handles.DrawDottedLine ( start, end, dashSize );
		// and direction line
//		Vector3 halfPoint = start + ( end - start ) * 0.5f;
//		float size = HandleUtility.GetHandleSize ( halfPoint ) * handleSize;
		float size;
//		Handles.color = Color.red;
//		Handles.ArrowHandleCap ( -1, halfPoint, Quaternion.LookRotation ( ( end - start ).normalized ), size * 2, EventType.Repaint );

		// start point
		size = HandleUtility.GetHandleSize ( start ) * handleSize;
		Handles.color = new Color ( 1f, 0.75f, 0.25f );
		Handles.SphereHandleCap ( -1, start, Quaternion.identity, size, EventType.Repaint );
//		Handles.color = Color.yellow;
//		size = HandleUtility.GetHandleSize ( start ) * handleSize;
//		EditorGUI.BeginChangeCheck ();
//		Vector3 newPos = Handles.FreeMoveHandle ( start, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
//		if ( EditorGUI.EndChangeCheck () )
//		{
//			Undo.RecordObject ( builder, "Start2" );
//			seg.start.position = builder.WorldToLocal ( newPos );
//			seg.radius = ( seg.start.position - seg.middle.position ).magnitude;
//			seg.end.position = seg.Sample ( 1f );
//			EditorUtility.SetDirty ( builder );
//		}

		// end point
		Handles.color = new Color ( 1f, 0.6f, 0.1f );
		Handles.SphereHandleCap ( -1, end, Quaternion.identity, size, EventType.Repaint );
//		Handles.SphereHandleCap ( -1, end, Quaternion.identity, size, EventType.Repaint );
//		size = HandleUtility.GetHandleSize ( end ) * handleSize;
//		EditorGUI.BeginChangeCheck ();
//		newPos = Handles.FreeMoveHandle ( end, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
//		if ( EditorGUI.EndChangeCheck () )
//		{
//			Undo.RecordObject ( builder, "End2" );
//			seg.end.position = builder.WorldToLocal ( newPos );
//			EditorUtility.SetDirty ( builder );
//		}

		// line from center to axis
		Handles.color = Color.red;
		Vector3 axisPoint = center + seg.axis * Vector3.up * seg.radius * 0.25f;
		Handles.DrawDottedLine ( center, axisPoint, dashSize );

		// center point
		Handles.color = Color.green;
		size = HandleUtility.GetHandleSize ( center ) * handleSize;
		EditorGUI.BeginChangeCheck ();
		newPos = Handles.FreeMoveHandle ( center, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Middle" );
			seg.middle.position = builder.WorldToLocal ( newPos );
			seg.radius = ( seg.start.position - seg.middle.position ).magnitude;
//			seg.end.position = seg.Sample ( 1f );
			EditorUtility.SetDirty ( builder );
		}

		// axis ring
		Handles.color = Color.cyan;
		size = HandleUtility.GetHandleSize ( center ) * handleSize;
		EditorGUI.BeginChangeCheck ();
		Quaternion q = Handles.FreeRotateHandle ( seg.axis, center, seg.radius * 0.25f );
//		newPos = Handles.FreeMoveHandle ( end, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Axis" );
			seg.axis = q;
//			seg.end.position = builder.WorldToLocal ( newPos );//.normalized * seg.radius );
			EditorUtility.SetDirty ( builder );
		}

		// axis point
		Handles.color = Color.red;
		size = HandleUtility.GetHandleSize ( center ) * handleSize;
		Handles.SphereHandleCap ( -1, axisPoint, Quaternion.identity, size, EventType.Repaint );
//		EditorGUI.BeginChangeCheck ();
//		newPos = Handles.FreeMoveHandle ( end, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
//		if ( EditorGUI.EndChangeCheck () )
//		{
//			Undo.RecordObject ( builder, "Axis" );
//			seg.axis = q;
//			seg.end.position = builder.WorldToLocal ( newPos );//.normalized * seg.radius );
//			EditorUtility.SetDirty ( builder );
//		}
	}
}