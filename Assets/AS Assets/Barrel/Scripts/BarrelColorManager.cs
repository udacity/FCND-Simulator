using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class BarrelColorManager : MonoBehaviour {

	/**
	 * 
	 * Renderer and color options.
	 * Default color is blue.
	 *
	**/

	public 	Renderer 	BodyRenderer;
	public 	Color		BodyColor = Color.blue;

	/**
	 * 
	 * Reflection prob otpions
	 * Default mode is "Custom"
	 *
	**/

	public  enum 		ReflectMode { Custom, Realtime }
	public  ReflectMode ReflectionMode;
	public 	ReflectionProbe ReflectProbe;

	/**
	 * 
	 * Materialotpions
	 *
	**/

	public 	Material	ReferenceMaterial;
	private Material 	RuntimeMaterial;

	/**
	 * 
	 * Create new material for barrel body.
	 * Cloning refence material.
	 *
	**/

	void ChangeMaterial () {

		if (ReferenceMaterial) {

			RuntimeMaterial = new Material (ReferenceMaterial);

			RuntimeMaterial.name = "Runtime_Material";

			BodyRenderer.sharedMaterial = RuntimeMaterial;
		
		} else {

			Debug.Log ("Refence material hast not been selected.");
		}
	}

	/**
	 * 
	 * Change body material color.
	 *
	**/

	void ChangeColor () {

		if (RuntimeMaterial) {

			RuntimeMaterial.color = BodyColor;
		
		} else {

			Debug.Log ("Runtime material hast not been created.");
		}
	}

	/**
	 * 
	 * Config reflection probe.
	 *
	**/
	
	void ConfigReflectionProbe () {
		
		if (ReflectProbe) {

			if(ReflectionMode == ReflectMode.Realtime) {

				Debug.Log("Realtime mode is active.");

				ReflectProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
			
			} else {

				Debug.Log("Custom mode is active.");

				ReflectProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;
			}
			
		} else {
			
			Debug.Log ("ReflectProbe has not been selected.");
		}
	}

	/**
	 * 
	 * Start script.
	 *
	**/ 

	void Start () {

		ConfigReflectionProbe();

		ChangeMaterial();

		ChangeColor();
	}

	void Update () {

		if (! Application.isPlaying) {

			ChangeColor();
		}
	}
}