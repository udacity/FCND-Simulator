using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowingLine : MonoBehaviour {

	public Rigidbody fromVehicle;
	public Transform fromPoint;
	public Rigidbody toVehicle;
	public Transform toPoint;

	public float lineForce;
	public float maxLineForce;
	public float normalLength;
	public float maxStreach;

	public LineRenderer lineRenderer;

	public bool deployed;
	public float currentStreach;
	public float streachAmount;
	public float force;
	public AnimationCurve bendCurve;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (!deployed) {
			lineRenderer.enabled = false;
			return;
		}

		if(!lineRenderer.enabled)
			lineRenderer.enabled = true;


		for (int i = 0; i < lineRenderer.positionCount; i++){

			Vector3 currentPosition = Vector3.Lerp (fromPoint.position, toPoint.position, ((float)i / (float)(lineRenderer.positionCount)));

			Vector3 middlePos = Vector3.Lerp (fromPoint.position, toPoint.position, 0.5f);
			middlePos.y = Mathf.Lerp (((fromPoint.position.y + toPoint.position.y) / 2) - (normalLength / 2), (fromPoint.position.y + toPoint.position.y) / 2, currentStreach / normalLength);

			float yPosCoef = (float)i / (float)(lineRenderer.positionCount);
			/*if (yPosCoef > 1)
				yPosCoef =  (((yPosCoef - 1f) - 1f) * -1f) ;*/
			if(yPosCoef <= 0.5f)
				currentPosition.y = Mathf.Lerp (fromPoint.position.y, middlePos.y, bendCurve.Evaluate (yPosCoef)) *  Mathf.Clamp01 (currentStreach);
			else
				currentPosition.y = Mathf.Lerp (toPoint.position.y, middlePos.y, bendCurve.Evaluate (yPosCoef)) *  Mathf.Clamp01 (currentStreach);
			
			lineRenderer.SetPosition (i, currentPosition);
					
		}

		
		lineRenderer.SetPosition (0, fromPoint.position);
		//Vector3 middlePos = Vector3.Lerp (fromPoint.position, toPoint.position, 0.5f);
		//middlePos.y = Mathf.Lerp ((normalLength / 2), (fromPoint.position.y + toPoint.position.y) / 2, currentStreach / normalLength);
		RaycastHit hit;
		//if (Physics.Raycast (middlePos, Vector3.down,out hit, 0.5f))
			//middlePos.y = hit.point.y;
		//lineRenderer.SetPosition (lineRenderer.positionCount / 2, middlePos);
		lineRenderer.SetPosition (lineRenderer.positionCount-1, toPoint.position);

		float distance = Vector3.Distance (fromPoint.position, toPoint.position);
		currentStreach = distance;
		if (distance >= normalLength) {

			streachAmount = (currentStreach - normalLength);
			force = lineForce * (streachAmount * streachAmount);
			Vector3 lineDirection = toPoint.position - fromPoint.position;
			toVehicle.AddForceAtPosition (lineDirection.normalized * -force, toPoint.position);
			fromVehicle.AddForceAtPosition (lineDirection.normalized * force, fromPoint.position);
		}
	}
}
