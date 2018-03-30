using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Underwater : MonoBehaviour {

	//This script enables underwater effects. Attach to main camera.

	public float underwaterLevel ;
	public Transform refObject;
	//The scene's default fog settings
	private bool defaultFog;
	private Color defaultFogColor;
	private float defaultFogDensity;
	private Material defaultSkybox;
	public Material noSkybox;

	void Start () {
		
		defaultFog = RenderSettings.fog;
		defaultFogColor = RenderSettings.fogColor;
		defaultFogDensity = RenderSettings.fogDensity;
		defaultSkybox = RenderSettings.skybox;
		GetComponent<Camera>().backgroundColor = new Color (0f, 0.4f, 0.7f, 1f);
	}

	void Update () {
		underwaterLevel = refObject.position.y;
		if (transform.position.y < underwaterLevel) {
			RenderSettings.fog = true;
			RenderSettings.fogColor = new Color (0f, 0.4f, 0.7f, 0.6f);
			RenderSettings.fogDensity = 0.04f;
			RenderSettings.skybox = noSkybox;
		}

		else {
			RenderSettings.fog = defaultFog;
			RenderSettings.fogColor = defaultFogColor;
			RenderSettings.fogDensity = defaultFogDensity;
			RenderSettings.skybox = defaultSkybox;
		}
	}
}
