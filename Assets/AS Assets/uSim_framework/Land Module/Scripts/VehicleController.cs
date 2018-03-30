using UnityEngine;
using System.Collections;

public class VehicleController : MonoBehaviour {

	public bool occupied;
	public bool isPlayer;
	public Transform cog;
	public float speed;
	public float acceleratorInput;
	public float steeringInput;
	public float brakingInput;
	public float clutchInput;
	public Transform[] steeringWheels;
	public float speedCoef;
	public float maxAngle;
	public Engine engine;
	public bool engineToggle;
	private Vector3 lastVelo;
	public float deltaZ;
	public Vector3 inertiaTensors;

	private float initialDrag;
	private float initialMass;
	private Vector3 initialCogPos;

	//transform reads
	public float transformSpeed;
	private Vector3 lastPosition;

	//Network
	private bool useNetwork;
	private NetworkView networkView;
	private bool local;
	private float remoteAngle;

	public InputsManager inputs;

	// Use this for initialization
	void Start () {
	
		cog = transform.Find ("CoG").transform;
		initialCogPos = cog.localPosition;
		GetComponent<Rigidbody> ().centerOfMass = cog.localPosition;
		initialDrag = GetComponent<Rigidbody> ().drag;
		initialMass = GetComponent<Rigidbody> ().mass;
		Vector3 inertia = GetComponent<Rigidbody> ().inertiaTensor;
		inertia.x = inertia.x * inertiaTensors.x;
		inertia.y = inertia.y * inertiaTensors.y;
		inertia.z = inertia.z * inertiaTensors.z;
		GetComponent<Rigidbody> ().inertiaTensor = inertia;

	}
	
	// Update is called once per frame
	void Update () {

		float z = Mathf.Lerp (initialMass /10000f, initialDrag, speed/1f);

		if (speed < 0.05f && speed > -0.05f) {

			GetComponent<Rigidbody> ().drag = Mathf.Lerp(GetComponent<Rigidbody> ().drag, z, Time.deltaTime * 3f);

		} else {

			GetComponent<Rigidbody> ().drag = Mathf.Lerp(GetComponent<Rigidbody> ().drag,initialDrag, Time.deltaTime * 3f);

		}

	
		steeringInput = inputs.steering;
		brakingInput = inputs.pedalBrake;
		acceleratorInput = inputs.accelerator;			
		clutchInput = inputs.engineClutch;

	}

	void FixedUpdate (){

		transformSpeed = GetTransformSpeed ();
		lastPosition = transform.position;
		Vector3 velo = GetComponent<Rigidbody> ().GetPointVelocity (transform.position);
		speed = transform.InverseTransformDirection (velo).z;
		deltaZ = transform.InverseTransformDirection (lastVelo).z - speed;

		
		Vector3 cogpos = cog.localPosition; 
		Vector3 newPos = Vector3.Lerp (initialCogPos,new Vector3 (initialCogPos.x,initialCogPos.y,initialCogPos.z + (deltaZ * 0.75f)), Time.deltaTime * 5f);
		cog.localPosition = newPos;

		if (acceleratorInput > 0f && engine != null) {

			engine.throttle =  acceleratorInput;

		}
		if (engine != null) {

			engine.clutch = clutchInput;

		}


		if(brakingInput < 0f) brakingInput = 0f;

		if (engineToggle) {
			
			transform.Find("engine/engineStarter").SendMessage ("startEngine");
			
		}

		for (int i = 0; i < steeringWheels.Length; i++) {

			Vector3 angles = steeringWheels[i].localEulerAngles;
			if(!local && useNetwork) {
				angles.y = remoteAngle;
			}
			else {

				angles.y = maxAngle * (steeringInput );
			steeringWheels[i].localEulerAngles  =  angles;
			}
		}

	
		lastVelo = velo;
		if (!useNetwork) {

			if(GetComponent<Rigidbody>().interpolation != RigidbodyInterpolation.Interpolate)
				GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

		}

		//Network
		if (Network.connections.Length > 0) {
			useNetwork = true;
			if(networkView == null)
				networkView = GetComponent<NetworkView>();
			local = networkView.isMine;
			if(local){
				if(GetComponent<Rigidbody>().interpolation != RigidbodyInterpolation.Interpolate)
					GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
				networkView.RPC ("SetRemoteSteeringAngle", RPCMode.All, maxAngle * steeringInput);
			}
			else{
				if(GetComponent<Rigidbody>().interpolation != RigidbodyInterpolation.None)
					GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
			}
		}
	}

	public float GetTransformSpeed (){

		return transform.InverseTransformDirection (transform.position - lastPosition).z * 60f;

	}

	[RPC]
	public void SetRemoteSteeringAngle (float inputAngle){

		remoteAngle = inputAngle;

	}

	void OnGUI(){
		//if(occupied && isPlayer)
	//	GUI.Label (new Rect (20f, 40f, 200f, 50f), "Speed: " + speed * 3.6f);
		
	}

}
