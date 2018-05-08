using UnityEngine;
using System.Collections;

public class VehicleWheel : MonoBehaviour {

	public bool remoteKinematic;
	public bool ignoreController;
	public Rigidbody attachedTo;
	public VehicleSuspension attachedSuspenssion;
	public AnimationCurve latitudinalGripCurve;
	public AnimationCurve longitudinalGripCurve;
	public AnimationCurve loadFactorCurve;
	public float maxGripForce;
	public float maxLongForce;
	public float radius;
	public float wheelMass; //kg
	public bool isDriven;
	public Transform model;
	public AudioSource skidSound;

	//compute values
	public float deltaRpm; //radianes * segundo

	[HideInInspector]
	public Vector3 pointVelocity;
	//[HideInInspector]
	public float pvX;
//	[HideInInspector]
	public float pvZ;
	//[HideInInspector]
	public float sideSlip;	
	//[HideInInspector]
	public float longSlip;	
	//[HideInInspector]
	public float loadFactor;	
	//[HideInInspector]
	public bool onGround;

	public float rpm;
	//[HideInInspector]
	public float trpm;
	//[HideInInspector]
	public float rpmFromEngine;
	//[HideInInspector]
	public float rpmDiff;
	//[HideInInspector]
	public float loadFromSusp;

	public float gripFactor;
	public float gripForce;
	public float lGripFactor;

	//[HideInInspector]
	public float Mx;
//	[HideInInspector]
	public float Mz;

	public float wheelInertia;
	public float angularAccel;
	public float angularV;
	private float speed;
	public float maxDeltaRPM;
	private RaycastHit hit;

	public Transform contactPoint;

	private Vector3 initPos;

	public float computForce;
	public float inputForce;
	public float driveForce;
	public float brakeForce;
	public float roadDragCoef = 0.1f;
	public float roadForce;
	public float outputForceL;

	public float currentGearRatio;

	public float diferentialRatio;
	public VehicleController controller;
	private InputsManager inputs;
	public float wheelClutch;
	public float wheelBrakeInput;

	// Use this for initialization
	void Start () {

		attachedSuspenssion = transform.parent.GetComponent<VehicleSuspension> ();
	if(attachedSuspenssion != null){

			initPos = attachedSuspenssion.wheelAnchor.localPosition;
			transform.localPosition = initPos;
		}
		if (attachedTo == null)
			attachedTo = transform.root.GetComponent<Rigidbody> ();
		
		controller = transform.root.GetComponent<VehicleController>();
	
			inputs = transform.root.GetComponent<InputsManager>();
		wheelInertia = wheelMass * (radius * radius) / 2; 
	}	

