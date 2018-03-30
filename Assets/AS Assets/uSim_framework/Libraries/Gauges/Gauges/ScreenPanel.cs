using UnityEngine;
using System.Collections;

public class ScreenPanel : MonoBehaviour {

	public bool show;
	public Texture2D panelImage;
	public Vector2 backgroundSize;
	public float offsetY;
	public float offsetX;
	public float heightAdd;
	public Rect panelRect;
	public float panelScale;
	// Use this for initialization
	void Start () {
	
		panelRect = new Rect ((Screen.width/2) + offsetX, (Screen.width/2) + offsetY, backgroundSize.x * panelScale, backgroundSize.y * panelScale+ heightAdd);

	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyUp (KeyCode.PageUp)) {

			offsetY += 10f;

		}
		if (Input.GetKeyUp (KeyCode.PageDown)) {
			
			offsetY -= 10f;
			
		}


	}

	void OnGUI(){
		if (show) {
			if(panelImage == null) return;
			panelRect = new Rect ((Screen.width/2) + offsetX, (Screen.width/2) + offsetY, backgroundSize.x * panelScale, backgroundSize.y * panelScale+ heightAdd);
			GUI.depth = 10;
			GUI.DrawTexture (panelRect, panelImage);
		}
	}
}
