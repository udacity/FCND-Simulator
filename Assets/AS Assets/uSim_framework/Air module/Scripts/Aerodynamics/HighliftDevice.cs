using UnityEngine;
using System.Collections;

public class HighliftDevice : MonoBehaviour {

	Rigidbody targetRigidbody;
	public AnimationCurve curve;
	public string curveName;
	Vector3 airVector;
	float airVectorZ;
	float airVectorY;
	float angleOfAttack;
	public float factor;
	public bool isSlat;
	public float slatPosition;
	public float slatFactor;
	public float liftPointShift;
	InputsManager inputsManager;


	void Start () {
		inputsManager = transform.root.GetComponent <InputsManager>();
		curve = transform.root.Find("curvesManager").GetComponent<CurvesManager>().GetCurve(curveName);
		if(GetComponent<Rigidbody>() != null && targetRigidbody == null) targetRigidbody = GetComponent<Rigidbody>();
		else if ( targetRigidbody == null) targetRigidbody = transform.root.GetComponent<Rigidbody>();
	}
	

	void FixedUpdate () {
	
		airVector = targetRigidbody.GetPointVelocity (transform.position);
		airVectorZ = transform.InverseTransformDirection (airVector).z;
		airVectorY = transform.InverseTransformDirection (airVector).y;
		if (!isSlat){
			angleOfAttack = ((Mathf.Atan2(-airVectorY, airVectorZ ) )* Mathf.Rad2Deg);
			factor = curve.Evaluate (angleOfAttack);		
		}
		else{
			slatPosition = inputsManager.slats;
			slatFactor = curve.Evaluate (slatPosition);
			
		}

	}

}
