using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindDisturbance : MonoBehaviour
{
	static WindDisturbance instance;
	static WindDisturbance Instance
	{
		get
		{
			if ( instance == null )
			{
				instance = new GameObject ( "WindDisturbance" ).AddComponent<WindDisturbance> ();
			}
			return instance;
		}
	}

	public WindZone windZone;
	public bool useWindZone;

	public Vector3 windDirection = Vector3.forward;
	public bool useDirectionNoise = true;
	[Range(0, 3)]
	public float dirNoiseStrength = 0.2f;
	public float minForce = 0.1f;
	public float maxForce = 5f;

	public List<Transform> affectedObjects = new List<Transform> ();

	Vector3 directionNoise;
	Vector3 windEuler;
	Vector3 lastWind;
	float lastNoise;

	void Awake ()
	{
		if ( instance != null && instance != this )
		{
			Destroy ( gameObject );
			return;
		}

		if ( minForce > maxForce )
		{
			float temp = minForce;
			minForce = maxForce;
			maxForce = minForce;
		}
		instance = this;
		windEuler = Quaternion.LookRotation ( windDirection.normalized ).eulerAngles;
		directionNoise = Random.insideUnitSphere * 2f;
	}

	void Start ()
	{
		
	}

	void FixedUpdate ()
	{

		if ( useDirectionNoise )
			windEuler = new Vector3 (
//				windEuler.x + ( 0.25f - Mathf.PerlinNoise ( Time.time * directionNoise.x, Time.time * directionNoise.x ) * 0.5f ) * dirNoiseStrength,
				Mathf.Lerp ( -dirNoiseStrength, dirNoiseStrength, Mathf.PerlinNoise ( Time.time * directionNoise.x, Time.time * directionNoise.x ) ),
				windEuler.y + ( 0.25f - Mathf.PerlinNoise ( Time.time * directionNoise.y, Time.time * directionNoise.y ) * 0.5f ) * dirNoiseStrength,
				Mathf.Lerp ( -dirNoiseStrength, dirNoiseStrength, Mathf.PerlinNoise ( Time.time * directionNoise.z, Time.time * directionNoise.z ) )
			);

		lastNoise = Mathf.PerlinNoise ( 0, Time.time * 2f );
		Vector3 curDirection = Quaternion.Euler ( windEuler ) * Vector3.forward;

		lastWind = curDirection * Mathf.Lerp ( minForce, maxForce, lastNoise );
		Vector3 frameWind = lastWind * Time.deltaTime;

		for ( int i = 0; i < affectedObjects.Count; i++ )
			affectedObjects [ i ].transform.position += frameWind;

//		if ( useWindZone )
//		{
//			
//		} else
//		{
//			
//		}
	}

	void OnGUI ()
	{
		GUILayout.BeginArea ( new Rect ( 10, 10, 200, 50 ) );
		GUILayout.Box ( "Last wind: " + lastWind.ToString () + "\nnoise: " + lastNoise.ToString ( "F2" ) );
//		GUILayout.Box ( "x: " + lastWind.x + " y: " + lastWind.y + " z: " + lastWind.z + "\nnoise: " + lastNoise );
		GUILayout.EndArea ();
	}

	public static void AddAffectedObject (Transform t)
	{
		if ( !Instance.affectedObjects.Contains ( t ) )
			Instance.affectedObjects.Add ( t );
	}

	public static void RemoveAffectedObject (Transform t)
	{
		Instance.affectedObjects.Remove ( t );
	}
}