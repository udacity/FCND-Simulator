using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using text = TMPro.TextMeshProUGUI;

public class TimestepTest : MonoBehaviour
{
	public text timestepText;
	public text rateText;
	public text framerateText;

	int updateCount;
	float lastUpdate;

	int frameCount;
	float frameUpdate;

	void Awake ()
	{
		Time.fixedDeltaTime = 0.02f;
		timestepText.text = "Timestep: " + Time.fixedDeltaTime.ToString ( "F2" ) + " (" + RateToHz (Time.fixedDeltaTime) + ")";
	}

	void LateUpdate ()
	{
		float fdt = Time.fixedDeltaTime;

		if ( Input.GetKeyDown ( KeyCode.Alpha1 ) )
			Time.fixedDeltaTime = 0.02f;
		if ( Input.GetKeyDown ( KeyCode.Alpha2 ) )
			Time.fixedDeltaTime = 0.01f;
		if ( Input.GetKeyDown ( KeyCode.Alpha3 ) )
			Time.fixedDeltaTime = 0.005f;
		if ( Input.GetKeyDown ( KeyCode.Alpha4 ) )
			Time.fixedDeltaTime = 0.002f;
		if ( Input.GetKeyDown ( KeyCode.Alpha5 ) )
			Time.fixedDeltaTime = 0.001f;
		
		if ( Time.fixedDeltaTime != fdt )
		{
			fdt = Time.fixedDeltaTime;
			timestepText.text = "Timestep: " + Time.fixedDeltaTime.ToString ( "F2" ) + " (" + RateToHz (Time.fixedDeltaTime) + ")";
		}

		frameCount++;
		float t = Time.unscaledTime - frameUpdate;
		if ( t > 1f )
		{
			framerateText.text = "FPS: " + ( 1f * frameCount / t ).ToString ( "F2" );
			frameCount = 0;
			frameUpdate = Time.unscaledTime;
		}
	}

	void FixedUpdate ()
	{
		updateCount++;
		float t = Time.unscaledTime - lastUpdate;
		if ( t > 1f )
		{
			rateText.text = "Updates: " + ( 1f * updateCount / t );
			updateCount = 0;
			lastUpdate = Time.unscaledTime;
		}
	}

	string RateToHz (float rate)
	{
		return ( 1f / rate ).ToString ("F2") + "Hz";
	}
}