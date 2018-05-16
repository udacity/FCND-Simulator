using UnityEngine;
using UnityStandardAssets.ImageEffects;
using DroneControllers;
using DroneInterface;

public class FollowCamera : MonoBehaviour
{
    public static FollowCamera activeCamera;
	public Transform targetTransform;
	public IDrone target;
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
		target = targetTransform.GetComponent<IDrone> ();
		ResetRotation ();
		lastAngle = transform.localEulerAngles.x;
    }

    void LateUpdate()
    {
//		if ( setRotationFlag )
//		{
//			setRotationFlag = false;
//			transform.rotation = targetRotation;
//		}

//		transform.position = target.CoordsUnity () - transform.forward * followDistance;
//        if (blurRotors)
//        {
//            float forcePercent = Mathf.Abs(target.Force.y / target.maxForce);
//            blurScript.velocityScale = forcePercent * forcePercent * forcePercent;
//            if (!blurScript.enabled)
//                blurScript.enabled = true;
//        }
//        else
//        {
//            if (blurScript.enabled)
//                blurScript.enabled = false;
//        }

		if ( !Simulation.UIIsOpen )
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
					transform.position = target.CoordsUnity () - transform.forward * followDistance;
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
					
//					float zoom = -scroll * zoomSpeed;
//					followDistance += zoom;
//					followDistance = Mathf.Clamp ( followDistance, zoomLevels [ 0 ].x, zoomLevels [ zoomLevels.Length - 1 ].x );
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
				transform.RotateAround ( target.CoordsUnity (), Vector3.up, x * rotateSpeed );
				if ( zoomLevels[curZoomLevel].y == Mathf.Infinity )
				{
					float y = Input.GetAxis ( "Mouse Y" );
					transform.RotateAround ( target.CoordsUnity (), transform.right, -y * rotateSpeed );
				}
			}
			// check for keyboard pan
			float tilt = Input.GetAxis ( "Camera Tilt" );
			float yaw = Input.GetAxis ( "Camera Yaw" );
			if ( tilt != 0 && zoomLevels [ curZoomLevel ].y == Mathf.Infinity )
				transform.RotateAround ( target.CoordsUnity (), transform.right, tilt * 180 * Time.deltaTime );
			if ( yaw != 0 )
				transform.RotateAround ( target.CoordsUnity (), Vector3.up, yaw * 180 * Time.deltaTime );
			
//			if ( !isRMB )
//			{
//				Vector3 targetForward = Vector3.ProjectOnPlane ( target.Forward, Vector3.up ).normalized;
//				Vector3 myForward = Vector3.ProjectOnPlane ( transform.forward, Vector3.up ).normalized;
//				float angle = Vector3.Angle ( targetForward, myForward );
//				Quaternion q = Quaternion.FromToRotation ( myForward, targetForward ) * transform.rotation;
//				
//				angle = Mathf.Max ( angle, 5f );
//				transform.rotation = Quaternion.RotateTowards ( transform.rotation, q, angle * 3 * Time.deltaTime );
//			}
		}

		transform.position = target.CoordsUnity () - transform.forward * followDistance;
    }

	void ResetRotation ()
	{
		Vector3 euler = transform.eulerAngles;
		euler.x = 45;
		euler.y = targetTransform.eulerAngles.y;
		transform.eulerAngles = euler;
	}
}