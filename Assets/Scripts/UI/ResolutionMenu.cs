using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DD = TMPro.TMP_Dropdown;
using DDText = TMPro.TextMeshProUGUI;

public class ResolutionMenu : MonoBehaviour
{
	public DDText title;
	public DD dropdown;
	public Button fullscreen;

	public Sprite maxSprite;
	public Sprite minSprite;

	Resolution[] resolutions;

	void Start ()
	{
		#if UNITY_EDITOR
		resolutions = new Resolution[1];
		Resolution r = new Resolution ();
		r.width = Screen.width;
		r.height = Screen.height;
		r.refreshRate = 60;
		resolutions[0] = r;
		dropdown.ClearOptions ();
		dropdown.AddOptions (new List<string> (new string[] { Screen.width + " x " + Screen.height }));
		#else
		List<Resolution> uniqueResolutions = new List<Resolution> ();
		resolutions = Screen.resolutions;
		foreach ( Resolution r in resolutions )
		{
			int index = uniqueResolutions.FindIndex ( x => x.width == r.width && x.height == r.height );
			if ( index != -1 )
			{
				if ( r.refreshRate > uniqueResolutions [ index ].refreshRate )
					uniqueResolutions [ index ] = r;
			}
			else
				uniqueResolutions.Add ( r );
		}
		uniqueResolutions.Reverse ();
		resolutions = uniqueResolutions.ToArray ();
		List<string> names = new List<string> ( resolutions.Length );
		for ( int i = 0; i < resolutions.Length; i++ )
			names.Add ( resolutions [ i ].width + " x " + resolutions[i].height );

		dropdown.ClearOptions ();
		dropdown.AddOptions ( names );

		int curIndex = resolutions.FindIndex ( x => x.width == Screen.width && x.height == Screen.height );
		if ( curIndex == -1 )
		{
			Debug.Log ( "current resolution not found: " + Screen.width + "x" + Screen.height );
		} else
			dropdown.value = curIndex;
		#endif
		dropdown.RefreshShownValue ();

		fullscreen.image.sprite = Screen.fullScreen ? minSprite : maxSprite;
		#if !UNITY_EDITOR
		if ( Application.platform == RuntimePlatform.WebGLPlayer )
			fullscreen.gameObject.SetActive ( false );
		#endif
	}

	public void OnDropdownSelection (int value)
	{
		Resolution r = resolutions [ value ];
		Screen.SetResolution ( r.width, r.height, Screen.fullScreen, r.refreshRate );
		fullscreen.image.sprite = Screen.fullScreen ? minSprite : maxSprite;
	}

	public void OnFullscreenButton ()
	{
		fullscreen.image.sprite = !Screen.fullScreen ? minSprite : maxSprite;
		Screen.fullScreen = !Screen.fullScreen;
	}

	public void SetVisible (bool visible = true)
	{
		fullscreen.enabled = dropdown.enabled = title.enabled = visible;
	}
}