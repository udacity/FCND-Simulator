using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Ttext = TMPro.TextMeshProUGUI;

public class UILocationOption : MonoBehaviour
{
	public Ttext labelText;
	public Image image;
	public Sprite sprite;
	public string title = "Some Location";
	public string latLongCoord = "37.792480,-122.397450";

	LocationSelectUI selectionScript;

	void OnEnable ()
	{
		selectionScript = gameObject.GetComponentInParent<LocationSelectUI> ();
	}

	void Awake ()
	{
		labelText.text = title;
		image.sprite = sprite;
	}

	public void OnClick ()
	{
		selectionScript.LocationSelected ( this );
	}
}