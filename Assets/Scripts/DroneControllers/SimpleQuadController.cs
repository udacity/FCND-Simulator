using UnityEngine;
using MovementBehaviors;

namespace DroneControllers
{
    public class SimpleQuadController : MonoBehaviour
    {
        public QuadController controller;
        // public FollowCamera followCam;
        //Vehicle status indicators
        public bool rotors_armed = false;
        //Flight modes
        public bool guided = false;
        public bool stabilized = true;
        public bool posctl = true;
        //Control Gains
        public float Kp_hdot = 10.0f;
        public float Kp_yaw = 6.5f;
        public float Kp_r = 20.0f;
        public float Kp_roll = 6.5f;
        public float Kp_p = 10.0f;
        public float Kp_pitch = 6.5f;
        public float Kp_q = 10.0f;
        public float Kp_pos = 0.1f;
        public float Kp_vel = 0.3f;//-0.05f;
        public float Kd_vel = 0.0f;
        public float Kp_alt = 1.0f;
        public float Ki_hdot = 0.1f;

        //Vehicle control thresholds
        public float posctl_band = 0.1f;
        public float posHoldDeadband = 1.0f;
        public float moveSpeed = 10;
        // in radians
        public float turnSpeed = 2.0f;
        public float thrustForce = 25.0f;
        public float thrustMoment = 2.0f;
        // in radians
        public float maxTilt = 0.5f;
        public float maxAscentRate = 5.0f;
        public float maxDescentRate = 2.0f;

        // Movement behaviors are enabled based on the active control mode.
        // Movement behavior hierachy:
        // - Manual
        //   - Stabilized
        //   - Position Control
        // - Guided
        public QuadMovementBehavior mb_Manual;
        public QuadMovementBehavior mb_ManualPosCtrl;
        public QuadMovementBehavior mb_Guided;

        [System.NonSerialized]
        public Rigidbody rb;
        float tiltX;
        float tiltZ;

        private float h_des = 0.0f;
        [System.NonSerialized]
        public bool pos_set = false;
        [System.NonSerialized]
        public Vector3 posHoldLocal = Vector3.zero;
        [System.NonSerialized]
        public float yawHold = 0.0f;
        [System.NonSerialized]
        public bool yawSet = false;
        Vector3 lastVelocityErrorBody = Vector3.zero;
        float hDotInt = 0.0f;
        public QuadMovementBehavior currentMovementBehavior;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            if (controller == null)
            {
                controller = GetComponent<QuadController>();
            }
            SelectMovementBehavior();
        }

        void LateUpdate()
        {
            if (Input.GetButtonDown("Position Control"))
            {
                posctl = !posctl;
                pos_set = false;
                SelectMovementBehavior();
            }

            if (rotors_armed)
            {
                // if (controlledRemotely) {

                // } else {
                currentMovementBehavior.OnLateUpdate();
                // }
            }
            else
            {
                pos_set = false;
            }
        }

        // Command the quad to a GPS location (latitude, relative_altitude, longitude)
        public void CommandGPS(double latitude, double longitude, double altitude)
        {
            Vector3 localPosition;
            localPosition = controller.GlobalToLocalPosition(longitude, latitude, altitude);
            CommandLocal(localPosition.x, localPosition.y, localPosition.z);
        }

        // Command the quad to a local position (north, east, down)
        public void CommandLocal(float north, float east, float down)
        {

            // The hold position is defined in the Unity reference frame, where (x,y,z)=>(north,up, east) #TODO
            if (guided)
            {
                posHoldLocal.x = east;
                posHoldLocal.y = -down;
                posHoldLocal.z = north;
                pos_set = true;
                // print("LOCAL POSITION COMMAND: " + north + ", " + east + ", " + down);
                // print("LOCAL POSITION: " + controller.GetLocalNorth() + ", " + controller.GetLocalEast());
            }
        }

        public void CommandHeading(float heading)
        {
            yawHold = heading * Mathf.Deg2Rad;
            yawSet = true;
        }

        public void ArmVehicle()
        {
            rotors_armed = true;
            // controller.SetHomePosition(controller.GetLongitude(), controller.GetLatitude(), controller.GetAltitude());
            controller.SetHomePosition(-121.995635d, 37.412939d, 0.0d);
        }

        public void DisarmVehicle()
        {
            rotors_armed = false;
        }

        public void SetGuidedMode(bool input_guided)
        {
            guided = input_guided;
            SelectMovementBehavior();
        }

        // Use this when any control variables change
        void SelectMovementBehavior()
        {
            if (guided)
            {
                currentMovementBehavior = mb_Guided;
            }
            else // manual
            {
                if (posctl)
                {
                    currentMovementBehavior = mb_ManualPosCtrl;
                }
                else
                {
                    currentMovementBehavior = mb_Manual;
                }
            }
            currentMovementBehavior.OnSelect(this);
        }
    }
}