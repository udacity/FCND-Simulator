using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DD = TMPro.TMP_Dropdown;

namespace ENUTest
{
	enum CoordSpace
	{
		Unity,
		ENU,
		Other
	}

	public class ENUTest : MonoBehaviour
	{
		public Transform arrow;
		public Transform box;

		public DD dropdown;
		public Slider xSlider;
		public Slider ySlider;
		public Slider zSlider;

		CoordSpace coords;

		void OnEnable ()
		{
			coords = (CoordSpace) dropdown.value;
		}

		public void OnCoordSystemChanged (int index)
		{
			coords = (CoordSpace) dropdown.value;
			xSlider.value = ySlider.value = zSlider.value = 0;
		}

		public void OnSliderValueChanged ()
		{
			Vector3 pos = new Vector3 ( xSlider.value, ySlider.value, zSlider.value );
			if ( coords == CoordSpace.ENU )
				pos = pos.UnityToENUDirection ();
//			Vector3 pos = Vector3.zero;
//			switch ( coords )
//			{
//			case CoordSpace.Unity:
//				pos = new Vector3 ( xSlider.value, ySlider.value, zSlider.value );
//				break;
//
//			case CoordSpace.ENU:
//				pos = new Vector3 ( xSlider.value, zSlider.value, ySlider.value );
//				break;
//
//			case CoordSpace.Other:
//				break;
//			}

			box.position = pos;
		}

		public void OnCenterButton (Slider s)
		{
			s.value = 0;
		}
	}
}