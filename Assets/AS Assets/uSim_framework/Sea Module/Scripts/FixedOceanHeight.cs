using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedOceanHeight : MonoBehaviour {

	public float oceanHeight;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 pos = transform.position;
		pos.y = oceanHeight;
		transform.position = pos;

	}
}
