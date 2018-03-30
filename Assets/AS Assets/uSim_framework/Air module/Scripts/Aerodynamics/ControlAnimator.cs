using UnityEngine;
using System.Collections;

public class ControlAnimator : MonoBehaviour {

	[System.Serializable]
	public class ControlSurface {
		
		public Transform obj;
		public float maxDeflection;
		
	}

	public ControlSurface[] rightAilerons;
	public ControlSurface[] leftAilerons;
	public ControlSurface[] elevators;
	public ControlSurface[] rudders;
	public ControlSurface[] airBrakes;
	public ControlSurface[] slats;
	public ControlSurface[] sweptWings;
	public float initialSwept;
	
	public Transform[] flaps;
	public float[] flapangles;
	public float flapsSpeed = 1f;
	[HideInInspector]
	public int flapsIndex;
	public InputsManager inputs;
	public AircraftControl aircraftControl;
	[HideInInspector]
	public float flapangle;



	void Start () {
		inputs = GetComponent <InputsManager>();
		aircraftControl = GetComponent <AircraftControl>();

	}
	

	void FixedUpdate () {

		foreach (ControlSurface rightaileron in rightAilerons){
			
			if (inputs.aileron >= 0){
				Vector3 eulers = rightaileron.obj.localEulerAngles;
				eulers.x = Mathf.Lerp ( 0f , rightaileron.maxDeflection , Mathf.Abs(inputs.aileron));
				rightaileron.obj.localEulerAngles = eulers;
			}
			if (inputs.aileron <= 0){
				Vector3 eulers = rightaileron.obj.localEulerAngles;
				eulers.x = Mathf.Lerp ( 0f , -rightaileron.maxDeflection , Mathf.Abs(inputs.aileron));
				rightaileron.obj.localEulerAngles = eulers;
			}
		}
		foreach( ControlSurface leftaileron in leftAilerons){
			
			if (inputs.aileron >= 0){
				Vector3 eulers = leftaileron.obj.localEulerAngles;
				eulers.x = Mathf.Lerp ( 0f , -leftaileron.maxDeflection , Mathf.Abs(inputs.aileron));
				leftaileron.obj.localEulerAngles = eulers;
			}
			if (inputs.aileron <= 0){
				Vector3 eulers = leftaileron.obj.localEulerAngles;
				eulers.x = Mathf.Lerp ( 0f , leftaileron.maxDeflection , Mathf.Abs(inputs.aileron));
				leftaileron.obj.localEulerAngles = eulers;
			}

		}	

		foreach ( ControlSurface airBrake in airBrakes){


			Vector3 eulers = airBrake.obj.localEulerAngles;
			eulers.x = Mathf.Lerp ( 0f , airBrake.maxDeflection , Mathf.Clamp01 (inputs.airBrakes));
			airBrake.obj.localEulerAngles = eulers;
			if(inputs.airBrakes >= 0f)
				airBrake.obj.GetComponent<AirdragObject> ().activeRate = Mathf.Clamp01 (inputs.airBrakes);
			
				

		}
		
		
		foreach ( ControlSurface elevator in elevators){
			

				Vector3 eulers = elevator.obj.localEulerAngles;
			eulers.x = Mathf.Lerp ( -elevator.maxDeflection , elevator.maxDeflection ,0.5f + inputs.elevator + inputs.trimauto + inputs.trim);
				elevator.obj.localEulerAngles = eulers;

			
		}
		
		foreach ( ControlSurface rudder in rudders){
					
			Vector3 eulers = rudder.obj.localEulerAngles;
			eulers.y = Mathf.Lerp ( rudder.maxDeflection , -rudder.maxDeflection , 0.5f + inputs.rudder);
			rudder.obj.localEulerAngles = eulers;
			
			
			
		}
		
		foreach (ControlSurface slat in slats){

			Vector3 eulers = slat.obj.localEulerAngles;
			eulers.x = Mathf.Lerp (0, slat.maxDeflection, inputs.slats);
			slat.obj.localEulerAngles = eulers;
		}

		if (flaps.Length > 0) {
			if (inputs.flaps >= flapangles.Length - 1)
				inputs.flaps = flapangles.Length - 1;
			if (inputs.flaps < 0)
				inputs.flaps = 0;

			flapsIndex = inputs.flaps;

			flapangle = Mathf.Lerp (flapangle, flapangles [flapsIndex], Time.deltaTime * flapsSpeed);

			for (int i = 0; i < flaps.Length; i++) {
			
				Vector3 eulers = flaps [i].localEulerAngles;
				eulers.x = flapangle * -1f;
				flaps [i].localEulerAngles = eulers;
			}
		}

		foreach (ControlSurface swept in sweptWings){

			Vector3 eulers = swept.obj.localEulerAngles;
			eulers.y = Mathf.LerpAngle (initialSwept, swept.maxDeflection, inputs.swept);
			swept.obj.localEulerAngles = eulers;
			swept.obj.GetComponent <Aerofoil>().sweptPosition = Mathf.Clamp01(inputs.swept);
			
		}
	}
}
