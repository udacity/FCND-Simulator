using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;

public class LocationSelectUI : MonoBehaviour
{
	public AbstractMap mapScript;

	void OnEnable ()
	{
		if ( mapScript == null )
		{
			GameObject mapObject = GameObject.Find ( "Map" );
			if ( mapObject != null )
				mapScript = mapObject.GetComponent <AbstractMap> ();
		}
	}

	public void LocationSelected (UILocationOption info)
	{
		if ( mapScript != null )
		{
			// set the drone position
//			DroneControllers.SimpleQuadController droneCtrl = 
			var location = Conversions.StringToLatLon ( info.latLongCoord );
			mapScript.Initialize ( location, mapScript.AbsoluteZoom );
			gameObject.SetActive ( false );
		}
	}
}