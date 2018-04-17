using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Map;
using UdacityNetworking;
using ttext = TMPro.TextMeshProUGUI;

public class RaycastGenerateColliders : MonoBehaviour
{
	public AbstractMap mapScript;
	public Vector3 startingPoint;
	public float range = 100;
	public LayerMask collisionMask;
	public Transform testCube;
	public float density = 50;
	public float stepDistance = 1;
	public float boxSize = 2;
	public bool useNewVersion = true;

	public Canvas canvas;
	public ttext messageText;
	public Button sendButton;
	public Button cancelButton;

	[NonSerialized]
	public List<ColliderVolume> colliders;

	float nextTestViz;

	void Awake ()
	{
		mapScript.OnInitialized += OnMapInitialized;
		canvas.enabled = false;
	}

	void Start ()
	{
		mapScript.MapVisualizer.OnMapVisualizerStateChanged += MapScript_MapVisualizer_OnMapVisualizerStateChanged;
	}

	void OnDestroy ()
	{
		mapScript.OnInitialized -= OnMapInitialized;
		mapScript.MapVisualizer.OnMapVisualizerStateChanged -= MapScript_MapVisualizer_OnMapVisualizerStateChanged;
//		Debug.Log ( "why am i being destroyed?" );
	}

	void OnMapInitialized ()
	{
		transform.position = mapScript.transform.position;
	}

	void MapScript_MapVisualizer_OnMapVisualizerStateChanged (ModuleState obj)
	{
		if ( obj == ModuleState.Finished )
			GenerateColliders ();
	}
	
