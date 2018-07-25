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
				SceneView.lastActiveSceneView.LookAt ( builder.SplineToWorldPosition ( spline.controlPoints [ i ].position ) );
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

		EditorGUI.BeginChangeCheck ();
		bool showTest = EditorGUILayout.ToggleLeft ( "Show Test Position", builder.showTestPosition );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Show Test" );
			builder.showTestPosition = showTest;
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

		if ( builder.showTestPosition )
		{
			ShowTestPosition ();
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
		Vector3 p = builder.SplineToWorldPosition ( point.position );
		float size = HandleUtility.GetHandleSize ( p ) * 0.2f;

		// only draw movement controls if point is selected
		if ( selected )
		{

			Handles.color = Color.yellow;
			if ( point.hasLeftHandle )
				Handles.DrawLine ( p, builder.SplineToWorldPosition ( point.leftHandle ) );
			if ( point.hasRightHandle )
				Handles.DrawLine ( p, builder.SplineToWorldPosition ( point.rightHandle ) );

			// draw main node
			Vector3 snap = Vector3.one * 0.5f;
			EditorGUI.BeginChangeCheck ();
			Vector3 pos = Handles.FreeMoveHandle ( p, Quaternion.identity, size * 2, snap, Handles.SphereHandleCap );
			if ( EditorGUI.EndChangeCheck () )
			{
				p = pos;
				pos = builder.WorldToSplinePosition ( pos );
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
				pos = Handles.FreeMoveHandle ( builder.SplineToWorldPosition ( point.leftHandle ), Quaternion.identity, size, snap, Handles.CubeHandleCap );
				if ( EditorGUI.EndChangeCheck () )
				{
					Undo.RecordObject ( builder, "Move Left Handle" );
					point.leftHandle = builder.WorldToSplinePosition ( pos );
					EditorUtility.SetDirty ( builder );
				}
			}

			// draw the right handle if we have one
			if ( point.hasRightHandle )
			{
				Handles.color = Color.yellow;
				EditorGUI.BeginChangeCheck ();
				pos = Handles.FreeMoveHandle ( builder.SplineToWorldPosition ( point.rightHandle ), Quaternion.identity, size, snap, Handles.CubeHandleCap );
				if ( EditorGUI.EndChangeCheck () )
				{
					Undo.RecordObject ( builder, "Move Right Handle" );
					point.rightHandle = builder.WorldToSplinePosition ( pos );
					EditorUtility.SetDirty ( builder );
				}
			}

		} else
		{
			Handles.color = Color.white;
			if ( Handles.Button ( p, Quaternion.identity, size, size, Handles.SphereHandleCap ) )
			{
				Repaint ();
				return true;
			}
		}

		return selected;
	}

	void DrawCurve (ControlPoint p1, ControlPoint p2)
	{
		Handles.DrawBezier ( builder.SplineToWorldPosition ( p1.position ), builder.SplineToWorldPosition ( p2.position ), builder.SplineToWorldPosition ( p1.rightHandle ), builder.SplineToWorldPosition ( p2.leftHandle ), Color.white, null, 6 );
		// temp
		Handles.color = new Color ( 1f, 0, 0, 0.5f );
		for (int i = 0; i < 10; i++)
		{
			float t = 1f * i / 9;
			Vector3 position = builder.SplineToWorldPosition ( spline.Sample ( p1, p2, t ) );
			Vector3 direction = spline.GetDirection ( builder.SplineToWorldPosition ( spline.controlPoints [ 0 ].position ), builder.SplineToWorldPosition ( spline.GetFirstDerivative ( p1, p2, t ) ) );
//			Vector3 direction = spline.GetDirection ( builder.SplineToWorldPosition ( p1.position ), builder.SplineToWorldPosition ( spline.GetFirstDerivative ( p1, p2, t ) ) );
			float size = HandleUtility.GetHandleSize ( position ) * 0.1f;
//			Handles.CircleHandleCap ( -1, position, Quaternion.LookRotation ( direction ), HandleUtility.GetHandleSize ( position ) * 0.1f, EventType.Repaint );
			if ( Handles.Button ( position, Quaternion.LookRotation ( direction ), size, size, Handles.CylinderHandleCap ) )
			{
				SceneView.lastActiveSceneView.LookAt ( position );
			}
			Handles.CylinderHandleCap ( -1, position, Quaternion.LookRotation ( direction ), HandleUtility.GetHandleSize ( position ) * 0.1f, EventType.Repaint );
		}
	}

	void ShowTestPosition ()
	{
		Vector3 pos = builder.SplineToWorldPosition ( builder.testPosition );
		float size = HandleUtility.GetHandleSize ( pos ) * 0.2f;
		Handles.color = Color.green;
		EditorGUI.BeginChangeCheck ();
		Vector3 newTestPos = Handles.FreeMoveHandle ( pos, Quaternion.identity, size, Vector3.one * 0.5f, Handles.SphereHandleCap );
		if ( EditorGUI.EndChangeCheck () )
		{
			Undo.RecordObject ( builder, "Move Test Pos" );
			builder.testPosition = builder.WorldToSplinePosition ( newTestPos );
			EditorUtility.SetDirty ( builder );
		}

		Vector3 closest = builder.SplineToWorldPosition ( spline.GetClosestPoint ( builder.testPosition ) );
		Handles.color = Color.black;
		Handles.SphereHandleCap ( -1, closest, Quaternion.identity, size, EventType.Repaint );
	}
}