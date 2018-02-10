using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastGenerateColliders : MonoBehaviour
{
	public Vector3 startingPoint;
	public float range = 100;
	public LayerMask collisionMask;
	public Transform testCube;
	public float density = 50;

	List<ColliderVolume> colliders;

	float nextTestViz;

	void Start ()
	{
		
	}
	
	void Update ()
	{
		if ( Input.GetButton ( "Shift Modifier" ) && Input.GetButtonDown ( "Save" ) )
		{
			GenerateColliders ();
//			CollidersToCSV ( true );
		}
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

	void GenerateColliders ()
	{
		StartCoroutine ( DoGenerate () );
	}

	IEnumerator DoGenerate ()
	{
		Debug.Log ( "generating colliders..." );
		yield return null;
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch ();
		float distance = range * 2 / density;
		float halfDistance = distance / 2;
		Vector3 startPos = transform.position - Vector3.one * ( range - halfDistance );
//		Vector3 endPos = transform.position + Vector3.one * ( range - distance / 2 );

		startPos.y = 500;
		Ray ray = new Ray ( startPos, -Vector3.up );
		RaycastHit hit;

		colliders = new List<ColliderVolume> ();

		for ( int x = 0; x < density; x++ )
		{
			for ( int z = 0; z < density; z++ )
			{
				ray.origin = startPos + new Vector3 ( x * distance, 0, z * distance );
				if ( Physics.Raycast ( ray, out hit, 1000, collisionMask ) )
				{
					if ( hit.point.y > 0 )
					{
						float halfHeight = hit.point.y / 2;
						Vector3 center = hit.point;
						center.y = halfHeight;
						colliders.Add ( new ColliderVolume ( center, new Vector3 ( distance, hit.point.y, distance ) ) );
					}
				}
			}
		}
		Debug.Log ( colliders.Count + " generated" );
	}
}