using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UIUtility : MonoBehaviour
{
	[MenuItem ("Utility/UI/Center Anchor")]
	static void CenterAnchor ()
	{
		GameObject[] objects = Selection.gameObjects;
//		GameObject go = Selection.activeGameObject;

		int width = 1920;
		int height = 1080;

		for ( int i = 0; i < objects.Length; i++ )
		{
			GameObject go = objects [ i ];
			RectTransform rt = go.GetComponent<RectTransform> ();
//			RectTransform parentRT = (RectTransform) rt.parent;
			
			Vector2 pos = rt.anchoredPosition;
			if ( rt.anchorMin != rt.anchorMax )
			{
				Debug.Log ( go.name + " has different anchors" );
				continue;
			}
			
			Vector2 anchorPosition = new Vector2 ( rt.anchorMin.x * width, rt.anchorMin.y * height );
			Vector2 anchor = new Vector2 ( ( anchorPosition.x + pos.x ) / width, ( anchorPosition.y + pos.y ) / height );
			
			Undo.RecordObject ( rt, "Anchor" + i );
			rt.anchorMin = rt.anchorMax = anchor;
			rt.anchoredPosition = Vector2.zero;
		}
	}

	[MenuItem ("Utility/UI/Position -> Anchor")]
	static void PositionToAnchor ()
	{
		GameObject[] objects = Selection.gameObjects;

		int width = 1920;
		int height = 1080;

		for ( int i = 0; i < objects.Length; i++ )
		{
			GameObject go = objects [ i ];
			RectTransform rt = go.GetComponent<RectTransform> ();
//			RectTransform parentRT = (RectTransform) rt.parent;

			Vector2 pos = rt.anchoredPosition;
			Vector2 size = rt.sizeDelta;
//			if ( rt.anchorMin != rt.anchorMax )
//			{
//				Debug.Log ( go.name + " has different anchors" );
//				continue;
//			}

//			Rect positionRect = new Rect ( rt.offsetMin, rt.offsetMax );
			Vector2 posMin = new Vector2 ( rt.anchorMin.x * width, rt.anchorMin.y * height );
			Vector2 anchorMin = posMin + rt.offsetMin;
			Vector2 posMax = new Vector2 ( rt.anchorMax.x * width, rt.anchorMax.y * height );
			Vector2 anchorMax = posMax + rt.offsetMax;

			anchorMin = new Vector2 ( anchorMin.x / width, anchorMin.y / height );
			anchorMax = new Vector2 ( anchorMax.x / width, anchorMax.y / height );

			Undo.RecordObject ( rt, "AnchorPosition" + i );
			rt.anchorMin = anchorMin;
			rt.anchorMax = anchorMax;
			rt.anchoredPosition = Vector2.zero;
			rt.sizeDelta = Vector2.zero;
		}
	}
}