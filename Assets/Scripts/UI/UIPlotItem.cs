using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ttext = TMPro.TextMeshProUGUI;

public class UIPlotItem : MonoBehaviour
{
	public string Label { get { return label.text; } }
	public bool IsOn { get { return toggle.isOn; } }
	public Color Color { get { return label.color; } }
	public Ttext label;
	public Toggle toggle;

	public Action<UIPlotItem, bool> callback;

	public void Init (string text, Action<UIPlotItem, bool> _callback)
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
			callback ( this, on );
	}
}