using UnityEngine;
using System.Collections;

public class CurvesManager : MonoBehaviour {
	[System.Serializable]
	public class Curves {
			
		public string name;
		public AnimationCurve curve;
	}
	
	string presetName;
	
	public Curves[] curves;

	public AnimationCurve GetCurve (string curvename) {
		
		AnimationCurve returnCurve;
		for (int i = 0; i < curves.Length; i++){
			if (curves[i].name == curvename){
				returnCurve = curves[i].curve;		
				return returnCurve;
			}
		}
		return null;
	}
}
