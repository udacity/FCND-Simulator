using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScenarioData
{
	public float runtime = 60;
	public bool canSucceedBeforeRuntime;
	public bool canFailBeforeRuntime;

	public Vector3 vehiclePosition;
	public Quaternion vehicleOrientation;
	public Vector3 vehicleEulerAngles;
	public Vector3 vehicleVelocity;
	public CameraLookMode cameraLookMode;
	public float cameraDistance;
    public float cameraAngle;
	public string title;
	[Multiline]
	public string description;
	[Multiline]
	public string successText;
	[Multiline]
	public string failText;
}