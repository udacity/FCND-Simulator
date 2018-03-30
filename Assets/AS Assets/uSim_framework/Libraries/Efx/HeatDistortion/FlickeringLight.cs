using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour {
    Light light;
    Transform transform;
    Vector3 originalPosition;
    public float intensityFlickerStrength;
    public float intensityFlickerSpeed;
    public float positionFlickerStrength;
    public float positionFlickerSpeed;

	void Start () {
        light = GetComponent<Light>();
        this.transform = GetComponent<Transform>();
        originalPosition = transform.position;
	}
	
	void Update () {
        light.intensity += (Mathf.Sin(Time.realtimeSinceStartup * intensityFlickerSpeed)) * intensityFlickerStrength;
        Vector3 positionOffset = new Vector3(Mathf.Sin(Time.realtimeSinceStartup * positionFlickerSpeed),
                                             Mathf.Cos(Time.realtimeSinceStartup * positionFlickerSpeed),
                                             Mathf.Sin(Time.realtimeSinceStartup * positionFlickerSpeed * 2));
        transform.position = originalPosition + positionOffset * positionFlickerStrength;
	}
}
