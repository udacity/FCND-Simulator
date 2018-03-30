using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKsolver : MonoBehaviour {

	public Transform pivot;
	public Transform target;
	Vector3 up;
	// Use this for initialization
	void Start () {
		up = transform.up;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = pivot.position;
		transform.LookAt (target, up);
	}
}
