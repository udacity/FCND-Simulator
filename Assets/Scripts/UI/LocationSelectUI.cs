using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;

public class LocationSelectUI : MonoBehaviour
{
	public AbstractMap mapScript;
	public GameObject uiObject;

	void OnEnable ()
	{
		if ( mapScript == null )
		{
			GameObject mapObject = GameObject.Find ( "Map" );
			if ( mapObject != null )
			{
				mapScript = mapObject.GetComponent <AbstractMap> ();
			}
		}
		mapScript.OnInitialized += OnMapInitialized;
		uiObject.SetActive ( false );
	}

	void Awake ()
	{
	}

	public void LocationSelected (UILocationOption info)
	{
		if ( mapScript != null )
		{
			// set the drone position
//			DroneControllers.SimpleQuadController droneCtrl = 
			var location = Conversions.StringToLatLon ( info.latLongCoord );
			mapScript.SetCenterLatitudeLongitude ( location );
			mapScript.Initialize ( location, mapScript.AbsoluteZoom );
			gameObject.SetActive ( false );
		}
	}

	void OnMapInitialized ()
	{
		Debug.Log ( "Map initialized: spawning drone and cam" );
		DroneSpawner.SpawnDrone ();

		Simulation.ActiveDrone.SetHomePosition ( mapScript.CenterLatitudeLongitude.y, mapScript.CenterLatitudeLongitude.x, -1 );
		uiObject.SetActive ( true );
	}
}