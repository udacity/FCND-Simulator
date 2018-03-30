using UnityEngine;
using System.Collections;

public class CarCamera : MonoBehaviour {

	public MonoBehaviour[] cameraControllers; 
	public MonoBehaviour lastController;
	public int index;

	//private VirtualCockpit vc;
	
	/*IEnumerator Start () {
		
		panel = transform.root.transform.GetComponentInChildren<ScreenPanel> ();

		do {

			vc = transform.root.transform.GetComponent<AircraftEntity>().vc;
			yield return new WaitForSeconds(0);

		} while (vc == null);

	}*/
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyUp (KeyCode.F1)) {

			index = 0;

			if(lastController != null){
				lastController.enabled = false;
				RestoreCameraZoom ();

//				panel.show = true;
				//vc.show = true;
			}


		}
		if (Input.GetKeyUp (KeyCode.F2) && cameraControllers.Length > 1) {
			if(lastController != null){
				lastController.enabled = false;
				RestoreCameraZoom ();

//				panel.show = false;
				//vc.show = false;
			}
			index = 1;
			
		}
		if (Input.GetKeyUp (KeyCode.F3) && cameraControllers.Length > 2) {
			if(lastController != null){
				lastController.enabled = false;
				lastController.SendMessage ("SetupCamera", SendMessageOptions.DontRequireReceiver);

	//			panel.show = false;
				//vc.show = false;
			}
			index = 2;
			
		}

		if (Input.GetKeyUp (KeyCode.F4) && cameraControllers.Length > 3) {
			if(lastController != null){
				lastController.enabled = false;

				//			panel.show = false;
				//vc.show = false;
			}
			index = 3;

		}
		cameraControllers [index].enabled = true;
		cameraControllers [index].SendMessage ("ResetPos", SendMessageOptions.DontRequireReceiver);
		lastController = cameraControllers [index];
	}

	void Awake () {
		 
		initialZoom = GetComponent<Camera> ().fieldOfView;

	}

	float initialZoom;
	void RestoreCameraZoom () {

		Camera[] cameras = GetComponentsInChildren<Camera> () as Camera[];
		foreach (Camera cam in cameras) {
			cam.fieldOfView = initialZoom;
		}
		GetComponent<Camera>().fieldOfView= initialZoom;
	}
}
