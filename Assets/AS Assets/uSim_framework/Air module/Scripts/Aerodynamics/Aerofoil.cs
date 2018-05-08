// This script simulates the aerodynamic behaviour of an independent lift generator element of the aircraft. 
// class used for gethering information and aplying forces at the points contained in the LiftPoints array.

using UnityEngine;
using System.Collections;

public class Aerofoil : MonoBehaviour {

	[System.Serializable]
	public class ActionPoint
	{
		public Transform pos;
		[HideInInspector]
		public Vector3 airVector;
		[HideInInspector]
		public float airVectorZ;
		[HideInInspector]
		public float airVectorY;
		[HideInInspector]
		public float airVectorX;
		public bool variableShape;
		[HideInInspector]
		public float incidence;
		public HighliftDevice shapeModiffier;	
		[HideInInspector]
		public float changeFactor;
		[HideInInspector]
		public float slatFactor;
		[HideInInspector]
		public float pointSpeed;
		public float pointAngleOfAttack;
		public float liftCoef;
		public float dragCoef;
		public float drag;
		[HideInInspector]
		public float sectionLift;
		[HideInInspector]
		public float sectionDrag;	
		public float liftOnSection;
		[HideInInspector]
		public float swept;	
		[HideInInspector]
		public AnimationCurve slatedWingCurve;
	}

	public string curveName;
	public string dragCurveName;
	public bool generateDrag;
	public ActionPoint[] liftPoints;
//	public float wingAspectRatio;
	public float wingArea;
	public float efficencyFactor;
	public bool variableSwept;
	[HideInInspector]
	public float sweptPosition;
	public string sweptCurve;
	[HideInInspector]
	public float airDensity = 0.005f;
	int totalActionPoints;

	public Rigidbody targetRigidbody;
	AnimationCurve curve;
	AnimationCurve sweptWingCurve;
	AnimationCurve dragCurve ;
	CurvesManager curvesManager;
	private AtmosphericGlobals atmosphere;
	private bool dynamicAtmosphere;
	public bool groundEffect;

	void Start () {

		totalActionPoints = liftPoints.Length;
		curvesManager = transform.root.Find ("curvesManager").GetComponent<CurvesManager> ();
		aircraftControl = transform.root.GetComponent<AircraftControl> ();
		curve = curvesManager.GetCurve(curveName);
		if(generateDrag){
			dragCurve = curvesManager.GetCurve(dragCurveName);
		}
		if(variableSwept){
			
			sweptWingCurve = curvesManager.GetCurve (sweptCurve);
			
		}
		for (int i = 0; i < ( liftPoints.Length); i++){
			if(liftPoints[i].shapeModiffier == null) 
				return;
			liftPoints[i].slatedWingCurve = liftPoints[i].shapeModiffier.curve;
			
		}

		if (GameObject.FindObjectOfType<AtmosphericGlobals> () != null) {

			atmosphere = GameObject.FindObjectOfType<AtmosphericGlobals> ();
			if (atmosphere.fixedAtmosphere)
				airDensity = atmosphere.fixedDensity;
			else
				dynamicAtmosphere = true;
		}

	}
	AircraftControl aircraftControl;
	public float geCoef = 1f;
	void FixedUpdate () {

		for (int i = 0; i < ( liftPoints.Length); i++)
			
		{
			ActionPoint curLiftPoint = liftPoints[i];
			curLiftPoint.airVector = targetRigidbody.GetPointVelocity (curLiftPoint.pos.position);
			curLiftPoint.airVectorZ = transform.InverseTransformDirection (curLiftPoint.airVector).z;
			curLiftPoint.airVectorY = transform.InverseTransformDirection (curLiftPoint.airVector).y;
			curLiftPoint.airVectorX = transform.InverseTransformDirection (curLiftPoint.airVector).x;
            curLiftPoint.pointSpeed = curLiftPoint.airVectorZ;// *  4.280f;			
			curLiftPoint.pointAngleOfAttack = (Mathf.Atan2(-curLiftPoint.airVectorY, curLiftPoint.airVectorZ ) )* Mathf.Rad2Deg;
			curLiftPoint.changeFactor = 0;
			curLiftPoint.incidence = 0;

			if(curLiftPoint.variableShape){
				
				curLiftPoint.changeFactor =  curLiftPoint.shapeModiffier.factor;
			}

			float angleOfAttack = curLiftPoint.pointAngleOfAttack;

			if(groundEffect){
				geCoef = Mathf.Clamp01 (aircraftControl.trueAltitude / 10f);

				}
			var lift = curve.Evaluate (angleOfAttack);			
			if(curLiftPoint.shapeModiffier != null){
				if(curLiftPoint.shapeModiffier.isSlat) 					
					
					lift = Mathf.Lerp (lift,lift * curLiftPoint.shapeModiffier.slatFactor, curLiftPoint.shapeModiffier.slatPosition);  
			}			
			if(variableSwept){
				curLiftPoint.swept = sweptWingCurve.Evaluate(curLiftPoint.pointAngleOfAttack);
				lift= Mathf.Lerp (lift,lift * curLiftPoint.swept, sweptPosition);
			}	


			curLiftPoint.liftCoef = (lift + curLiftPoint.changeFactor) * Mathf.Lerp (1.25f, 1f, geCoef);												    //air density
			if (dynamicAtmosphere)
				airDensity = atmosphere.GetDensity(transform.position.y);
			

			curLiftPoint.sectionLift = ((0.5f) * airDensity *  curLiftPoint.liftCoef * (curLiftPoint.pointSpeed * curLiftPoint.pointSpeed) * (wingArea /totalActionPoints));
			
			if(generateDrag){			
				var drag = dragCurve.Evaluate (curLiftPoint.pointAngleOfAttack);	
				curLiftPoint.dragCoef = (drag + curLiftPoint.changeFactor);			
				curLiftPoint.drag = (airDensity * curLiftPoint.dragCoef * (wingArea /totalActionPoints) * 0.5f * (curLiftPoint.pointSpeed * curLiftPoint.pointSpeed)) * efficencyFactor;
			}				
			
			curLiftPoint.liftOnSection = curLiftPoint.sectionLift;
			
			Vector3 forcePos = curLiftPoint.pos.position;
			Vector3 dragVector = Vector3.zero;
			if (generateDrag){
				//targetRigidbody.AddForceAtPosition( ),forcePos);
				dragVector = (curLiftPoint.airVector.normalized * (-curLiftPoint.drag * Mathf.Lerp (0.75f, 1f, geCoef)));

			}						

			if(curLiftPoint.shapeModiffier != null)
				forcePos = forcePos + (curLiftPoint.pos.forward * (-curLiftPoint.shapeModiffier.liftPointShift * curLiftPoint.shapeModiffier.factor)); 
			
			Vector3 vector;
			vector = new Vector3(0,curLiftPoint.liftOnSection,0) ;
			targetRigidbody.AddForceAtPosition (curLiftPoint.pos.TransformDirection  (vector) + dragVector,forcePos);

		}
	
	}
}
