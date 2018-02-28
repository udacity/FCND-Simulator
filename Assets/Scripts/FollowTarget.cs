using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
	public Transform followTarget;

	Transform tr;

	void Awake ()
	{
		tr = transform;
	}

	void LateUpdate ()
	{
		tr.position = followTarget.position;
	}
}