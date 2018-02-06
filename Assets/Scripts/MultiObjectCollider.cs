using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MultiObjectCollider : MonoBehaviour
{
	BoxCollider bc;

	void OnTransformChildrenChanged ()
	{
		BuildCollider ();
	}

	void OnEnable ()
	{
		BuildCollider ();
	}

	void BuildCollider ()
	{
		bc = GetComponent<BoxCollider> ();
		if ( bc == null )
			bc = gameObject.AddComponent<BoxCollider> ();

		var bounds = new Bounds ();
		var rends = GetComponentsInChildren<Renderer> ();
		if ( rends == null || rends.Length == 0 )
		{
			bc.center = transform.position;
			bc.size = Vector3.one;
			return;
		}

		foreach ( var r in rends )
			bounds.center += r.bounds.center;
		bounds.center /= rends.Length;
		foreach ( var r in rends )
			bounds.Encapsulate ( r.bounds );

		bc.center = bounds.center - transform.position;
		bc.size = bounds.size;
	}
}