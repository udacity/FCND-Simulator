using System;
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

	[NonSerialized]
	public List<ColliderVolume> colliders;

	float nextTestViz;

	void Start ()
	{
		
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
		StartCoroutine ( DoGenerate ( onComplete ) );
	}

	IEnumerator DoGenerate (Action onComplete = null)
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

		if ( onComplete != null )
			onComplete ();
	}
}