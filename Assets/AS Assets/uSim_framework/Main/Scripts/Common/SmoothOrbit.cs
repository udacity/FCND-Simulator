using UnityEngine;
using UnityEngine;
using System.Collections;

//[AddComponentMenu("Camera-Control/Mouse drag Orbit with zoom")]
public class SmoothOrbit : MonoBehaviour
{
	public bool followVehicle;
	public float followAngle;
	public bool lockMovement;
	public Transform target;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;
	
	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;
	
	public float distanceMin = .5f;
	public float distanceMax = 15f;
	
	public float smoothTime = 2f;
	
	float rotationYAxis = 0.0f;
	float rotationXAxis = 0.0f;
	
	float velocityX = 0.0f;
	float velocityY = 0.0f;
	
	// Use this for initialization
	void Start()
	{
		Vector3 angles = transform.eulerAngles;
		rotationYAxis = angles.y;
		rotationXAxis =10f;
		distance =15f;
		

	}
	
	void LateUpdate()
	{
		float zoomSpeed = Input.GetAxis("Mouse ScrollWheel");
		if(zoomSpeed < 0f)
			//GameObject.FindObjectOfType<CameraManager>().SetCamMode(CameraManager.CameraModes.CarrierOrbit);
		if (lockMovement)
			return;
		if (target)
		{
			transform.parent = target.transform.root;
			if (Input.GetMouseButton (1) ) {
				velocityX += xSpeed * Input.GetAxis ("Mouse X") * 0.03f;
				velocityY += ySpeed * Input.GetAxis ("Mouse Y") * 0.03f;
				followVehicle = false;
			} else {

				followVehicle = true;

			}
			if (GetComponent<CarCamera> () == null)
				followVehicle = false;
			if (!followVehicle) {
				
				rotationYAxis += velocityX;
				rotationXAxis -= velocityY;
				rotationXAxis = ClampAngle (rotationXAxis, yMinLimit, yMaxLimit);
			} else {

				rotationYAxis = Mathf.LerpAngle (rotationYAxis, target.eulerAngles.y, Time.deltaTime * 5f);
				rotationXAxis = Mathf.LerpAngle (rotationXAxis, target.eulerAngles.x + followAngle, Time.deltaTime * 5f);
				
			}
			distance -= (zoomSpeed * 200f) * Time.deltaTime * 5f;
			distance = Mathf.Clamp(distance,distanceMin,distanceMax);
						
			Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
			Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
			Quaternion rotation = toRotation;
			
			
			
			
			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + target.position;
			
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 5f);
			transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 5f);
			
			velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
			velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
		}
		
	}
	
	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}
}
