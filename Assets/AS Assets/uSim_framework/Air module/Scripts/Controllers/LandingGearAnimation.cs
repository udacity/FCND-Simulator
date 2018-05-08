using UnityEngine;
using System.Collections;

public class LandingGearAnimation : MonoBehaviour {

	public Animation animation;
	AnimationState gearAnimation;
	AircraftControl aircraftControl;
	public GameObject staticGear;
	public float retractSpeed;
	public float extendSpeed;

	public float gearTime;

	// Use this for initialization
	void Awake () {
		aircraftControl = transform.root.GetComponent <AircraftControl>();
		if (false && animation == null && GetComponent<Animation>() ["gear"] != null){			
			gearAnimation = GetComponent<Animation>() ["gear"];
			//animation = GetComponent<Animation> ();
			gearAnimation.enabled = true;
			gearAnimation.weight = 1f;
			gearTime = 0.05f;
			gearAnimation.normalizedTime = gearTime;
			animation.Stop ();
		}
        if(false)
		gearAnimation = animation ["gear"];
	}

	public void SetStart (bool up){
        if (gearAnimation == null)
            return;
		if (up) {			

			gearAnimation = animation ["gear"];
			gearAnimation.normalizedTime = 1f;
			gearTime = 0.99f;
			animation.gameObject.SetActive (true);
			staticGear.SetActive (false);
			aircraftControl.gearDwn = false;
			gearAnimation.enabled = true;
			gearAnimation.weight = 1f;
			animation.Play ();

		} else {

			gearAnimation = animation ["gear"];
			gearAnimation.normalizedTime = 0f;
			gearTime = 0f;
			animation.gameObject.SetActive (false);
			staticGear.SetActive (true);
			aircraftControl.gearDwn = true;
		}


	}

	public void ToggleGear (bool toggle){
		
		StartCoroutine (HandleGear(toggle));

	}

	IEnumerator HandleGear (bool toggle){

		yield return AnimateGear(toggle);
		yield return true;
	}

	
	// Update is called once per frame
	IEnumerator AnimateGear (bool retract) {
	
		AirdragObject airDragObj = null;
		if (staticGear.GetComponent<AirdragObject> () != null)
			airDragObj = staticGear.GetComponent<AirdragObject> ();
			

		if (retract) {					

			animation.gameObject.SetActive (true);
			gearAnimation = animation ["gear"];
			//animation = GetComponent<Animation> ();
			gearAnimation.enabled = true;
			gearAnimation.weight = 1f;
			gearAnimation.time = 0f;
			staticGear.SetActive (false);
			gearAnimation.speed = retractSpeed;
			animation.Play ();

			yield return new WaitForEndOfFrame ();
			do {				
					/*gearTime += retractSpeed * Time.deltaTime;						
				if (gearTime < 0.5f)
					aircraftControl.wheelon = false;	
				if (gearAnimation != null)
					gearAnimation.normalizedTime = gearTime;*/
				if(airDragObj != null)
					airDragObj.activeRate = gearAnimation.normalizedTime;
				yield return new WaitForEndOfFrame ();
			
			} while (gearAnimation.normalizedTime < 1f);
			gearAnimation.speed = 0f;
			//animation.Stop ();
		} else {

			gearAnimation = animation ["gear"];
			//animation = GetComponent<Animation> ();
			gearAnimation.enabled = true;
			gearAnimation.weight = 1f;
			gearAnimation.normalizedTime =0.99f;
			gearAnimation.speed = -extendSpeed;
			animation.Play ();
			yield return new WaitForEndOfFrame ();
			do {							
					/*gearTime -= extendSpeed * Time.deltaTime;
				if (gearTime > 0.5f)
				aircraftControl.wheelon = true;
				if (gearAnimation != null)
					gearAnimation.normalizedTime = gearTime;*/
				if(airDragObj != null)
					airDragObj.activeRate = gearAnimation.normalizedTime;
				yield return new WaitForEndOfFrame ();
			} while (animation.isPlaying);
			gearAnimation.speed = 0f;
			//animation.Stop ();
			animation.gameObject.SetActive (false);
			staticGear.SetActive (true);

	    }


		yield return true;
	}

	void FixedUpdate(){

		if(false && gearAnimation.normalizedTime < 1f && gearAnimation.normalizedTime > 0f)
		gearTime = gearAnimation.normalizedTime;

	}
}
