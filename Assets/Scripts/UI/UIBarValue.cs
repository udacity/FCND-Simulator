using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ttext = TMPro.TextMeshProUGUI;

public class UIBarValue : MonoBehaviour
{
	public Image barFill;
	public RectTransform indicator;
	public Ttext valueText;
	public bool vertical;
	public bool displayAsPercent;
	public int precision = 1;

	string format = "F1";

	void Awake ()
	{
		format = "F" + precision;
	}

	public void SetValue (float value)
	{
		if ( vertical )
			value = Mathf.Clamp01 ( value );
		else
			value = Mathf.Clamp ( value, -1f, 1f );

		if ( displayAsPercent )
			valueText.text = ( value * 100f ).ToString ( format ) + "%";
		else
			valueText.text = value.ToString ( format );

		if ( vertical )
		{
			barFill.fillAmount = value;
			indicator.anchorMin = indicator.anchorMax = new Vector2 ( 0.5f, value );
		} else
		{
			float lerp = Mathf.InverseLerp ( -1f, 1f, value );
			indicator.anchorMin = indicator.anchorMax = new Vector2 ( lerp, 0.5f );
		}
	}
}