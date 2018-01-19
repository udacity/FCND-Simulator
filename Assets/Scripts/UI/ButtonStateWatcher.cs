using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPText = TMPro.TextMeshProUGUI;

public enum _State {On, Off}

[System.Serializable]
public class ButtonState
{
	public string title;
	public string status;
	public Color color;
	public Color textColor;
}


[ExecuteInEditMode]
public class ButtonStateWatcher : MonoBehaviour, ISerializationCallbackReceiver
{
	public bool resetOnEnable;

	public ButtonState[] states;
	// public Dictionary<_State, ButtonState> statesMap;
	// public int CurrentState { get { return curState; } }

	/// <summary>
	/// Whether the button is active or not
	/// </summary>
	public bool active = false;

	Button button;
	TMPText buttonTitle;
	TMPText buttonStatus;

	void OnEnable ()
	{
		if ( resetOnEnable )
		{
			active = false;
		}
		UpdateState ();
	}

	void OnDisable ()
	{
		button = null;
	}

	public void OnBeforeSerialize ()
	{
		
	}

	public void OnAfterDeserialize ()
	{
//		if ( resetOnEnable )
//			active = false;
//		UpdateState ();
	}

	void UpdateState ()
	{
		if ( button == null )
		{
			button = GetComponent<Button> ();
			var texts = button.GetComponentsInChildren<TMPText> ();
			buttonTitle = texts [ 0 ];
			buttonStatus = texts [ 1 ];
		}

		var curState = 0;
		if (active) {
			curState = 1;
		}
		button.targetGraphic.color = states [ curState ].color;
		buttonTitle.text = states [ curState ].title;
		buttonTitle.color = states [ curState ].textColor;
		buttonStatus.text = states [ curState ].status;
		buttonStatus.color = states [ curState ].textColor;
	}

	public void OnClick ()
	{
		active = !active;
		UpdateState ();
	}
}