using UnityEngine;
using System.Collections;


public class SimpleProp : MonoBehaviour {

	public Engine engineAttached;

	//inputs
	[HideInInspector]
	public float inputForce;

	//propieties
	public float propRadius;
	public float propMass;
	public AnimationCurve outputCurve;
	public AnimationCurve frictionCurve;

	public float friction;

	//[HideInInspector]
	public float rpm;
	[HideInInspector]
	public float torqueAtProp;
	//[HideInInspector]
	public float outputForce;
	[HideInInspector]
	public float frictionForce;
	[HideInInspector]
	public float propArea;
	public float reductionGear;
	public float thrustDir = 1f;

	//visuals
	public Transform propBlades;
	public MeshRenderer propBlur;

	//force apply
	public Rigidbody targetBody;
	public bool useCustomForcePoint;
	public Transform forcePoint;

	public float densityCoef = 1f;

    public float rpmBlur;
    public float maxRpm;

    public Transform customTransform;
  

    //Low pass time constant to add engine dynamics
    public float throttleAlpha = 0.1f;

	void Start () {
		propArea = Mathf.PI * (propRadius * propRadius) ;
		thrustDir = 1f;
	}

	void FixedUpdate () {

        //if (engineAttached == null)
        //	return;
        if (!useCustomForcePoint)
        {
            customTransform = transform;
            forcePoint = transform;
        }

        //thrustDir = engineAttached.thrustDir;

        //thrustDir = Mathf.Clamp (thrustDir, -1f, 1f);

        //inputForce = engineAttached.outputForce;

        //rpm = engineAttached.rpm / reductionGear;

        customTransform.Rotate (0f,0f,rpm /60f);

		//frictionForce = frictionCurve.Evaluate (rpm / 1000f) * friction;

		outputForce = (0.5f * rpm * propArea * outputCurve.Evaluate (rpm / 1000f) * densityCoef) * thrustDir;

		//engineAttached.addedFriction = frictionForce;

		if (targetBody != null)
			targetBody.AddForceAtPosition (customTransform.TransformDirection (Vector3.forward) * outputForce, forcePoint.position);

		if (rpm > rpmBlur) {
			propBlur.enabled = true;
            float blurRate = -0.5f + (rpm - rpmBlur) / (maxRpm - rpmBlur);
            propBlur.transform.Rotate(0f, 0f, blurRate);
			propBlades.gameObject.SetActive (false);
		}
		if (rpm < rpmBlur) {
			propBlur.enabled = false;
			propBlades.gameObject.SetActive (true);
		}
	}

    public void SetRPM(float r)
    {
        rpm = (1-throttleAlpha)*rpm+throttleAlpha*r;
    }
}
