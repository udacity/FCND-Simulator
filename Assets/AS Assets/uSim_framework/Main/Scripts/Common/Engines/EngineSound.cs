using UnityEngine;
using System.Collections;

public class EngineSound : MonoBehaviour {

	public float rpm;
	public AudioSource idleSource;
	public float idleRpm;
	public float idleOut;
	public float idleOutEnd;
	public AudioSource normalSource;
	public float normalRpm;
	public float normalIn;
	public float normalInEnd;
	public AudioSource farSource;
	public float idleVolume;
	public float normalVolume;
	public float masterVolume;
	public Engine engineComponent;
	
	public float pitchFactor;
	// Use this for initialization
	void Start () {
		engineComponent = GetComponent<Engine>();
		idleSource.time = idleSource.clip.length * Random.Range (0.1f, 1f);
		normalSource.time = normalSource.clip.length * Random.Range (0.1f, 1f);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		rpm = engineComponent.rpm;
		if (!engineComponent.stall) {
			masterVolume = Mathf.Clamp01 (0.3f + engineComponent.throttle);
		
			idleVolume = Mathf.Lerp (1f, 0f, Mathf.Clamp01 (((rpm - idleOut / idleOutEnd - idleOut) / idleOutEnd) + 0.5f));

		
			idleSource.pitch = (rpm / idleRpm) * pitchFactor;
		
			normalVolume = Mathf.Lerp (0f, 1f, Mathf.Clamp01 (((rpm - normalIn / normalInEnd - normalIn) / normalInEnd) + 0.5f));

			normalSource.pitch = (rpm / normalRpm) * pitchFactor;		


		
		} else if (engineComponent.stall) {
			masterVolume = 0f;

		}

		idleSource.volume = idleVolume * masterVolume;
		normalSource.volume = normalVolume * masterVolume;
		farSource.volume = Mathf.Clamp01 (((normalVolume - 0.5f) * engineComponent.throttle) * masterVolume);
	}
}
