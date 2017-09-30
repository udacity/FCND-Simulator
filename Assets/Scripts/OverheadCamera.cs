using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheadCamera : MonoBehaviour
{
	public static OverheadCamera instance;

	[System.NonSerialized]
	public Camera camera;
	public float minZoom = 5;
	public float maxZoom = 35;

	int defaultZoomLevel = 2;
	float zoomAmount;

	void Awake ()
	{
		instance = this;
		camera = GetComponent<Camera> ();
		zoomAmount = ( maxZoom - minZoom ) / 6;
		camera.orthographicSize = minZoom + zoomAmount * defaultZoomLevel;
	}

	void LateUpdate ()
	{
		if ( Input.GetButtonDown ("Minimap Zoom") )
		{
			float zoomRaw = Input.GetAxisRaw ( "Minimap Zoom" );
			if ( zoomRaw > 0 )
				ZoomIn ();
			else
			if ( zoomRaw < 0 )
				ZoomOut ();
		}
		if ( Input.GetButtonDown ( "Minimap Reset" ) )
		{
			ResetZoom ();
		}
	}

	public void ZoomIn ()
	{
		camera.orthographicSize = Mathf.Max ( camera.orthographicSize - zoomAmount, minZoom );
	}

	public void ZoomOut ()
	{
		camera.orthographicSize = Mathf.Min ( camera.orthographicSize + zoomAmount, maxZoom );
	}

	public void ResetZoom ()
	{
		camera.orthographicSize = minZoom + zoomAmount * defaultZoomLevel;
	}
}