	void Update ()
	{
//		if ( Input.GetButton ( "Shift Modifier" ) && Input.GetButtonDown ( "Save" ) )
//		{
//			GenerateColliders ();
//			CollidersToCSV ( true );
//		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos ()
	{
		if ( UnityEditor.Selection.activeTransform == null || ( UnityEditor.Selection.activeTransform != transform && UnityEditor.Selection.activeTransform != testCube ) )
			return;
		if ( colliders == null || colliders.Count == 0 )
			return;

		Gizmos.color = new Color ( 1, 0.5f, 0 );
		Gizmos.DrawWireCube ( colliders [ 0 ].position, colliders [ 0 ].size );

		Gizmos.color = Color.yellow;
		for ( int i = colliders.Count - 1; i >= 1; i-- )
			Gizmos.DrawWireCube ( colliders [ i ].position, colliders [ i ].size );

//		if (Time.time > nextTestViz)
//		{
//			testVolumes = GetNearbyColliders(testCube.position, testRange);
//			nextTestViz = Time.time + 1f;
//		}
//		if (testVolumes != null && testVolumes.Length > 0)
//		{
//			Gizmos.color = Color.blue;
//			for (int i = testVolumes.Length - 1; i >= 0; i--)
//				Gizmos.DrawWireCube(testVolumes[i].position, testVolumes[i].size);
//		}

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube ( testCube.position, Vector3.one * range * 2 );
	}
	#endif

	public void GenerateColliders (Action onComplete = null)
	{
		messageText.text = "Generating colliders, please wait...";
		sendButton.gameObject.SetActive ( false );
		cancelButton.gameObject.SetActive ( false );
		canvas.enabled = true;

		if ( useNewVersion )
			StartCoroutine ( DoGenerate ( onComplete ) );
		else
			StartCoroutine ( DoGenerateOld ( onComplete ) );
	}

	void ShowCompleteUI ()
	{
		messageText.text = "Finished generating colliders!";
		sendButton.gameObject.SetActive ( true );
		cancelButton.gameObject.SetActive ( true );
	}

	public void OnSendButtonClicked ()
	{
		canvas.enabled = false;
		GameObject networkGO = GameObject.FindWithTag ( "NetworkController" );
		NetworkController controller = networkGO.GetComponent<NetworkController> ();
		byte[] message = ZipString ( GetCollidersString ().ToString () );

		controller.SendMessage ( MessageType.Colliders, message );
	}

	public void OnCancelButtonClicked ()
	{
		canvas.enabled = false;
		Debug.Log ( "Cancelled sending colliders" );
	}

	IEnumerator DoGenerate (Action onComplete = null)
	{
		Debug.Log ( "generating colliders..." );
		yield return null;

		float lineCount = range * 2 / stepDistance;
		float halfSize = boxSize / 2;
//		float distance = range * 2 / density2;
//		float halfDistance = distance / 2;
		Vector3 startPos = transform.position - Vector3.one * ( range - stepDistance );
//		Vector3 startPos = transform.position - Vector3.one * ( range - halfDistance );

		startPos.y = 500;
		Ray ray = new Ray ( startPos, -Vector3.up );
		RaycastHit hit;

		colliders = new List<ColliderVolume> ();
		int count = 0;

		for ( int x = 0; x < lineCount; x++ )
		{
			for ( int z = 0; z < lineCount; z++ )
			{
				ray.origin = startPos + new Vector3 ( x * stepDistance, 0, z * stepDistance );
				if ( Physics.Raycast ( ray, out hit, 1000, collisionMask, QueryTriggerInteraction.Ignore ) )
				{
					if ( hit.point.y > 0 )
					{
						float halfHeight = hit.point.y / 2;
						Vector3 center = hit.point;
						center.y = halfHeight;
						colliders.Add ( new ColliderVolume ( center, new Vector3 ( halfSize, hit.point.y, halfSize ) ) );
					}
				}

				// limit to 1000 raycasts per frame
				count++;
				if ( count > 5000 )
				{
					count -= 5000;
					yield return null;
				}
			}
		}

		GameObject[] props = GameObject.FindGameObjectsWithTag ( "Prop" );
		props.ForEach ( ( x ) =>
		{
			Collider c = x.GetComponent<Collider> ();
			if ( c != null )
				colliders.Add ( ColliderVolume.FromCollider ( c ) );
		} );

		Debug.Log ( colliders.Count + " generated from raycasts and props." );
		ShowCompleteUI ();

		if ( onComplete != null )
			onComplete ();
	}

	IEnumerator DoGenerateOld (Action onComplete = null)
	{
		Debug.Log ( "generating colliders..." );
		yield return null;
		//		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch ();
		float distance = range * 2 / density;
		float halfDistance = distance / 2;
		Vector3 startPos = transform.position - Vector3.one * ( range - halfDistance );
		//		Vector3 endPos = transform.position + Vector3.one * ( range - distance / 2 );

		startPos.y = 500;
		Ray ray = new Ray ( startPos, -Vector3.up );
		RaycastHit hit;

		colliders = new List<ColliderVolume> ();
		int count = 0;

		for ( int x = 0; x < density; x++ )
		{
			for ( int z = 0; z < density; z++ )
			{
				ray.origin = startPos + new Vector3 ( x * distance, 0, z * distance );
				if ( Physics.Raycast ( ray, out hit, 1000, collisionMask, QueryTriggerInteraction.Ignore ) )
				{
					if ( hit.point.y > 0 )
					{
						float halfHeight = hit.point.y / 2;
						Vector3 center = hit.point;
						center.y = halfHeight;
						colliders.Add ( new ColliderVolume ( center, new Vector3 ( distance, hit.point.y, distance ) ) );
					}
				}

				// limit to 1000 raycasts per frame
				count++;
				if ( count > 1000 )
				{
					count -= 1000;
					yield return null;
				}
			}
		}

		GameObject[] props = GameObject.FindGameObjectsWithTag ( "Prop" );
		props.ForEach ( ( x ) =>
		{
			Collider c = x.GetComponent<Collider> ();
			if ( c != null )
				colliders.Add ( ColliderVolume.FromCollider ( c ) );
		} );

		Debug.Log ( colliders.Count + " generated from raycasts and props." );
		ShowCompleteUI ();

		if ( onComplete != null )
			onComplete ();
	}

	System.Text.StringBuilder GetCollidersString ()
	{
		var header = "posX,posY,posZ,halfSizeX,halfSizeY,halfSizeZ\n";
		string latString = Simulation.latitude0.ToString ();
		string lonString = Simulation.longitude0.ToString ();
		System.Text.StringBuilder sb = new System.Text.StringBuilder ();

		int length = latString.Split ( '.' ) [ 1 ].Length;
		for ( int i = 0; i < ( 6 - length ); i++ )
			latString += "0";

		length = lonString.Split ( '.' ) [ 1 ].Length;
		for ( int i = 0; i < ( 6 - length ); i++ )
			lonString += "0";
		sb.Append ( "lat0 " + latString + ", lon0 " + lonString + "\n" );
		sb.Append ( header );

		foreach (var c in colliders)
		{
			var pos = c.position;
			var hsize = c.halfSize;
			var row = string.Format("{0},{1},{2},{3},{4},{5}\n", pos.z, pos.x, pos.y, hsize.z, hsize.x, hsize.y);
			sb.Append ( row );
		}

		return sb;
	}

	byte[] ZipString (string s)
	{
		byte[] bytes = s.GetBytes ();
		Debug.Log ( "there are " + bytes.Length + " bytes before zipping!" );

		using ( System.IO.MemoryStream ms = new System.IO.MemoryStream () )
		using ( GZipStream gzip = new GZipStream ( ms, CompressionMode.Compress ) )
		{
			gzip.Write ( bytes, 0, bytes.Length );
			gzip.Close ();
			bytes = ms.ToArray ();
		}

		Debug.Log ( "there are " + bytes.Length + " bytes after zipping!" );

		return bytes;
	}
}