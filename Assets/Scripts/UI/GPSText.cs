// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using TMPro;


// public class GPSText : MonoBehaviour {

// 	// private QuadController quadController;
//     // private GameObject droneObj;
// // 

//     public static GPSText instance;

//     [System.NonSerialized]
//     public static TMPro.TextMeshPro text;

//     void Awake () {
//         instance = this;
//         droneObj = GameObject.Find("Quad Drone");
// 		quadController = droneObj.GetComponent<QuadController>();
// 	}

// 	void LateUpdate () {
//         var lat = quadController.getLatitude();
//         var lon = quadController.getLongitude();
//         var alt = quadController.getAltitude();
// 		gpsText.text = string.Format("Latitude = {0:0.000}\nLongitude = {1:0.000}\nAltitude = {2:0.000} (meters)", lat, lon, alt);
//         // _gpsText.color = new Color(255, 255, 255, 0);

// 	}
// }