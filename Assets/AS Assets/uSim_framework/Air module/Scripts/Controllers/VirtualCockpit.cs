using UnityEngine;
using System.Collections;

public class VirtualCockpit : MonoBehaviour {

	public bool show;
	public Camera vcCamera;
	public Transform cameraObj;
	public Transform aircraft;
	// Use this for initialization
	void Start () {
		
	//	show = true;
		transform.parent = null;
		transform.position = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
	
		if(aircraft != null) transform.rotation = aircraft.rotation;

		if (cameraObj != null) {

			vcCamera.transform.localRotation = cameraObj.localRotation;

		}
		vcCamera.enabled = show;
	}
}
