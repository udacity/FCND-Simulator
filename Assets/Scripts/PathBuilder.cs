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
}