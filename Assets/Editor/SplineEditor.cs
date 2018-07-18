using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (SplineBuilder))]
public class SplineEditor : Editor
{
	SplineBuilder builder;
	Spline spline;
	int selectedIndex = -1;
	bool isCtrlHeld;

	public override void OnInspectorGUI ()
	{
		builder = target as SplineBuilder;
		spline = builder.spline;
		// show the selected point in the inspector
		if ( selectedIndex >= 0 && selectedIndex < spline.Count )
			DrawSelectedPointInspector ();

		for ( int i = 0; i < spline.Count; i++ )
		{
			EditorGUILayout.BeginHorizontal ();
			if ( GUILayout.Button ( "Point " + i, EditorStyles.label ) )
			{
				SceneView.lastActiveSceneView.LookAt ( spline.controlPoints [ i ].position );
			}
//			EditorGUILayout.LabelField ( "Point " + i );
			GUILayout.FlexibleSpace ();
			if ( i < spline.Count - 1 && GUILayout.Button ( "+" ) )
			{
				Undo.RecordObject ( builder, "Insert Point" );
				builder.InsertControlPoint ( i );
				EditorUtility.SetDirty ( builder );
			}
			if ( GUILayout.Button ( "x" ) )
			{
				Undo.RecordObject ( builder, "Remove Point" );
				builder.RemoveControlPoint ( i );
				EditorUtility.SetDirty ( builder );
			}
			EditorGUILayout.EndHorizontal ();
		}
		
		if ( GUILayout.Button ( "Add Control Point" ) )
		{
			Undo.RecordObject ( builder, "Add CP" );
			builder.AddControlPoint ();
			EditorUtility.SetDirty ( builder );
		}
	}

	void OnSceneGUI ()
	{
		builder = target as SplineBuilder;
		spline = builder.spline;

		if ( Event.current.isKey )
			isCtrlHeld = ( Event.current.keyCode == KeyCode.LeftControl || Event.current.keyCode == KeyCode.RightControl );

		int count = spline.Count;
		if ( count > 1 )
		{
			for ( int i = 0; i < count - 1; i++ )
				DrawCurve ( spline.controlPoints [ i ], spline.controlPoints [ i + 1 ] );
		}

		for ( int i = 0; i < spline.Count; i++ )
		{
			if ( DrawControlPoint ( spline.controlPoints [ i ], i == selectedIndex ) )
			{
				selectedIndex = i;
			}
		}
	}

	void DrawSelectedPointInspector ()
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
	}

	bool DrawControlPoint (ControlPoint point, bool selected)
	{
		float size = HandleUtility.GetHandleSize ( point.position ) * 0.2f;

		// only draw movement controls if point is selected
		if ( selected )
		{
			Handles.color = Color.yellow;
			if ( point.hasLeftHandle )
				Handles.DrawLine ( point.position, point.leftHandle );
			if ( point.hasRightHandle )
				Handles.DrawLine ( point.position, point.rightHandle );

			// draw main node
			Vector3 snap = Vector3.one * 0.5f;
			EditorGUI.BeginChangeCheck ();
			Vector3 pos = Handles.FreeMoveHandle ( point.position, Quaternion.identity, size * 2, snap, Handles.SphereHandleCap );
			if ( EditorGUI.EndChangeCheck () )
			{
				Vector3 move = pos - point.position;
				Undo.RecordObject ( builder, "Move Point" );
				point.position = pos;
				if ( isCtrlHeld )
				{
					point.leftHandle += move;
					point.rightHandle += move;
				}
				EditorUtility.SetDirty ( builder );
			}

			// draw left handle if we have one
			if ( point.hasLeftHandle )
			{
				EditorGUI.BeginChangeCheck ();
				pos = Handles.FreeMoveHandle ( point.leftHandle, Quaternion.identity, size, snap, Handles.CubeHandleCap );
				if ( EditorGUI.EndChangeCheck () )
				{
					Undo.RecordObject ( builder, "Move Left Handle" );
					point.leftHandle = pos;
					EditorUtility.SetDirty ( builder );
				}
			}

			// draw the right handle if we have one
			if ( point.hasRightHandle )
			{
				Handles.color = Color.yellow;
				EditorGUI.BeginChangeCheck ();
				pos = Handles.FreeMoveHandle ( point.rightHandle, Quaternion.identity, size, snap, Handles.CubeHandleCap );
				if ( EditorGUI.EndChangeCheck () )
				{
					Undo.RecordObject ( builder, "Move Right Handle" );
					point.rightHandle = pos;
					EditorUtility.SetDirty ( builder );
				}
			}

		} else
		{
			Handles.color = Color.white;
			if ( Handles.Button ( point.position, Quaternion.identity, size, size, Handles.SphereHandleCap ) )
			{
				Repaint ();
				return true;
			}
		}

		return selected;
	}

	void DrawCurve (ControlPoint p1, ControlPoint p2)
	{
		Handles.DrawBezier ( p1.position, p2.position, p1.rightHandle, p2.leftHandle, Color.white, null, 6 );
	}

/*	Vector3 ShowPoint (int index)
	{
		var cp = spline.controlPoints [ index ];
		Vector3 point = cp.position;
		float size = HandleUtility.GetHandleSize ( point ) * 0.1f;
		if ( index == selectedIndex )
		{
			size *= 2f;
			Handles.color = Color.yellow;
		} else
			Handles.color = Color.white;
		if ( Handles.Button ( point, Quaternion.identity, size, size, Handles.DotHandleCap ) )
		{
			selectedIndex = index;
			Repaint ();
		}
		if ( selectedIndex == index )
		{
			EditorGUI.BeginChangeCheck ();
			point = Handles.DoPositionHandle ( point, Quaternion.identity );
			if ( EditorGUI.EndChangeCheck () )
			{
				Undo.RecordObject ( builder, "Move Point" );
				EditorUtility.SetDirty ( builder );
				spline.controlPoints [ index ].position = point;
			}
		}
		return point;
	}*/
}