	void FixedUpdate () {
		float bForce = 0f;

		if(!ignoreController){

			if (!remoteKinematic) {
				bForce = 0f;
				if (controller != null) {
					bForce = Mathf.Clamp01 (controller.brakingInput + inputs.parkingBrake + wheelBrakeInput) * brakeForce;
					if (bForce < 0f)
						bForce = 0f;
				}
				if (controller == null) {

					bForce = Mathf.Clamp01 (inputs.wheelBrake + inputs.parkingBrake) * brakeForce;

				}
			}

		}

		if (rpm > rpmFromEngine && isDriven)
			rpm = rpmFromEngine;

		if (Physics.Raycast (transform.position, transform.TransformDirection(Vector3.down), out hit, radius * 1.15f)) //transform.TransformDirection(Vector3.down), out hit, radius * 1.15f))
        { 

			onGround = true;

			if(contactPoint != null) contactPoint.position = hit.point;

			if (!remoteKinematic) {
				
				pointVelocity = attachedTo.GetPointVelocity (hit.point);
				pvX = pointVelocity.x;
				pvZ = pointVelocity.z;
				Mx = transform.InverseTransformDirection (pointVelocity).x;
				Mz = transform.InverseTransformDirection (pointVelocity).z;

				sideSlip = (Mathf.Atan2 (Mx, Mz)) * Mathf.Rad2Deg;
								
				roadForce = (roadDragCoef * attachedSuspenssion.compressionCoeff) * (Mz*Mz) ;
				loadFactor = loadFactorCurve.Evaluate (loadFromSusp);
								
				gripFactor = latitudinalGripCurve.Evaluate (sideSlip);

				if (loadFactor < 0f)
					loadFactor = 0f;
		
				bForce = bForce  * Mathf.Clamp (rpm * 5f,-1f,1f);

				gripForce =  Mathf.Lerp((maxGripForce * gripFactor) * loadFactor, (maxGripForce * (gripFactor /1.3f)) * loadFactor,(Mathf.Clamp01(Mathf.Abs (longSlip)))) *  Mathf.Clamp01 (Mathf.Abs(Mx));
				attachedTo.AddForceAtPosition (transform.TransformDirection(Vector3.right) *   -gripForce, hit.point);

				trpm = Mz / ( 2 * 3.14f * radius )*60f;

				deltaRpm = (rpm - trpm);
				longSlip = Mathf.Clamp (deltaRpm / maxDeltaRPM, -1f,1f);
				lGripFactor = longitudinalGripCurve.Evaluate (longSlip );
				computForce = Mathf.Lerp ((maxLongForce * lGripFactor),maxLongForce * (lGripFactor-(Mathf.Abs(gripFactor) /2f)),( Mathf.Abs(sideSlip) /35f) * Mathf.Clamp01(Mz));
				//computForce = maxLongForce * lGripFactor;
				driveForce = ( inputForce) - bForce- computForce - roadForce;
				float accel = ((inputForce) - bForce - (computForce * 0.5f) - roadForce);			
				angularAccel = Mathf.Lerp (angularAccel,(accel) / wheelInertia, Time.deltaTime * 10f);

			

				if (isDriven) {

					outputForceL = ((inputForce ) * radius) * (1f - wheelClutch);
					attachedTo.AddForceAtPosition (transform.TransformDirection (Vector3.forward) * outputForceL  * Mathf.Abs (lGripFactor) * (-Physics.gravity.y), hit.point);
					rpm += (angularAccel  / (2 * Mathf.PI)) * (Time.deltaTime * wheelInertia);

				}
					else {
					rpm += angularAccel * Time.deltaTime * 5f * wheelInertia;
									
				}
				attachedTo.AddForceAtPosition (transform.TransformDirection (Vector3.forward) * (computForce * 2.5f ) , hit.point);
				if (Mz > 0 && rpm <= 0f &&  Mathf.Abs (bForce) > wheelMass) {
					angularAccel = 0f;
					rpm = 0f;
					bForce = 0f;
				}
				if (Mz < 0 && rpm >= 0f &&  Mathf.Abs (bForce) > wheelMass) {
					angularAccel = 0f;
					rpm = 0f;
					bForce = 0f;
				}

			
					
			
				
				model.Rotate ((rpm * (radius * 1.5f)) * 60 / 360, 0f, 0f);	

			}
			if(remoteKinematic){
				transformSpeed = GetTransformSpeed ();
				lastPosition = transform.position;
				speed = transformSpeed;
				rpm = speed / ((2 * 3.14f) * (radius)) * 60f;		
				model.Rotate ((rpm * radius * 2f) * 60 / 360, 0f, 0f);	
			}

			if(skidSound != null)
				skidSound.volume = Mathf.Abs ((gripFactor+lGripFactor)/2 * Mathf.Clamp(Mz,-1f,1f) )/5f;


		}//in the air.
		else {
			onGround = false;
		if(contactPoint != null) 	contactPoint.position =  transform.position + transform.TransformDirection (Vector3.down) * radius ;
			if (!isDriven)
				rpm = Mathf.Lerp (rpm, 0f, Time.deltaTime / 4f);
			else
				rpm = rpmFromEngine * radius;
			computForce = 0f;	
		}

		rpmDiff =  trpm - rpm ;		

		if (attachedSuspenssion != null) {
			
			transform.position = attachedSuspenssion.wheelAnchor.position;
						
		}	

	}

	//transform reads
	public float transformSpeed;
	private Vector3 lastPosition;
	public float GetTransformSpeed (){
			
		return transform.InverseTransformDirection (transform.position - lastPosition).z * 60f;

	}
}
