using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage (AttributeTargets.Field)]
public class Tunable : PropertyAttribute
{
	public float defaultValue;
	public float minValue;
	public float maxValue;

	public Tunable (float _defaultValue, float _minValue, float _maxValue)
	{
		if ( _minValue > _maxValue )
		{
			float temp = _minValue;
			_minValue = _maxValue;
			_maxValue = temp;
		}
		_defaultValue = Mathf.Clamp ( _defaultValue, _minValue, _maxValue );

		defaultValue = _defaultValue;
		minValue = _minValue;
		maxValue = _maxValue;
	}
}