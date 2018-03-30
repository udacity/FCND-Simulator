using UnityEngine;
using System.Collections;

public class AerofoilSounds : MonoBehaviour {

	Aerofoil aerofoil;
	AudioSource audioSource;
	public AnimationCurve volumeCurve;


	// Use this for initialization
	void Start () {
	
		aerofoil = GetComponent<Aerofoil> ();
		audioSource = GetComponentInChildren<AudioSource> ();

	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		float speedVol = aerofoil.liftPoints [0].pointSpeed / 50f;
		audioSource.volume = volumeCurve.Evaluate (aerofoil.liftPoints [0].pointAngleOfAttack) * speedVol;

	}
}
