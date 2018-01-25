using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Interfaces;

using Ttext = TMPro.TextMeshPro;

public class UIRoadName : MonoBehaviour, IFeaturePropertySettable
{
	public Ttext text;
	public float fadeDistance = 50;
	public int textSize = 18;
	string label;
	public Color bgColor;
	public Sprite bgSprite;

	Camera cam;
	Transform camTransform;
	Transform tr;
	Vector3 screenPos;
	float fadeStart;
	float fadeEnd;
	float curSqDist;
	float alpha;

	void Awake ()
	{
		cam = Camera.main;
		camTransform = cam.transform;
		tr = transform;
		transform.LookAt ( -Vector3.up );
//		fadeEnd = fadeDistance * fadeDistance * 1.5f;
//		fadeDistance *= fadeDistance;
	}

/*	void Update ()
	{
		curSqDist = ( camTransform.position - tr.position ).sqrMagnitude;
		if ( curSqDist > fadeEnd )
			alpha = 0;
		else
		if ( curSqDist < fadeDistance )
			alpha = 1;
		else
			alpha = Mathf.InverseLerp ( fadeEnd, fadeDistance, curSqDist );

		screenPos = cam.WorldToScreenPoint ( transform.position + Vector3.up * 5 );
		screenPos.y = Screen.height - screenPos.y;
		bgColor.a = alpha;
//		transform.rotation = cam.transform.rotation;
	}*/

/*	void OnGUI ()
	{
		// don't draw streets behind or we're just too far
		if ( screenPos.z < 0 || curSqDist > fadeEnd )
			return;
		
		Vector2 screen = new Vector2 ( Screen.width, Screen.height );
		float ratio = screen.y / 1080;
		GUIStyle style = GUI.skin.label;
		int fontSize = style.fontSize;
		TextAnchor anchor = style.alignment;
		TextClipping clip = style.clipping;
		bool wrap = style.wordWrap;

		style.fontSize = (int) ( textSize * ratio );
		style.alignment = TextAnchor.MiddleCenter;
		style.clipping = TextClipping.Overflow;
		style.wordWrap = false;

//		Vector3 screenPos = cam.WorldToScreenPoint ( transform.position );
//		screenPos.y = screen.y - screenPos.y;

		Vector2 size = style.CalcSize ( new GUIContent ( label ) ) + new Vector2 ( 20, 10 ) * ratio;
//		GUI.color = bgColor;
		Rect bgRect = new Rect ( screenPos.x - size.x / 2, screenPos.y - size.y / 2, size.x, size.y );
//		GUI.DrawTexture ( bgRect, bgSprite.texture );
//		float aspect = 0;
		float aspect = bgRect.width / bgRect.height;
		GUI.color = bgColor;
		GUI.DrawTexture ( bgRect, bgSprite.texture, ScaleMode.ScaleToFit, true, aspect );
		GUI.DrawTexture ( bgRect, bgSprite.texture, ScaleMode.ScaleToFit, true, aspect, new Color ( 1, 1, 1, alpha ), 2, 4 );
//		GUI.Box ( bgRect, "" );
//		GUI.color = Color.white;

		// draw white shadow first
		GUI.color = new Color ( 1, 1, 1, alpha * 0.5f );
		GUI.Label ( new Rect ( screenPos.x + 1, screenPos.y + 1, 1, 1 ), label, style );
		// then the text
		GUI.color = new Color ( 0, 0, 0, alpha );
		GUI.Label ( new Rect ( screenPos.x, screenPos.y, 1, 1 ), label, style );

		style.fontSize = fontSize;
		style.alignment = anchor;
		style.clipping = clip;
		style.wordWrap = wrap;
	}*/

	public void Set (Dictionary<string, object> props)
	{
//		label = GetFeatureName ( props );
//		name = label + " - " + name;
//		Debug.Log ( name + " " + props.DictionaryToString () );
		text.text = GetFeatureName ( props );
		name = text.text + " - " + name;
		transform.position += Vector3.up * 0.05f;
	}

	string GetFeatureName (Dictionary<string, object> props)
	{
		if ( props.ContainsKey ( "name" ) )
		{
			return props [ "name" ].ToString ();
		} else
			if ( props.ContainsKey ( "house_num" ) )
			{
				return props [ "house_num" ].ToString ();
			} else
				if ( props.ContainsKey ( "type" ) )
				{
					return props [ "type" ].ToString ();
				}

		return "?";
	}
}