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

		public void OnClicked ()
		{
			transform.root.GetComponent<MainMenu> ().LoadProject ( this );
		}
    }
}