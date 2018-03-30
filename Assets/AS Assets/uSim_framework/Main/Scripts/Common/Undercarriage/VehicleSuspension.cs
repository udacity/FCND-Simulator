using UnityEngine;
using System.Collections;

public class VehicleSuspension : MonoBehaviour {

	public bool remoteKinematic;
	public bool hitt;
	public Rigidbody anchoredTo;
	[HideInInspector]
	public Transform anchor;
	[HideInInspector]
	public Transform wheelAnchor;
	public VehicleWheel attachedWheel;
	public AnimationCurve compressionCurve;
	
	public float springForce;
	public float springDamping;
	public float runLength;
	public float rebound;
	
	public float runDist;
	public float compressionCoeff;
	public float compression;
	public float currentForce;
	private Vector3 initialPos;
	
	// Use this for initialization
	void Start () {
		
		anchor = transform.Find ("_anchor").transform;
		wheelAnchor = transform.Find ("_wheelAnchor").transform;
		initialPos = wheelAnchor.position;
		if (anchoredTo == null)
			anchoredTo = transform.root.GetComponent<Rigidbody> ();
	}
	RaycastHit hit;
	// Update is called once per frame
	void FixedUpdate () {
		
		//Spring
		Ray ray = new Ray (anchor.transform.position, anchor.transform.TransformDirection (Vector3.down));


		
		if (Physics.Raycast (ray, out hit, runLength + attachedWheel.radius)) { 

			if(hit.collider.tag != "VehCollider"){
			hitt = true;
				runDist = hit.distance - attachedWheel.radius;
			//data
							
			attachedWheel.onGround = true;
			
			
			
				//wheelAnchor.position = hit.point;
			
				if (!remoteKinematic) {

					attachedWheel.loadFromSusp = Mathf.Lerp (0, 1f, compression);
					compression = Mathf.InverseLerp (1f, 0f, runDist / runLength);
					compressionCoeff = compressionCurve.Evaluate (compression);
					currentForce = springForce * compressionCoeff;
			
					//Damp
					var t = anchoredTo.GetPointVelocity (anchor.position);
					t.z = t.x = 0;
					var shockDrag = t * springDamping;		
				
					Vector3 dir = Vector3.Lerp (Vector3.up, transform.TransformDirection (Vector3.up), Mathf.Clamp01(attachedWheel.Mz));
					anchoredTo.AddForceAtPosition (dir * currentForce, anchor.position);
					anchoredTo.AddForceAtPosition (anchor.transform.TransformDirection (Vector3.down) * shockDrag.y, anchor.position);
				}
		}
		} else {
			hitt = false;
			runDist = runLength;
			attachedWheel.onGround = false;
			attachedWheel.loadFromSusp = 0f;

		}
		wheelAnchor.position = Vector3.Lerp(wheelAnchor.position, anchor.transform.position + anchor.transform.TransformDirection (Vector3.down) * ( runDist ), Time.deltaTime * 3f);
	}
}
