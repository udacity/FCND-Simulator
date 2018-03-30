using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockHorizontalPosition : MonoBehaviour {


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.localPosition;
		pos.x = pos.z = 0f;
		transform.localPosition = pos;

	}
}
