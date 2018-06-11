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
	public RuntimeTunableParameter parameter;

	float defaultValue;

	public void OnSliderChanged ()
	{
		curValue.text = slider.value.ToString ( "F2" );
	}

	public void OnValueChanged ()
	{
		slider.value = float.Parse ( curValue.text );
	}

	public void Set (string pName, float value, float min, float max, RuntimeTunableParameter parm)
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
		parameter.field.SetValue ( parameter.fieldInstance, GetValue () );
	}

	public void ResetToDefault ()
	{
		curValue.text = defaultValue.ToString ( "F2" );
		slider.value = defaultValue;
	}
}