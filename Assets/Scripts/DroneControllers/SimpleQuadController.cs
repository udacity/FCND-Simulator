using UnityEngine;
using MovementBehaviors;

namespace DroneControllers
{
    public class SimpleQuadController : MonoBehaviour
    {
        public QuadController controller;
        public bool armed = false;
        public bool guided = false;
        public bool attitudeControl = true;
        public bool positionControl = true;
        public bool remote = false;

        public float hDotInt;

        ///
        /// Control Gains
        ///

        public float Kp_hdot = 10.0f;
        public float Kp_yaw = 6.5f;
        public float Kp_r = 20.0f;
        public float Kp_roll = 6.5f;
        public float Kp_p = 10.0f;
        public float Kp_pitch = 6.5f;
        public float Kp_q = 10.0f;
        public float Kp_pos = 2.0f;
        public float Kp_pos2 = 0.4f; //Different gain used for small error (within posHoldDeadband)
        public float Kp_vel = 0.3f;
        public float Kd_vel = 0.0f;
        public float Kp_alt = 10.0f;
        public float Ki_hdot = 0.1f;

        // Vehicle control thresholds
        public float posctl_band = 0.1f;
        public float posHoldDeadband = 1.0f;
        public float velDeadband = 1.0f;
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
        public QuadMovementBehavior mb_ManualAttCtrl;
        public QuadMovementBehavior mb_GuidedPosCtrl;
        public QuadMovementBehavior mb_GuidedAttCtrl;
        public QuadMovementBehavior mb_GuidedMotors;
        

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
        public Vector4 guidedCommand = Vector4.zero;
        [System.NonSerialized]
        public float yawHold = 0.0f;
        [System.NonSerialized]
        public bool yawSet = false;
        Vector3 lastVelocityErrorBody = Vector3.zero;
        public QuadMovementBehavior currentMovementBehavior;

        public AttitudeControl attCtrl = new AttitudeControl();
        public PositionControl posCtrl = new PositionControl();


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
                positionControl = !positionControl;
                if(positionControl)
                    posHoldLocal = new Vector3(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());

            }
            SelectMovementBehavior();

            if (armed)
            {
                currentMovementBehavior.OnLateUpdate();
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
                positionControl = true;
                attitudeControl = false;

                guidedCommand.x = north;
                guidedCommand.y = east;
                guidedCommand.z = down;
                
                // print("LOCAL POSITION COMMAND: " + north + ", " + east + ", " + down);
                // print("LOCAL POSITION: " + controller.GetLocalNorth() + ", " + controller.GetLocalEast());
            }
        }

        public void CommandHeading(float heading)
        {
            guidedCommand.w = heading;
        }

        public void CommandAttitude(float roll,float pitch,float yawRate,float thrust)
        {

            positionControl = false;
            
            guidedCommand.x = roll;
            guidedCommand.y = pitch;
            guidedCommand.w = yawRate;
            guidedCommand.z = thrust;
            attitudeControl = true;
        }

        public void CommandMotors(float rollMoment, float pitchMoment, float yawMoment, float thrust)
        {
            positionControl = false;
            attitudeControl = false;
            guidedCommand.x = rollMoment;
            guidedCommand.y = pitchMoment;
            guidedCommand.w = yawMoment;
            guidedCommand.z = thrust;
        }
        public void ArmVehicle()
        {
            
            // controller.SetHomePosition(controller.GetLongitude(), controller.GetLatitude(), controller.GetAltitude());
            controller.SetHomePosition(-121.995635d, 37.412939d, 0.0d);

            if (guided)
            {
                guidedCommand.x = controller.GetLocalNorth();
                guidedCommand.y = controller.GetLocalEast();
                guidedCommand.z = controller.GetLocalDown();
            }
            else
            {
                posHoldLocal = new Vector3(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());
            }

            //Set the hold position to the current position
            
            armed = true;
        }

        

        public void DisarmVehicle()
        {
            armed = false;
        }

        public void SetGuidedMode(bool input_guided)
        {
            if (!input_guided)
            {
                posHoldLocal = new Vector3(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());
            }

            guided = input_guided;
            
            SelectMovementBehavior();
        }

        // Use this when any control variables change
        void SelectMovementBehavior()
        {
            if (guided)
            {
                if (positionControl)
                {
                    currentMovementBehavior = mb_GuidedPosCtrl;
                }else if (attitudeControl)
                {
                    
                    currentMovementBehavior = mb_GuidedAttCtrl;
                }
                else
                {
                    currentMovementBehavior = mb_GuidedMotors;
                }
                
            }
            else // manual
            {
                if (positionControl)
                {
                    currentMovementBehavior = mb_ManualPosCtrl;
                }
                else if (attitudeControl)
                {
                    currentMovementBehavior = mb_ManualAttCtrl;
                }else
                {
                    currentMovementBehavior = mb_Manual;
                }
            }
            currentMovementBehavior.OnSelect(this);
        }
    }
}