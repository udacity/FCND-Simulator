#define USE_FASTNOISE
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

	public LayerMask ignoreLayers;

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

	FastNoise fn = new FastNoise ();

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
		#if USE_FASTNOISE
		directionNoise = new Vector3 ( Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f ) * 25;
//		directionNoise = Random.insideUnitSphere * 2f;
		#else
		directionNoise = Random.insideUnitSphere * 2f;
		#endif
	}

	void FixedUpdate ()
	{

		if ( useDirectionNoise )
			windEuler = new Vector3 (
				GetNoise ( Time.time * directionNoise.x, Time.time * directionNoise.x ) * dirNoiseStrength,
				windEuler.y + GetNoise ( Time.time * directionNoise.y, Time.time * directionNoise.y ) * dirNoiseStrength,
				GetNoise ( Time.time * directionNoise.z, Time.time * directionNoise.z ) * dirNoiseStrength
			);
//			windEuler = new Vector3 (
//				Mathf.Lerp ( -dirNoiseStrength, dirNoiseStrength, GetNoise01 ( Time.time * directionNoise.x, Time.time * directionNoise.x ) ),
//				windEuler.y + ( 0.25f - GetNoise01 ( Time.time * directionNoise.y, Time.time * directionNoise.y ) * 0.5f ) * dirNoiseStrength,
//				Mathf.Lerp ( -dirNoiseStrength, dirNoiseStrength, GetNoise01 ( Time.time * directionNoise.z, Time.time * directionNoise.z ) )
//			);

		lastNoise = GetNoise01 ( 0, Time.time * 21.2485f );
		Vector3 curDirection = Quaternion.Euler ( windEuler ) * Vector3.forward;

		lastWind = curDirection * Mathf.Lerp ( minForce, maxForce, lastNoise );
		Vector3 frameWind = lastWind * Time.deltaTime;

		for ( int i = 0; i < affectedObjects.Count; i++ )
			affectedObjects [ i ].transform.position += frameWind;
	}

	void OnGUI ()
	{
		GUILayout.BeginArea ( new Rect ( 10, 10, 200, 50 ) );
		GUILayout.Box ( "Last wind: " + lastWind.ToString () + "\nnoise: " + lastNoise.ToString ( "F2" ) );
		GUILayout.EndArea ();
	}

	void OnTriggerEnter (Collider other)
	{
		if ( ignoreLayers.ContainsLayer ( other.gameObject.layer ) )
			return;
		Rigidbody rb = other.transform.root.GetComponentInChildren<Rigidbody> ();
		if ( rb != null )
			affectedObjects.Add ( rb.transform );
	}

	void OnTriggerExit (Collider other)
	{
		Rigidbody rb = other.transform.root.GetComponentInChildren<Rigidbody> ();
		if ( rb != null )
			affectedObjects.Remove ( rb.transform );
	}

	float GetNoise (float x, float y)
	{
		#if USE_FASTNOISE
		return fn.GetSimplex ( x, y );
		#else
		return Mathf.PerlinNoise ( x, y );
		#endif
	}

	float GetNoise01 (float x, float y)
	{
		#if USE_FASTNOISE
		return 0.5f * fn.GetSimplex ( x, y ) + 0.5f;
		#else
		return Mathf.PerlinNoise ( x, y );
		#endif
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