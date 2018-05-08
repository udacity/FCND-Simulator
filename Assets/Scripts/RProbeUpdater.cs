using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;

public class RProbeUpdater : MonoBehaviour
{
	public float updateTime = 2;
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
		nextUpdate = Time.time + updateTime;
	}

	void Update ()
	{
		if ( Time.time > nextUpdate )
		{
			probe.RenderProbe ();
			nextUpdate = Time.time + updateTime;
		}
	}

	void OnMapInitialized ()
	{
		transform.position = mapTransform.position + Vector3.up * 99;
	}
}