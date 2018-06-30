using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ttext = TMPro.TextMeshProUGUI;

[System.Serializable]
public class VisualizationParameter
{
	public string name;
	public float minValue;
	public float maxValue;
	public float value;
	public bool useAcceptableRange;
	public float minAcceptableValue;
	public float maxAcceptableValue;
	public string suffix = "";
}

public class UIVizParameter : MonoBehaviour
{
	public Ttext titleText;
	public Ttext valueText;
	public Image bar;
	public Image indicator;

	public bool useFill;

	float min;
	float max;
	float minAcceptable;
	float maxAcceptable;
	bool useAcceptableRange;
	string suffix;
	float targetValue;

	void Awake ()
	{
		SetActive ( false );
	}

	public void Set (VisualizationParameter param)
	{
		titleText.text = param.name;
		min = param.minValue;
		max = param.maxValue;
		targetValue = param.value;

		indicator.gameObject.SetActive ( !useFill );

		useAcceptableRange = param.useAcceptableRange;

		if ( param.useAcceptableRange )
		{
			minAcceptable = param.minAcceptableValue;
			maxAcceptable = param.maxAcceptableValue;
		}
		suffix = param.suffix;
		UpdateValue ( param.value );
		SetActive ( true );
	}

//	public void Set (string title, float minValue, float maxValue, string unitSuffix)
//	{
//		titleText.text = title;
//		min = minValue;
//		max = maxValue;
//		suffix = unitSuffix;
//		SetActive ( true );
//	}

	public void UpdateValue (float value, int decimals = 1)
	{
		float lerp = Mathf.Clamp01 ( Mathf.InverseLerp ( min, max, value ) );

		if ( useFill )
			bar.fillAmount = lerp;
		else
		{
			bar.fillAmount = 1f;
			indicator.rectTransform.anchorMin = indicator.rectTransform.anchorMax = new Vector2 ( 1f, lerp );
		}

		string format = "F" + decimals;
		valueText.text = value.ToString ( format ) + suffix;

		Image targetImage = useFill ? bar : indicator;

		if ( useAcceptableRange )
		{
			if ( Mathf.Approximately ( value, targetValue ) )
			{
				targetImage.color = Color.green;
			} else
			if ( value >= minAcceptable && value <= maxAcceptable )
			{
				float valueLerp = value < targetValue ?
						Mathf.InverseLerp ( minAcceptable, targetValue, value ) :
						Mathf.InverseLerp ( maxAcceptable, targetValue, value );
				targetImage.color = Color.Lerp ( Color.yellow, Color.green, valueLerp );
			} else
			{
				targetImage.color = Color.red;
			}
		} else
		{
			if ( Mathf.Approximately ( value, targetValue ) )
			{
				targetImage.color = Color.green;
			} else
			{
				targetImage.color = Color.red;
			}
		}
	}

	public void SetActive (bool b)
	{
		gameObject.SetActive ( b );
	}
}