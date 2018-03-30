using UnityEngine;
using System.Collections;

public class FlyByCamera : MonoBehaviour {

	public Vector3 offsets;
	public float cameraDistance;
	public Transform playerVehicle;
	public Transform cameraT;
	Vector3 newCamPos;



	void Update () {

		cameraT.LookAt (playerVehicle);

	}


	public void SetupCamera () {

		newCamPos = playerVehicle.position + (playerVehicle.forward * cameraDistance) + playerVehicle.TransformDirection (offsets);
		newCamPos.y = playerVehicle.position.y + offsets.y;
		cameraT.position = newCamPos;
		cameraT.parent = null;
		SetCameraZoom ();
	}
	public float zoom = 30f;
	void SetCameraZoom () {

		Camera[] cameras = GetComponentsInChildren<Camera> () as Camera[];
		foreach (Camera cam in cameras) {
			cam.fieldOfView = zoom;
		}
		GetComponent<Camera>().fieldOfView= zoom;
	}
}
