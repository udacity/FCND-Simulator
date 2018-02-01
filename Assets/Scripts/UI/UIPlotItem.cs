using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ttext = TMPro.TextMeshProUGUI;

public class UIPlotItem : MonoBehaviour
{
	public Ttext label;
	public Toggle toggle;

	public Action<string, bool> callback;

	public void Init (string text, Action<string, bool> _callback)
	{
		toggle.isOn = false;
		label.text = text;
		callback = _callback;
		gameObject.SetActive ( true );
	}

	public void SetColor (Color c)
	{
		label.color = c;
	}

	public void OnToggle (bool on)
	{
		if ( callback != null )
			callback ( label.text, on );
	}
}