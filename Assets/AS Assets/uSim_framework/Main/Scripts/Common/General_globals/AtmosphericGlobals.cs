using UnityEngine;
using System.Collections;

public class AtmosphericGlobals : MonoBehaviour {

	public bool fixedAtmosphere;
	public float fixedDensity;
	public AnimationCurve atmosphereDensity;
	public float dynamicDensity;


	public float GetDensity (float altitude ) {

		return 	atmosphereDensity.Evaluate(altitude / 10000f);

	}

}
