using UnityEngine;
using UnityStandardAssets.ImageEffects;
using DroneControllers;

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
    public QuadController target;
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
    }

    void Start()
    {
    }

    void LateUpdate()
    {
        if (setRotationFlag)
        {
            setRotationFlag = false;
            transform.rotation = targetRotation;
        }

        transform.position = target.Position - transform.forward * followDistance;
        if (blurRotors)
        {
            float forcePercent = Mathf.Abs(target.Force.y / target.maxForce);
            blurScript.velocityScale = forcePercent * forcePercent * forcePercent;
            if (!blurScript.enabled)
                blurScript.enabled = true;
        }
        else
        {
            if (blurScript.enabled)
                blurScript.enabled = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float zoom = -scroll * zoomSpeed;
        followDistance += zoom;
        followDistance = Mathf.Clamp(followDistance, 1.5f, 20);

        if (Input.GetMouseButtonDown(1))
            rmbTime = Time.time;
        if (Input.GetMouseButtonUp(1) && Time.time - rmbTime < 0.1f)
        {
            Vector3 forward = target.forward.forward;
            Vector3 right = target.right.right;
            transform.rotation = Quaternion.LookRotation(forward - right - Vector3.up, forward - right + Vector3.up);
        }

        if (Input.GetMouseButton(1) && Time.time - rmbTime > 0.2f)
        {
            float x = Input.GetAxis("Mouse X");
            transform.RotateAround(target.Position, Vector3.up, x * rotateSpeed);
            // transform.Rotate ( Vector3.up * x * rotateSpeed, Space.World );
            float y = Input.GetAxis("Mouse Y");
            transform.RotateAround(target.Position, transform.right, -y * rotateSpeed);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Alpha5))
        {
            var pose = 0;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                pose = 1;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                pose = 2;
            if (Input.GetKeyDown(KeyCode.Alpha4))
                pose = 3;
            if (Input.GetKeyDown(KeyCode.Alpha5))
                pose = 4;
        }
    }

    public void ChangePoseType(CameraPoseType newType)
    {
        poseType = newType;

        switch (poseType)
        {
            case CameraPoseType.XNorm:
                targetRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                break;

            case CameraPoseType.YNorm:
                targetRotation = Quaternion.LookRotation(-Vector3.right, Vector3.up);
                break;

            case CameraPoseType.ZNorm:
                targetRotation = Quaternion.LookRotation(-Vector3.up, (Vector3.forward - Vector3.right).normalized);
                break;

            case CameraPoseType.Iso:
            case CameraPoseType.Free:
                targetRotation = Quaternion.LookRotation((Vector3.forward - Vector3.right - Vector3.up).normalized, (Vector3.forward - Vector3.right + Vector3.up).normalized);
                break;
        }

        setRotationFlag = true;
    }
}