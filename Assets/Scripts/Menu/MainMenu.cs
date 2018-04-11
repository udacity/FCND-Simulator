using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Menu
{
    public class MainMenu : MonoBehaviour
    {
        // project selection index
//        private int projectIndex = 0;
//        public List<Project> projects;
//        public TMPro.TextMeshProUGUI ProjectName;
//        public Image ProjectImage;

        public void Start()
        {
        }

        public void LoadMainMenu()
        {
            Debug.Log("Loading the main menu");
            SceneManager.LoadScene("MainMenu");
        }

		public void LoadProject (Project project)
        {
			Debug.Log ( "Loading project: " + project.title );
			SceneManager.LoadScene ( project.sceneName );
        }
    }
}