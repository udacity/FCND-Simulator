using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Menu
{
    // TODO: better name for this
    public class Exit : MonoBehaviour
    {
        void LateUpdate()
        {
            // Return to the main menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Loading main menu");
                SceneManager.LoadScene("MainMenu");
            }
            // Reload current scene
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Debug.Log("Resetting scene");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
            // TODO: Toggle controls menu
            // NOTE: not the same as loading the menu, which is done via
            // the main menu
            else if ("a" == "b")
            {
                Debug.Log("Toggling control menu");
            }
        }

    }
}