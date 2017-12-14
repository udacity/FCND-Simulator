using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof (BoxCollider))]
[RequireComponent (typeof (LightProbeGroup))]
public class LightProbeGrid : MonoBehaviour
{
	public int xCount;
	public int yCount;
	public int zCount;

	int lastXCount;
	int lastYCount;
	int lastZCount;

	Vector3 lastCenter;
	Vector3 lastSize;

	BoxCollider box;
	LightProbeGroup probes;

	void OnEnable ()
	{
		box = GetComponent<BoxCollider> ();
		box.isTrigger = true;
		probes = GetComponent<LightProbeGroup> ();
	}

	void Update ()
	{
		if ( xCount != lastXCount || yCount != lastYCount || zCount != lastZCount )
		{
			lastXCount = xCount;
			lastYCount = yCount;
			lastZCount = zCount;
			BuildProbes ();
		}

		if ( box.center != lastCenter || box.size != lastSize )
		{
			lastCenter = box.center;
			lastSize = box.size;
			BuildProbes ();
		}
	}

	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere ( transform.position + transform.rotation * ( lastCenter - lastSize / 2 ), 0.05f );
		Gizmos.color = Color.red;
		Gizmos.DrawSphere ( transform.position + transform.rotation * ( lastCenter + lastSize / 2 ), 0.05f );
	}
	
	void BuildProbes ()
	{
		Vector3 start = transform.rotation * ( lastCenter - lastSize / 2 );
		Vector3 end = transform.rotation * ( lastCenter + lastSize / 2 );
		Vector3 pos = start;
		Vector3 spacing = new Vector3 (
			                  ( end.x - start.x ) / ( xCount - 1 ),
			                  ( end.y - start.y ) / ( yCount - 1 ),
			                  ( end.z - start.z ) / ( zCount - 1 )
		                  );
		Vector3[] positions = new Vector3[xCount * yCount * zCount];
		int i = 0;
		for ( int x = 0; x < xCount; x++ )
		{
			for ( int y = 0; y < yCount; y++ )
			{
				for ( int z = 0; z < zCount; z++ )
				{
					positions [ i ] = start + new Vector3 ( x * spacing.x, y * spacing.y, z * spacing.z );
					i++;
				}
			}
		}
		probes.probePositions = positions;
	}
}