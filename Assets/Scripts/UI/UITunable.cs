using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TText = TMPro.TextMeshProUGUI;
using TInput = TMPro.TMP_InputField;

public class UITunable : MonoBehaviour
{
	public TText paramName;
	public Slider slider;
	public TInput curValue;
	public TText minValue;
	public TText maxValue;

	[System.NonSerialized]
	public TunableParameter parameter;

	float defaultValue;

	public void OnSliderChanged ()
	{
		curValue.text = slider.value.ToString ( "F2" );
	}

	public void OnValueChanged ()
	{
		slider.value = float.Parse ( curValue.text );
	}

	public void Set (TunableParameter param)
	{
		paramName.text = param.name;
		curValue.text = param.value.ToString ( "F2" );
		minValue.text = param.minValue.ToString ( "F2" );
		maxValue.text = param.maxValue.ToString ( "F2" );
		slider.minValue = param.minValue;
		slider.maxValue = param.maxValue;
		slider.value = param.value;
		defaultValue = param.value;
		parameter = param;
	}

	public void Set (string pName, float value, float min, float max, TunableParameter parm)
	{
		paramName.text = pName;
		curValue.text = value.ToString ( "F2" );
		minValue.text = min.ToString ( "F2" );
		maxValue.text = max.ToString ( "F2" );
		slider.minValue = min;
		slider.maxValue = max;
		slider.value = value;
		defaultValue = value;
		parameter = parm;
	}

	public float GetValue ()
	{
		return float.Parse ( curValue.text );
	}

	public void ApplyValue ()
	{
		parameter.value = GetValue ();
	}

	public void RestoreValue ()
	{
		float value = parameter.value;
		curValue.text = value.ToString ( "F2" );
		slider.value = value;
	}

	public void ResetToDefault ()
	{
		curValue.text = defaultValue.ToString ( "F2" );
		slider.value = defaultValue;
	}
}