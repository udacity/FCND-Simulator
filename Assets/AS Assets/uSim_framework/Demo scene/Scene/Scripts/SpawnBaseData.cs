using UnityEngine;
using System.Collections;

public class SpawnBaseData : MonoBehaviour {

	public bool showBase;
	public int ownerId;
	public int locationIndex;
	public string idName;
	public string baseDescription;
	public Texture2D baseIcon;
	public SpawnData[] spawnPoints;

	void OnGUI (){
		if (showBase) {
			GUISkin skin = GUI.skin;
			GUI.skin = VSF_Unet_DemoMain.main.mapSkin;
			Vector3 baseMapPos = VSF_Unet_DemoMain.main.mapCamera.WorldToScreenPoint (transform.position);
			GUI.Label (new Rect (baseMapPos.x + 20f, Screen.height - baseMapPos.y - 20f, 150f, 35f), baseDescription);
			if (VSF_Unet_DemoMain.main.playerEntity.hasAuthority && GUI.Button (new Rect (baseMapPos.x, Screen.height - baseMapPos.y, 35f, 35f), baseIcon)) {
				VSF_Unet_DemoMain.main.HideLocationsIcons ();
				VSF_Unet_DemoMain.main.FillVehicles (ownerId);
				VSF_Unet_DemoMain.main.SetLocation (locationIndex);
				showBase = false;
				StartCoroutine (WaitAndSetVehicle ());
			
			}
			GUI.skin = skin;
		}
	}

	IEnumerator WaitAndSetVehicle(){

		yield return new WaitForEndOfFrame ();

		VSF_Unet_DemoMain.main.SetSelectedVehicle (0);

		VSF_Unet_DemoMain.main.SetSpawnPoint (0);

	}
}
