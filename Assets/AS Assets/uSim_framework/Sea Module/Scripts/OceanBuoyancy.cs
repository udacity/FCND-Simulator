using UnityEngine;
using System.Collections;

public class OceanBuoyancy : MonoBehaviour {

	[System.Serializable]
	public class floatPoint {

		public Transform refPoint;
		public Transform bouyancyPoint;
		public float force;
		public float deltaFloat;
		public float buoyancy;
		public float sectionHeight;
		public AudioSource wakeSound;
		public ParticleSystem particles;
	}
	public float initialDrag;
	public Texture2D editorIcon;
	public floatPoint[] floatPoints;
	public Vector3 inertiaTensors;

	public float dragForce;
	float hullDragX = 0f;
	public float dragForceZ;
	float hullDragZ = 0f;
	public float dragY;

	public Rigidbody targetRigidbody;
	public AudioSource oceanSound;
	public Transform cog;

	// Use this for initialization
	void Start () {
		
		Vector3 tensors = targetRigidbody.inertiaTensor;
		tensors.x *= inertiaTensors.x;
		tensors.y *= inertiaTensors.y;
		tensors.z *= inertiaTensors.z;
		targetRigidbody.inertiaTensor = tensors;
		if(cog == null)
		cog = transform.Find ("Cog").transform;
		targetRigidbody.centerOfMass = cog.localPosition; 
		initialDrag = targetRigidbody.drag;
	}
	

	void FixedUpdate () {

		if (floatPoints.Length > 0) {
			oceanSound.volume = Mathf.Clamp01 (floatPoints [0].deltaFloat / 1f);
			oceanSound.pitch = Mathf.Lerp (0.5f, 0.5f + (hullDragZ / 10f), Time.deltaTime * 2f);
		}

		for (var i = 0 ; i < floatPoints.Length ; i++){

			Vector3 pointVelocity = targetRigidbody.GetPointVelocity (floatPoints[i].refPoint.position);
			floatPoint currentFp = floatPoints [i];
			float velocityY = currentFp.refPoint.InverseTransformDirection (pointVelocity).y;
			//Vector3 alignBuoyancy = currentFp.refPoint.InverseTransformPoint (new Vector3 (currentFp.refPoint.position.x,currentFp.bouyancyPoint.position.y,currentFp.refPoint.position.z));
			currentFp.bouyancyPoint.position = new Vector3(currentFp.refPoint.position.x,currentFp.bouyancyPoint.position.y,currentFp.refPoint.position.z);
			currentFp.deltaFloat = Mathf.Clamp( currentFp.bouyancyPoint.position.y  - currentFp.refPoint.position.y, 0f, currentFp.sectionHeight);
			/*if (transform.position.y > 20f)
				currentFp.deltaFloat = 0f;*/

			Vector3 hullDrag = targetRigidbody.GetPointVelocity (currentFp.bouyancyPoint.position);

			hullDragX = Mathf.Lerp (hullDragX, floatPoints[i].bouyancyPoint.InverseTransformDirection (hullDrag).x * dragForce, Time.deltaTime * 3f);
			hullDragZ = Mathf.Lerp (hullDragZ, floatPoints[i].bouyancyPoint.InverseTransformDirection (hullDrag).z * dragForceZ, Time.deltaTime * 3f);

			if(currentFp.deltaFloat < currentFp.sectionHeight ){
				currentFp.force = 1000f * 9.8f * currentFp.deltaFloat * currentFp.buoyancy;
			}

			if(currentFp.force < 0) currentFp.force = 0;

			if (currentFp.deltaFloat > 0) {				
				float directionality = currentFp.deltaFloat / currentFp.sectionHeight;
				Vector3 averageVector = Vector3.Lerp (currentFp.refPoint.TransformDirection (Vector3.up), Vector3.up, 1);	

				targetRigidbody.AddForceAtPosition (averageVector * currentFp.force, currentFp.refPoint.position);
				Vector3 forces = new Vector3 (hullDragX * -dragForce, velocityY * -dragY, hullDragZ * -dragForceZ * Mathf.Clamp01 (currentFp.deltaFloat));
				targetRigidbody.AddForceAtPosition (currentFp.bouyancyPoint.TransformDirection (forces), currentFp.bouyancyPoint.position);
				targetRigidbody.drag = initialDrag * (1 + (10f * Mathf.Clamp01 (directionality)));

				if (currentFp.wakeSound != null) {
					currentFp.wakeSound.volume =  Mathf.Lerp (currentFp.wakeSound.volume, Mathf.Clamp01 (velocityY * 5f), Time.deltaTime * 3f);
				}
				if (currentFp.particles != null) {
					currentFp.particles.startSize = Mathf.Clamp01 (0.5f + (currentFp.deltaFloat / 1f * hullDragZ)) * 3f;
					var emission = currentFp.particles.emission;
					var rate = emission.rate;
					rate.constantMax = hullDragZ;
					emission.rate = rate;

				}
			} else {
				targetRigidbody.drag = initialDrag;
				if (currentFp.wakeSound != null)
				currentFp.wakeSound.volume = Mathf.Lerp (currentFp.wakeSound.volume, 0f, Time.deltaTime );
				if (currentFp.particles != null) {
					currentFp.particles.startSize = 0f;
					var emission = currentFp.particles.emission;
					var rate = emission.rate;
					rate.constantMax = Mathf.Lerp (rate.constantMax, 0f, Time.deltaTime );
					emission.rate = rate;
				}
			}
		}

	}

	void OnDrawGizmosSelected () {

		for (int i = 0 ; i < floatPoints.Length ; i++){

			Gizmos.color = Color.blue;
			Gizmos.DrawIcon (floatPoints[i].refPoint.position + new Vector3(0,floatPoints[i].sectionHeight,0), editorIcon.name, true);

		}
	}
}
