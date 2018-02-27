using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;

public class RProbeUpdater : MonoBehaviour
{
	ReflectionProbe probe;
	Transform mapTransform;
	float nextUpdate;

	void Awake ()
	{
		GameObject mapObject = GameObject.Find ( "Map" );
		if ( mapObject != null )
		{
			mapTransform = mapObject.transform;
			mapObject.GetComponent<AbstractMap> ().OnInitialized += OnMapInitialized;
		}
		probe = GetComponent<ReflectionProbe> ();
		nextUpdate = Time.time + 2;
	}

	void Update ()
	{
		if ( Time.time > nextUpdate )
		{
			probe.RenderProbe ();
			nextUpdate = Time.time + 2;
		}
	}

	void OnMapInitialized ()
	{
		transform.position = mapTransform.position + Vector3.up * 99;
	}
}