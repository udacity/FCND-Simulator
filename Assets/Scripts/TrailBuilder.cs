using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TrailBuilder : MonoBehaviour 
{
	public LineRenderer line;
	public int subSegments = 1;

	Transform tr;
	Vector3[] positions;
	float nextUpdateTime;

	void OnEnable ()
	{
		tr = transform;
	}

	void Awake ()
	{
		UpdateLine ();
	}

	void Update ()
	{
		if ( Time.time > nextUpdateTime )
		{
			nextUpdateTime = Time.time + 0.2f;
			UpdateLine ();
		}
	}

	void UpdateLine ()
	{
		int childCount = tr.childCount;
		if ( childCount > 0 )
		{
			if ( subSegments < 1 )
				subSegments = 1;
			positions = new Vector3[childCount * subSegments];
			for ( int i = 0; i < childCount; i++ )
			{
				int index = subSegments * i;
				Transform t = tr.GetChild ( i );
				positions [ index ] = t.position;
				if ( i < childCount - 1 && subSegments > 1 )
				{
					Transform t2 = tr.GetChild ( i + 1 );
					for ( int j = 1; j < subSegments; j++ )
					{
						positions [ index + j ] = Vector3.Lerp ( t.position, t2.position, 1f * j / subSegments );
					}
				}
			}
			line.SetPositions ( positions );
		}
		line.positionCount = positions.Length;
	}
}