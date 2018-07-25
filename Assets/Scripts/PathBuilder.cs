using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathBuilder : MonoBehaviour
{
	[HideInInspector]
	public List<PathSegment> segments = new List<PathSegment> ();
	[HideInInspector]
	public Transform tr;

	public Material lineMaterial;

	void OnEnable ()
	{
		tr = transform;
	}

	public void AddSegment (PathSegmentType type)
	{
		PathSegment seg = new PathSegment ();
		seg.type = type;
		seg.start = new ControlPoint ();
		seg.middle = new ControlPoint ( Vector3.right );
		seg.end = new ControlPoint ();
		seg.axis = Quaternion.LookRotation ( Vector3.forward, Vector3.up );
		seg.radius = 1;
		seg.angle = 90;
		segments.Add ( seg );
	}

	public Vector3 LocalToWorld (Vector3 position)
	{
		return tr.TransformPoint ( position );
	}

	public Vector3 WorldToLocal (Vector3 position)
	{
		return tr.InverseTransformPoint ( position );
	}

	public PathSegment[] TestPosition (Vector3 position, float radius = 0)
	{
		if ( segments == null || segments.Count == 0 )
			return null;

		Vector3 localPosition = WorldToLocal ( position );
		List<PathSegment> list = new List<PathSegment> ();
		for ( int i = 0; i < segments.Count; i++ )
		{
			if ( segments [ i ].TestPosition ( localPosition, radius ) )
				list.Add ( segments [ i ] );
		}
		return list.ToArray ();
	}
}