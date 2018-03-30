using UnityEngine;
using System.Collections;

public class VehicleGearBox : MonoBehaviour {

	[System.Serializable]
	public class Gear {

		public string displayName;
		public float ratio;

	}

	public Gear[] gears;
	public int currIndex;
	public float curRatio;
	public float inputForce;
	public Engine attachedEngine;
	public Differential attachedDifferential;
	public VehicleController controller;
	// Use this for initialization
	void Start () {
		controller = transform.root.GetComponent<VehicleController>();
		currIndex = 1;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyUp (KeyCode.I) && currIndex < gears.Length-1)
			currIndex++;
		if (Input.GetKeyUp (KeyCode.K) && currIndex > 0)
			currIndex--;

		curRatio = gears [currIndex].ratio;
		if (curRatio == 0) {

			attachedEngine.clutch = 1;
			return;
		}
		if (attachedEngine != null) {
			float rpm = (attachedDifferential.fRpm * attachedDifferential.diffRatio * curRatio ) / 2 * Mathf.PI;
		
			attachedEngine.rpmFromAttach = rpm;
			attachedEngine.brakeFromAttach = ((attachedDifferential.wheels [0].computForce) / attachedDifferential.diffRatio / curRatio) * Mathf.Lerp (0.5f,0f, attachedEngine.clutch /1f);
			inputForce = attachedEngine.outputForce;
		}
		if (attachedDifferential != null)
			attachedDifferential.SetOutput (inputForce * curRatio);
	}

	void OnGUI(){
		if(controller.occupied && controller.isPlayer)
			GUI.Label (new Rect (20f, Screen.height - 20f, 200f, 50f), "Gear: " + gears [currIndex].displayName);

	}
}
