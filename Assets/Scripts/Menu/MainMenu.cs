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
        private int projectIndex;
        public List<Project> projects;
        public Text ProjectName;
        public Image ProjectImage;

        public void Start()
        {
            projectIndex = 0;
            var p = projects[projectIndex];
            ProjectName.text = p.title;
            ProjectImage.sprite = p.image;
        }

        public void LoadControlMenu()
        {
            SceneManager.LoadScene("ControlMenu");
        }

        public void LoadMainMenu()
        {
            Debug.Log("Go to main menu");
            SceneManager.LoadScene("MenuScene");
        }

        public void SelectMode()
        {
            var p = projects[projectIndex];
            SceneManager.LoadScene(p.sceneName);
        }

        public void Next()
        {
            projectIndex = (projectIndex + 1) % projects.Count;
            var p = projects[projectIndex];
            ProjectName.text = p.title;
            ProjectImage.sprite = p.image;
        }

        public void Previous()
        {
            if (projectIndex == 0)
            {
                projectIndex = projects.Count - 1;
            }
            else
            {
                projectIndex = (projectIndex - 1) % projects.Count;
            }
            projectIndex = (projectIndex + 1) % projects.Count;
            var p = projects[projectIndex];
            ProjectName.text = p.title;
            ProjectImage.sprite = p.image;
        }
    }
}