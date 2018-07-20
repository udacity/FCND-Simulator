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
		EditorGUI.BeginChangeCheck ();
		Vector3 newPos = EditorGUILayout.Vector3Field ( "Start", builder.LocalToWorld ( seg.start.position ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Start1" );
			seg.start.position = builder.WorldToLocal ( newPos );
			seg.radius = ( seg.start.position - seg.middle.position ).magnitude;
			seg.end.position = seg.Sample ( 1f );
			EditorUtility.SetDirty ( builder );
		}

		// center point
		EditorGUI.BeginChangeCheck ();
		newPos = EditorGUILayout.Vector3Field ( "Center", builder.LocalToWorld ( seg.middle.position ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Middle1" );
			seg.middle.position = builder.WorldToLocal ( newPos );
			seg.radius = ( seg.start.position - seg.middle.position ).magnitude;
			seg.end.position = seg.Sample ( 1f );
			EditorUtility.SetDirty ( builder );
		}

		// end point
//		EditorGUI.BeginChangeCheck ();
		GUI.enabled = false;
		newPos = EditorGUILayout.Vector3Field ( "End", builder.LocalToWorld ( seg.end.position ) );
		GUI.enabled = true;
//		if ( EditorGUI.EndChangeCheck () )
//		{
//			Undo.RecordObject ( builder, "End1" );
//			seg.end.position = builder.WorldToLocal ( newPos );
//			EditorUtility.SetDirty ( builder );
//		}


		// angle
		EditorGUI.BeginChangeCheck ();
		float angle = EditorGUILayout.Slider ( "Angle", seg.angle, 0, 360 );
//		float angle = EditorGUILayout.FloatField ( "Angle", seg.angle.ToString ( "F2" ) );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Angle1" );
			seg.angle = angle;
			seg.end.position = seg.Sample ( 1f );
			EditorUtility.SetDirty ( builder );
		}

		// sub-segments
		EditorGUI.BeginChangeCheck ();
		int subs = EditorGUILayout.IntField ( "Sub-segments", seg.subSegments );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Sub1" );
			seg.subSegments = subs;
			EditorUtility.SetDirty ( builder );
		}

		// radius
		EditorGUILayout.LabelField ( "Radius", seg.radius.ToString ( "F2" ) );

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
		Vector3 start = builder.LocalToWorld ( seg.start.position );
		Vector3 end = builder.LocalToWorld ( seg.end.position );
		Vector3 center = builder.LocalToWorld ( seg.middle.position );

		// draw the arc made up of segments
		int segCount = seg.subSegments - 1;
		Vector3 pos1 = builder.LocalToWorld ( seg.start.position );
		Vector3 pos2;
		Handles.color = Color.white;
		for (int i = 0; i < seg.subSegments; i++)
		{
			float t = 1f * ( i + 1 ) / seg.subSegments;
//			float t = 1f * i / segCount;
			pos2 = builder.LocalToWorld ( seg.Sample ( t ) );
			Handles.DrawAAPolyLine ( 6, pos1, pos2 );

			pos1 = pos2;
		}

		// draw radius lines
//		float size = HandleUtility.GetHandleSize ( start ) * handleSize;
		Handles.color = Color.yellow;
		float dashSize = 8;
//		Handles.drawaa
		Handles.DrawDottedLine ( start, center, dashSize );
		Handles.DrawDottedLine ( end, center, dashSize );
		Handles.DrawDottedLine ( start, end, dashSize );
		// and direction line
		Vector3 halfPoint = start + ( end - start ) * 0.5f;
		float size = HandleUtility.GetHandleSize ( halfPoint ) * handleSize;
		Handles.color = Color.red;
		Handles.ArrowHandleCap ( -1, halfPoint, Quaternion.LookRotation ( ( end - start ).normalized ), size * 2, EventType.Repaint );

		// start point
		Handles.color = Color.yellow;
		size = HandleUtility.GetHandleSize ( start ) * handleSize;
		EditorGUI.BeginChangeCheck ();
		Vector3 newPos = Handles.FreeMoveHandle ( start, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Start2" );
			seg.start.position = builder.WorldToLocal ( newPos );
			seg.radius = ( seg.start.position - seg.middle.position ).magnitude;
			seg.end.position = seg.Sample ( 1f );
			EditorUtility.SetDirty ( builder );
		}

		// end point
		Handles.color = new Color ( 1f, 0.6f, 0.1f );
		Handles.SphereHandleCap ( -1, end, Quaternion.identity, size, EventType.Repaint );
//		size = HandleUtility.GetHandleSize ( end ) * handleSize;
//		EditorGUI.BeginChangeCheck ();
//		newPos = Handles.FreeMoveHandle ( end, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
//		if ( EditorGUI.EndChangeCheck () )
//		{
//			Undo.RecordObject ( builder, "End2" );
//			seg.end.position = builder.WorldToLocal ( newPos );
//			EditorUtility.SetDirty ( builder );
//		}

		// center point
		Handles.color = Color.green;
		size = HandleUtility.GetHandleSize ( center ) * handleSize;
		EditorGUI.BeginChangeCheck ();
		newPos = Handles.FreeMoveHandle ( center, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "End" );
			seg.middle.position = builder.WorldToLocal ( newPos );
			seg.radius = ( seg.start.position - seg.middle.position ).magnitude;
			seg.end.position = seg.Sample ( 1f );
			EditorUtility.SetDirty ( builder );
		}

		Handles.color = Color.blue;
//		Handles.Disc ( Quaternion.LookRotation ( Vector3.up * 50 ), builder.LocalToWorld ( seg.middle.position ), Vector3.right, seg.radius, false, 0.5f );
	}
}