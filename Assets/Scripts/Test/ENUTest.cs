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
		public Renderer[] boxAxes;
		public Material[] axisMaterials;

		public DD dropdown;
		public Slider xSlider;
		public Slider ySlider;
		public Slider zSlider;
		public Slider xRotSlider;
		public Slider yRotSlider;
		public Slider zRotSlider;

		CoordSpace coords;

		void OnEnable ()
		{
			coords = (CoordSpace) dropdown.value;
		}

		public void OnCoordSystemChanged (int index)
		{
			coords = (CoordSpace) dropdown.value;
			xSlider.value = ySlider.value = zSlider.value = 0;
			xRotSlider.value = yRotSlider.value = zRotSlider.value = 0;

			boxAxes [ 0 ].sharedMaterial = axisMaterials [ 0 ];
			boxAxes [ 1 ].sharedMaterial = boxAxes [ 1 ].sharedMaterial;

			boxAxes [ 2 ].sharedMaterial = index == 0 ? axisMaterials [ 1 ] : axisMaterials [ 2 ];
			boxAxes [ 3 ].sharedMaterial = boxAxes [ 2 ].sharedMaterial;

			boxAxes [ 4 ].sharedMaterial = index == 0 ? axisMaterials [ 2 ] : axisMaterials [ 1 ];
			boxAxes [ 5 ].sharedMaterial = boxAxes [ 4 ].sharedMaterial;
		}

		public void OnPositionSliderChanged ()
		{
			Vector3 pos = new Vector3 ( xSlider.value, ySlider.value, zSlider.value );
			if ( coords == CoordSpace.ENU )
				pos = pos.UnityToENUDirection ();

			box.position = pos;
		}

		public void OnRotationSliderChanged ()
		{
			Vector3 euler = new Vector3 ( xRotSlider.value, yRotSlider.value, zRotSlider.value );
			if ( coords == CoordSpace.ENU )
				euler = euler.UnityToENURotation ();

			box.eulerAngles = euler;
		}

		public void OnCenterButton (Slider s)
		{
			s.value = 0;
		}
	}
}