using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TuningParameter
{
	public float MinValue { get { return minValue; } }
	public float MaxValue { get { return maxValue; } }
	public float Value { get { return currentValue; } }
	public string Name { get { return name; } }

	[SerializeField]
	string name = "TuneMe";
	[SerializeField]
	float minValue = 0;
	[SerializeField]
	float maxValue = 1;
	[SerializeField]
	float defaultValue = 0.5f;
	float currentValue;

	public void Reset ()
	{
		currentValue = defaultValue;
	}
}