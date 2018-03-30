using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkVehicleTag : MonoBehaviour {

	public Transform mainCamera;
	public Text tagText;
	// Use this for initialization
	void Awake () {
		tagText = GetComponentInChildren<Text> ();
		mainCamera = VSF_Unet_DemoMain.main.mainCamera.transform;
	}

	public void SetTagText (string value){

		if(tagText != null)
		tagText.text = value;

	}
	
	// Update is called once per frame
	void LateUpdate () {

		if (GetComponentInParent<NetworkVehicle>().hasAuthority && !GetComponentInParent<NetworkVehicle>().isAi)
			gameObject.SetActive (false);

		if (mainCamera == null || !mainCamera.gameObject.activeSelf) {
			if(Camera.main != null)
			mainCamera = Camera.main.transform;
		}

		if (mainCamera != null) {

			Vector3 dirToCam = transform.position - mainCamera.transform.position;
			Quaternion lookRotation = Quaternion.LookRotation (dirToCam, Vector3.up);
			transform.rotation = lookRotation;
			transform.position = transform.parent.position + (Vector3.up * 5f);
		}
	}
}
