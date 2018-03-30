using UnityEngine;
using System.Collections;

public class AircraftControl : MonoBehaviour {

	public Vector3 inertiaTensors;	
	public float speed;
	int stallSpeedWarning;
	[HideInInspector]
	public int ias;
	[HideInInspector]
	public float verticalSpeed;
	Vector3 fusAero;
	float fusAeroX;
	float fusAeroY;
	float FusAeroZ;
	[HideInInspector]
	float angleOfAttack;
	public Transform cog;
	[HideInInspector]
	public bool gearDwn;
	public bool isRGear;
	public Transform steerWheel;
	public bool isTailWheel;
	public float maxSteerAngle;
	
	float aeroBrake;
	bool aerobrakeon;

	float aileronInput;
	float elevatorInput;
	float trimInput;
	float rudderInput;
	InputsManager inputs;
	Rigidbody aircraft;
	[HideInInspector]
	public bool wheelon;
	[HideInInspector]
	public bool parkingBrake;
	[HideInInspector]
	public bool tailwheelLock;

	private float initialDrag;
	private float initialRotDrag;
	private Vector3 initialCogPos;
	private Vector3 lastVelo;
	[HideInInspector]
	public float deltaZ;
	public float trueAltitude;
	public bool useAutoTrim;
	public float autoTrimValue;
	public AnimationCurve autoTrimCurve;

	// Use this for initialization
	void Start () {

		cog = transform.Find ("Cog").transform;
		inputs = GetComponent <InputsManager>();
		aircraft = GetComponent<Rigidbody> ();
		if(GetComponentInChildren<LandingGearAnimation> () != null)
			GetComponentInChildren<LandingGearAnimation> ().SetStart (!gearDwn);
		wheelon = false; 

		Vector3 tensors = aircraft.inertiaTensor;
		tensors.x *= inertiaTensors.x;
		tensors.y *= inertiaTensors.y;
		tensors.z *= inertiaTensors.z;
		aircraft.inertiaTensor = tensors;

		initialDrag = aircraft.drag;
		initialRotDrag = aircraft.angularDrag;
		initialCogPos = cog.localPosition;

		Vector3 vel = aircraft.velocity;
		vel = transform.TransformDirection (Vector3.forward * speed);
		aircraft.velocity = vel;
				
		tailwheelLock = true;
		aircraft.centerOfMass = cog.localPosition; 
	}
	
	public Transform altCaster;
	void FixedUpdate () {
		

		if (inputs.occupied) {
			RaycastHit hit = new RaycastHit ();
			if (Physics.Raycast (altCaster.position, Vector3.down, out hit)) {
				trueAltitude = hit.distance;
			}

			float z = Mathf.Lerp (aircraft.mass / 1000f, initialDrag, speed / 1f);
			float y = Mathf.Lerp (aircraft.mass / 3000f, initialRotDrag, speed / 1f); 
		
			if (speed < 0.05f && speed > -0.05f) {
			
				aircraft.drag = Mathf.Lerp (aircraft.drag, z, Time.deltaTime * 3f);
				//aircraft.angularDrag = Mathf.Lerp (aircraft.angularDrag, y, Time.deltaTime * 3f);

			
			} else {
			
				aircraft.drag = Mathf.Lerp (aircraft.drag, initialDrag, Time.deltaTime * 3f);
				//aircraft.angularDrag = Mathf.Lerp (aircraft.angularDrag, initialRotDrag, Time.deltaTime * 3f);
			
			}
		} else {

			aircraft.drag = aircraft.mass / 500f;
			//aircraft.angularDrag = Mathf.Lerp (aircraft.angularDrag,  aircraft.mass / 300f, Time.deltaTime * 3f);
		}
		fusAero = aircraft.GetPointVelocity (transform.position);
		fusAeroX = transform.InverseTransformDirection (fusAero).x;
		fusAeroY = transform.InverseTransformDirection (fusAero).y;
		FusAeroZ = transform.InverseTransformDirection (fusAero).z;
		speed = FusAeroZ;
		ias = Mathf.FloorToInt (speed * 1.94f); 
		


		verticalSpeed = aircraft.velocity.y;
	
		//steering wheel.
		rudderInput = inputs.rudder;
		
		Vector3 steerWheelEulers = steerWheel.localEulerAngles;
		if (!isTailWheel ) {
			
			steerWheelEulers.y = maxSteerAngle * rudderInput;
		} else {
			if(!tailwheelLock)
			steerWheelEulers.y = -maxSteerAngle * rudderInput;
		}
		if (isTailWheel && tailwheelLock) {
			steerWheelEulers.y = 0f;
		}
			
		steerWheel.localEulerAngles = steerWheelEulers;
	
		deltaZ = transform.InverseTransformDirection (lastVelo).z - speed;

		Vector3 cogpos = cog.localPosition; 
		Vector3 newPos = Vector3.Lerp (initialCogPos,new Vector3 (initialCogPos.x,initialCogPos.y,initialCogPos.z + (deltaZ * 0.75f)), Time.deltaTime * 5f);
		cog.localPosition = newPos;

		lastVelo = aircraft.velocity;

		if (useAutoTrim) {
			autoTrimValue = autoTrimCurve.Evaluate (ias / 100f);
			inputs.trim = autoTrimValue;
		}
	}

	void OnGUI(){
		if(parkingBrake)
		GUI.Label (new Rect (Screen.width -170f,Screen.height -30f, 250f, 30f), "Parking brake set!");
	//	if(!tailwheelLock)
		//	GUI.Label (new Rect (Screen.width -200f,Screen.height -60f, 250f, 30f), "Tail wheel Unlocked!");

	}
}
