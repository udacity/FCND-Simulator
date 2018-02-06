using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof (System.Tuple<string, float>))]
public class TupleDrawer : PropertyDrawer
{
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight (property, label);
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty ( position, label, property );

		SerializedProperty stringProp = property.FindPropertyRelative ( "Item1" );
		SerializedProperty floatProp = property.FindPropertyRelative ( "Item2" );

		Rect r1 = new Rect ( position.x, position.y, position.width * 0.7f, position.height );
		Rect r2 = new Rect ( r1.x, r1.y, position.width * 0.3f, r1.height );

		EditorGUI.PropertyField ( r1, stringProp, new GUIContent ( "Type" ) );
		EditorGUI.PropertyField ( r2, floatProp, new GUIContent ( "Value" ) );

		EditorGUI.EndProperty ();
	}
}
