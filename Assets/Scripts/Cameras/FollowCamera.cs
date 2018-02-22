using UnityEngine;
using UnityStandardAssets.ImageEffects;
using DroneControllers;
using DroneInterface;

public enum CameraPoseType
{
    XNorm,
    YNorm,
    ZNorm,
    Iso,
    Free
}

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

    public bool autoAlign = false;
    //	public Vector3 forward;
    public CameraPoseType poseType;

    public bool blurRotors = true;

    [HideInInspector]
    public Camera cam;
    bool setRotationFlag;
    Quaternion targetRotation;
    float initialFollowDistance;

    float rmbTime;

    void Awake()
    {
        if (activeCamera == null)
            activeCamera = this;
        initialFollowDistance = followDistance;
        cam = GetComponent<Camera>();
        cam.depthTextureMode |= DepthTextureMode.MotionVectors;
//		target = targetTransform.GetComponent<IDrone> ();
    }

    void Start()
    {
		target = targetTransform.GetComponent<IDrone> ();
    }

    void LateUpdate()
    {
//		if ( setRotationFlag )
//		{
//			setRotationFlag = false;
//			transform.rotation = targetRotation;
//		}

//		transform.position = target.UnityCoords () - transform.forward * followDistance;
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
			float scroll = Input.GetAxis ( "Mouse ScrollWheel" );
			float zoom = -scroll * zoomSpeed;
			followDistance += zoom;
			followDistance = Mathf.Clamp ( followDistance, 1.5f, 20 );
			
			if ( Input.GetMouseButtonDown ( 1 ) )
				rmbTime = Time.time;
			if ( Input.GetMouseButtonUp ( 1 ) && Time.time - rmbTime < 0.1f )
			{
				Vector3 euler = transform.eulerAngles;
				euler.x = 45;
				euler.y = targetTransform.eulerAngles.y;
				transform.eulerAngles = euler;
			}
			
			bool isRMB = Input.GetMouseButton ( 1 );
			if ( isRMB && Time.time - rmbTime > 0.2f )
			{
				float x = Input.GetAxis ( "Mouse X" );
				transform.RotateAround ( target.UnityCoords (), Vector3.up, x * rotateSpeed );
				// transform.Rotate ( Vector3.up * x * rotateSpeed, Space.World );
				float y = Input.GetAxis ( "Mouse Y" );
				transform.RotateAround ( target.UnityCoords (), transform.right, -y * rotateSpeed );
			}
			
			if ( !isRMB )
			{
				Vector3 targetForward = Vector3.ProjectOnPlane ( target.Forward, Vector3.up ).normalized;
				Vector3 myForward = Vector3.ProjectOnPlane ( transform.forward, Vector3.up ).normalized;
				float angle = Vector3.Angle ( targetForward, myForward );
				Quaternion q = Quaternion.FromToRotation ( myForward, targetForward ) * transform.rotation;
				
				angle = Mathf.Max ( angle, 5f );
				transform.rotation = Quaternion.RotateTowards ( transform.rotation, q, angle * 3 * Time.deltaTime );
			}
		}

		transform.position = target.UnityCoords () - transform.forward * followDistance;
    }

//    public void ChangePoseType(CameraPoseType newType)
//    {
//        poseType = newType;
//
//        switch (poseType)
//        {
//            case CameraPoseType.XNorm:
//                targetRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
//                break;
//
//            case CameraPoseType.YNorm:
//                targetRotation = Quaternion.LookRotation(-Vector3.right, Vector3.up);
//                break;
//
//            case CameraPoseType.ZNorm:
//                targetRotation = Quaternion.LookRotation(-Vector3.up, (Vector3.forward - Vector3.right).normalized);
//                break;
//
//            case CameraPoseType.Iso:
//            case CameraPoseType.Free:
//                targetRotation = Quaternion.LookRotation((Vector3.forward - Vector3.right - Vector3.up).normalized, (Vector3.forward - Vector3.right + Vector3.up).normalized);
//                break;
//        }
//
//        setRotationFlag = true;
//    }
}