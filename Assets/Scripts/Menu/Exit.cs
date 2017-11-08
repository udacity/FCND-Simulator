using UnityEngine;
using UnityEngine.SceneManagement;

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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Loading main menu");
                SceneManager.LoadScene("MainMenu");
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Reload current scene
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Debug.Log("Resetting scene");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                // NOTE: not the same as loading the menu, which is done via
                // the main menu
                else if (controlsOverlay != null && Input.GetKeyDown(KeyCode.C))
                {
                    Debug.Log("Toggling control menu overlay");
                    var active = controlsOverlay.activeSelf;
                    controlsOverlay.SetActive(!active);
                }
            }
        }

    }
}