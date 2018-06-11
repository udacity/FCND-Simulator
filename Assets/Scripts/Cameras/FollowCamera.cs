using UnityEngine;
using UnityStandardAssets.ImageEffects;
using DroneControllers;
using DroneInterface;

public class FollowCamera : MonoBehaviour
{
    public static FollowCamera activeCamera;
	public Transform targetTransform;
	public UsimVehicle target;
//    public QuadController target;
    public CameraMotionBlur blurScript;
    public float followDistance = 5;
    public float height = 4;
    public float zoomSpeed = 4;
    public float rotateSpeed = 2;

    public Vector2[] zoomLevels;
	public int initialZoomLevel;
	public int curZoomLevel;
	public float transitionDuration = 0.5f;

    public bool blurRotors = true;
	public CameraLookMode lookMode = CameraLookMode.Follow;

    public bool sideCam = false;

    [HideInInspector]
    public Camera cam;
    bool setRotationFlag;
    Quaternion targetRotation;
    float initialFollowDistance;

    float rmbTime;
	bool isInTransition;
	float transitionTime;
	int lastZoomLevel;
	float lastAngle;

    void Awake()
    {
        if (activeCamera == null)
            activeCamera = this;
        initialFollowDistance = followDistance;
        cam = GetComponent<Camera>();
        cam.depthTextureMode |= DepthTextureMode.MotionVectors;
		curZoomLevel = initialZoomLevel;
		followDistance = zoomLevels [ curZoomLevel ].x;
//		target = targetTransform.GetComponent<IDrone> ();
    }

    void Start()
    {
		target = targetTransform.GetComponent<UsimVehicle> ();
		ResetRotation ();
		lastAngle = transform.localEulerAngles.x;
    }

    void LateUpdate()
    {

		if ( !Simulation.UIIsOpen && lookMode == CameraLookMode.Follow )
		{
			if ( isInTransition )
			{
				Vector2 curZoom = zoomLevels [ curZoomLevel ];
				Vector2 lastZoom = zoomLevels [ lastZoomLevel ];
				transitionTime += Time.deltaTime;
				Vector3 euler = transform.localEulerAngles;
				float t = transitionTime / transitionDuration;
				if ( transitionTime >= transitionDuration )
				{
					followDistance = curZoom.x;
					if ( curZoom.y == Mathf.Infinity )
						euler.x = lastAngle;
					else
						euler.x = curZoom.y;
					isInTransition = false;
					transform.position = target.transform.position - transform.forward * followDistance;
					return;

				} else
				{
					followDistance = Mathf.SmoothStep ( lastZoom.x, curZoom.x, t );
					if ( curZoom.y == Mathf.Infinity )
					{
						if ( lastZoom.y != Mathf.Infinity )
							euler.x = Mathf.SmoothStep ( lastZoom.y, lastAngle, t );
						
					} else
					{
						if ( lastZoom.y == Mathf.Infinity )
							euler.x = Mathf.SmoothStep ( lastAngle, curZoom.y, t );
						else
							euler.x = Mathf.SmoothStep ( lastZoom.y, curZoom.y, t );
					}
					transform.localEulerAngles = euler;
				}
			} else
			{
				float scroll = Input.GetAxis ( "Mouse ScrollWheel" );
				if ( scroll == 0 )
				{
					scroll = Input.GetAxis ( "Camera Zoom" );
				}
				if ( scroll != 0 )
				{
					transitionTime = 0;
					lastZoomLevel = curZoomLevel;
					if ( scroll > 0 && curZoomLevel > 0 )
						curZoomLevel--;
					else
					if ( scroll < 0 && curZoomLevel < zoomLevels.Length - 1 )
						curZoomLevel++;

					if ( curZoomLevel != lastZoomLevel )
					{
						isInTransition = true;
						if ( zoomLevels [ lastZoomLevel ].y == Mathf.Infinity && zoomLevels [ curZoomLevel ].y != Mathf.Infinity )
							lastAngle = transform.localEulerAngles.x;
					}
				}
			}
			
			if ( Input.GetMouseButtonDown ( 1 ) )
				rmbTime = Time.time;
			if ( Input.GetMouseButtonUp ( 1 ) && Time.time - rmbTime < 0.1f && zoomLevels [ curZoomLevel ].y == Mathf.Infinity )
			{
				ResetRotation ();
			}

			// check for mouse pan
			bool isRMB = Input.GetMouseButton ( 1 );
			if ( isRMB && Time.time - rmbTime > 0.2f )
			{
				float x = Input.GetAxis ( "Mouse X" );
				transform.RotateAround (target.transform.position, Vector3.up, x * rotateSpeed );
				if ( zoomLevels[curZoomLevel].y == Mathf.Infinity )
				{
					float y = Input.GetAxis ( "Mouse Y" );
					transform.RotateAround (target.transform.position, transform.right, -y * rotateSpeed );
				}
			}
			// check for keyboard pan
			float tilt = Input.GetAxis ( "Camera Tilt" );
			float yaw = Input.GetAxis ( "Camera Yaw" );
			if ( tilt != 0 && zoomLevels [ curZoomLevel ].y == Mathf.Infinity )
				transform.RotateAround (target.transform.position, transform.right, tilt * 180 * Time.deltaTime );
			if ( yaw != 0 )
				transform.RotateAround (target.transform.position, Vector3.up, yaw * 180 * Time.deltaTime );
			
		}

		switch ( lookMode )
		{
		case CameraLookMode.Left:
			transform.position = targetTransform.position - targetTransform.right * followDistance;
			transform.rotation = Quaternion.LookRotation ( targetTransform.right, Vector3.up );
			break;

		case CameraLookMode.Right:
			transform.position = targetTransform.position + targetTransform.right * followDistance;
			transform.rotation = Quaternion.LookRotation ( -targetTransform.right, Vector3.up );
			break;

		case CameraLookMode.Top:
			transform.position = targetTransform.position + Vector3.up * followDistance;
			transform.rotation = Quaternion.LookRotation ( Vector3.down, Vector3.ProjectOnPlane ( targetTransform.forward, Vector3.up ) );
			break;

		case CameraLookMode.Follow:
                transform.position = targetTransform.position - transform.forward * followDistance;
                transform.rotation = Quaternion.LookRotation(targetTransform.forward, Vector3.up);

			break;
		}

/*        if (sideCam)
        {
            transform.position = target.transform.position - -1.0f * target.transform.right * followDistance;

            var tempRotation = target.transform.rotation.eulerAngles;

            transform.rotation = Quaternion.Euler(0.0f, tempRotation.y - 90.0f, 0.0f);
        }
        else
        {
            transform.position = target.transform.position - transform.forward * followDistance;
            var tempRotation = target.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0.0f, tempRotation.y, 0.0f);
        }*/
    }

	void ResetRotation ()
	{
		Vector3 euler = transform.eulerAngles;
		euler.x = 45;
		euler.y = targetTransform.eulerAngles.y;
		transform.eulerAngles = euler;
	}

	public void SetLookMode (CameraLookMode mode, float distance)
	{
		lookMode = mode;
		followDistance = distance;
	}
}