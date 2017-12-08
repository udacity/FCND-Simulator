using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPText = TMPro.TextMeshProUGUI;

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
	public int CurrentState { get { return curState; } }

	Button button;
	TMPText buttonTitle;
	TMPText buttonStatus;
	int curState;

	void OnEnable ()
	{
		if ( resetOnEnable )
			curState = 0;
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
//			curState = 0;
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

		button.targetGraphic.color = states [ curState ].color;
		buttonTitle.text = states [ curState ].title;
		buttonTitle.color = states [ curState ].textColor;
		buttonStatus.text = states [ curState ].status;
		buttonStatus.color = states [ curState ].textColor;
	}

	public void OnClick ()
	{
		curState = ++curState % states.Length;
		UpdateState ();
	}
}