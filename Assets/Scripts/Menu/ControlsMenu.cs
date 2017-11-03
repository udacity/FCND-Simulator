using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class ControlsMenu : MonoBehaviour
    {
        public void Start()
        {
        }

        public void LoadMainMenu()
        {
            Debug.Log("Loading to main menu");
            SceneManager.LoadScene("MenuScene");
        }
    }
}