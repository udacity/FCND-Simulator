using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Menu
{
    // TODO: better name for this
    public class Exit : MonoBehaviour
    {
        private GameObject controlsOverlay = null;

        void Start()
        {
            var go = GameObject.Find("GCS");
            var count = go.transform.childCount;
            for (var i = 0; i < count; i++)
            {
                var name = go.transform.GetChild(i).gameObject.name;
                if (name == "ControlsMenuOverlay")
                {
                    controlsOverlay = go.transform.GetChild(i).gameObject;
                    break;
                }
            }
        }

        void LateUpdate()
        {
            // Return to the main menu
			if ( Input.GetButtonDown ( "Back/Exit" ) )
			{
				Debug.Log ( "Loading main menu" );
				SceneManager.LoadScene ( "MainMenu" );
			}
			else if ( Input.GetButton ( "Shift Modifier" ) )
            {
                // Reload current scene
				if ( Input.GetButtonDown ( "Reload/Reset" ) )
				{
					Debug.Log ( "Resetting scene" );
//					string curName = SceneManager.GetActiveScene ().name;

					var mapScript = GameObject.Find ( "Map" ).GetComponent<Mapbox.Unity.Map.AbstractMap> ();
//					Destroy ( mapScript.gameObject );
					mapScript.Reset ();
//					SceneReloader.Reload ();
//					StartCoroutine ( DoReload () );


//					SceneManager.LoadScene ( SceneManager.GetActiveScene ().name );
				}
                // NOTE: not the same as loading the menu, which is done via
                // the main menu
				else if (controlsOverlay != null && Input.GetButtonDown ( "Controls Menu" ))
                {
                    Debug.Log("Toggling control menu overlay");
                    var active = controlsOverlay.activeSelf;
                    controlsOverlay.SetActive(!active);
                }
            }
        }

		IEnumerator DoReload ()
		{
			yield return new WaitForSeconds ( 0.5f );
			SceneReloader.Reload ();
		}

    }
}