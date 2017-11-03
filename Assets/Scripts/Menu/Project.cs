using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class Project : MonoBehaviour
    {
        public string sceneName;
        public string title;
        public Sprite image;
        public string description;

        public Project(string sceneName, string title, Sprite image, string description)
        {
            this.sceneName = sceneName;
            this.title = title;
            this.image = image;
            this.description = description;
        }
    }

}