using UnityEngine;
using System.Collections;

public class Gauge2D : MonoBehaviour {

	public enum gaugeMode {Airspeed, Altimeter, VerticalSpeed};

	public Vector2 offsets;
	public Vector2 size;
	public float gaugeRotation;
	public Texture2D background;
	public Texture2D needle;
	public Texture2D needle2;
	public bool useCustomAnimation;
	public AnimationCurve needleAnimation;
	public float turnValue;//maxValue
	public gaugeMode mode;
	public float inputValue;
	public float needle2Rate;
	public float angle;
	public float angle2;

	public bool showBox;

	private ScreenPanel panel;

	void Start () {
	
		panel = transform.parent.GetComponent<ScreenPanel> ();

	}
	

	void Update () {
	
		switch (mode) {

		case gaugeMode.Airspeed :

			inputValue = GetSpeed();

			break;

		case gaugeMode.Altimeter :
			
			inputValue = GetAlt();
			
			break;


		case gaugeMode.VerticalSpeed :
			
			inputValue = Mathf.Lerp (inputValue, GetVSpeed(), Time.deltaTime * 5f);
			
			break;
		}

		SetAnimtime ();

	}

	private float GetSpeed (){

		Vector3 velocity = transform.root.GetComponent<Rigidbody> ().GetPointVelocity (transform.position);

		return (transform.InverseTransformDirection (velocity).z * 1.94f);//Kts

	}

	private float GetAlt (){
		
		float altitudeSealvl = transform.root.transform.position.y;

		return altitudeSealvl;//mts
		
	}

	private float GetVSpeed(){

		float vspeed = transform.root.GetComponent<Rigidbody>().velocity.y;

		return vspeed;
	}


	private void SetAnimtime (){

		if (useCustomAnimation) {

			float animTime = needleAnimation.Evaluate (inputValue);
			angle = 360f * animTime;
		} else {

			angle = inputValue / turnValue;
		}

		angle2 = angle * needle2Rate;

	}

	void OnGUI () {

		Rect rect = new Rect (panel.panelRect.x + offsets.x, 
		                      panel.panelRect.y + offsets.y, size.x, size.y);

		if (showBox)
			GUI.Box (rect, background);
		else 
			GUI.Label (rect, background);

		var p = new Vector2(rect.x + rect.width/2,rect.y+ rect.height/2); // find the center
		Matrix4x4 svMat = GUI.matrix; // save gui matrix	

		//needle 1
		GUIUtility.RotateAroundPivot(angle + gaugeRotation,p); // prepare matrix to rotate
		GUI.DrawTexture(rect,needle); // draw the needle rotated by angle

		//needle 2	
		if (needle2 != null){
			GUIUtility.RotateAroundPivot(angle2 + gaugeRotation,p); // prepare matrix to rotate
			GUI.DrawTexture(rect,needle2); // draw the needle rotated by angle
			}

		GUI.matrix = svMat; // restore gui matrix

	}
}
