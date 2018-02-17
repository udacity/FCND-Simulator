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

	public Toggle[] sizeButtons;

	public Sprite maxSprite;
	public Sprite minSprite;

	Resolution[] resolutions;

	Resolution maxResolution;
	int curSizeButton;

	void Start ()
	{
		// new: up to 5 size buttons instead of a whole giant dropdown
		int count = Screen.resolutions.Length;
		maxResolution = Screen.resolutions [ count - 1 ];

		resolutions = new Resolution[5];
		Resolution r = new Resolution ();
		r.width = 640;
		r.height = 480;
		r.refreshRate = 60;
		resolutions [ 0 ] = r;
		r.width = 800;
		r.height = 600;
		resolutions [ 1 ] = r;
		r.width = 1024;
		r.height = 768;
		resolutions [ 2 ] = r;
		r.width = 2048;
		r.height = 1536;
		resolutions [ 3 ] = r;
		r.width = maxResolution.width;
		r.height = maxResolution.height;
		resolutions [ 4 ] = r;

		int curIndex = resolutions.FindIndex ( x => x.width == Screen.width && x.height == Screen.height );
		if ( curIndex == 4 && Screen.fullScreen )
		{
			curSizeButton = 4;
			Debug.Log ( "fullscreen ok on 4" );
		} else
		if ( curIndex == -1 || curIndex == 4 )
		{
			for ( int i = 0; i < 4; i++ )
			{
				if ( resolutions [ i + 1 ].width > Screen.width || resolutions [ i + 1 ].height > Screen.height )
				{
					curSizeButton = i;
					Debug.Log ( "best resolution " + i );
					break;
				}
			}
		} else
		{
			curSizeButton = curIndex;
			Debug.Log ( "already on " + curSizeButton );
		}
		sizeButtons [ curSizeButton ].isOn = true;
		GetComponentInChildren<ToggleGroup> ().NotifyToggleOn ( sizeButtons [ curSizeButton ] );
//		Screen.SetResolution ( 640, 480, false, 60 );

		sizeButtons [ 3 ].gameObject.SetActive ( maxResolution.height > 2048 );
		sizeButtons [ 2 ].gameObject.SetActive ( maxResolution.height > 1024 );
//		sizeButtons [ 3 ].interactable = maxResolution.height > 2048;
//		sizeButtons [ 2 ].interactable = maxResolution.height > 1024;

		// lock resolution to one of the acceptable

/*		#if UNITY_EDITOR
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
		#endif*/
	}

	public void OnDropdownSelection (int value)
	{
		Resolution r = resolutions [ value ];
		Screen.SetResolution ( r.width, r.height, Screen.fullScreen, r.refreshRate );
		fullscreen.image.sprite = Screen.fullScreen ? minSprite : maxSprite;
	}

	public void OnResolutionButton (int value)
	{
		if ( sizeButtons [ value ].isOn && value != curSizeButton )
		{
			curSizeButton = value;
			switch ( value )
			{
			case 0:
				Screen.SetResolution ( 640, 480, false, 60 );
				Debug.Log ( "Setting resolution to 640x480" );
				break;

			case 1:
				Screen.SetResolution ( 800, 600, false, 60 );
				Debug.Log ( "Setting resolution to 800x600" );
				break;

			case 2:
				Screen.SetResolution ( 1024, 768, false, 60 );
				Debug.Log ( "Setting resolution to 1024x768" );
				break;

			case 3:
				Screen.SetResolution ( 2048, 1536, false, 60 );
				Debug.Log ( "Setting resolution to 2048x1536" );
				break;

			case 4:
				Screen.SetResolution ( maxResolution.width, maxResolution.height, true, 60 );
				Debug.Log ( "Setting resolution to " + maxResolution.width + "x" + maxResolution.height );
				break;
			}
		}
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