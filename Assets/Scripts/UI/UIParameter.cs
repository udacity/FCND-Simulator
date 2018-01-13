using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using input = TMPro.TMP_InputField;
using ttext = TMPro.TextMeshProUGUI;

public class UIParameter : MonoBehaviour
{
	public ttext title;
	public input valueText;

	[System.NonSerialized]
	SimParameter parameter;

	public void Init (SimParameter param, bool observe = false)
	{
		parameter = param;
		title.text = name = param.displayName;
		valueText.text = param.Value.ToString ();
		if ( observe )
			parameter.Observe ( OnParamChanged );
	}

	public void OnValueChanged (string newValue)
	{
//		Debug.Log ( "value changed on " + name );
		float val = 0;
		if ( float.TryParse ( newValue, out val ) && parameter != null )
			parameter.Value = val;
	}

	void OnParamChanged (SimParameter p)
	{
//		Debug.Log ( "parameter value changed on " + name );
		valueText.text = p.Value.ToString ();
	}
}