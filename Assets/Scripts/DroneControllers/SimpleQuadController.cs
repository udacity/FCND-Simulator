using UnityEngine;
using MovementBehaviors;
using UdaciPlot;

namespace DroneControllers
{
    public class SimpleQuadController : MonoBehaviour
    {
//		[System.NonSerialized]
        public QuadController controller;
        public bool armed = false;
        public bool guided = false;
        public bool attitudeControl = true;
        public bool positionControl = true;
        public bool remote = false;

        public float hDotInt;

        ///
        /// Default control Gains are no found in PositionControl.cs and AttitudeControl.cs
        ///

        public float posctl_band = 0.1f;
        private float lastControlTime = 0.0f;
        public float maxTimeBetweenControl = 0.1f;
        // Movement behaviors are enabled based on the active control mode.
        // Movement behavior hierachy:
        // - Manual
        //   - Attitude Control
        //   - Position Control
        // - Guided
        //   - Attitude Control
        //   - Position Control
        //   - Motor Control (Currently Moments)
        public QuadMovementBehavior mb_Manual;
        public QuadMovementBehavior mb_ManualPosCtrl;
        public QuadMovementBehavior mb_ManualAttCtrl;
        public QuadMovementBehavior mb_GuidedPosCtrl;
        public QuadMovementBehavior mb_GuidedAttCtrl;
        public QuadMovementBehavior mb_GuidedMotors;


        public Vector3 attitudeTarget = Vector3.zero; //roll, pitch, yaw target in radians
        public Vector3 positionTarget = Vector3.zero; //north, east, down target in meters
        public Vector3 bodyRateTarget = Vector3.zero; //p, q, r target in radians/second
        public Vector3 velocityTarget = Vector3.zero; //north, east, down, velocity targets in meters/second
        public Vector3 accelerationTarget = Vector3.zero; //north, east, down acceleration targets in meters/second^2
        public Vector4 momentThrustTarget = Vector4.zero; //body x, y, z moment target (in Newton*meters), thrust target in Newstons

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
		bool alive;


        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            if (controller == null)
            {
                controller = GetComponent<QuadController>();
            }
            SelectMovementBehavior();
			alive = true;
        }

		void Start ()
		{

		}



        void LateUpdate()
        {
            if (!attitudeControl&&!positionControl&&((Time.time - lastControlTime) > maxTimeBetweenControl))
            {
                CommandLocal(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());
            }
                
            if (Input.GetButtonDown("Position Control"))
            {
                positionControl = !positionControl;
                if (positionControl)
                {
                    posHoldLocal = new Vector3(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());
                }

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
//			
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

                positionTarget.x = north;
                positionTarget.y = east;
                positionTarget.z = down;

                attitudeTarget.z = 0.0f;

                guidedCommand.x = north;
                guidedCommand.y = east;
                guidedCommand.z = down;

                // print("LOCAL POSITION COMMAND: " + north + ", " + east + ", " + down);
                // print("LOCAL POSITION: " + controller.GetLocalNorth() + ", " + controller.GetLocalEast());
            }
        }

        public void CommandHeading(float heading)
        {
            attitudeTarget.z = heading;
            guidedCommand.w = heading;
        }

        public void CommandAttitude(float roll, float pitch, float yawRate, float thrust)
        {

            positionControl = false;


            attitudeTarget.x = roll;
            attitudeTarget.y = pitch;
            bodyRateTarget.z = yawRate;
            momentThrustTarget.w = thrust;

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

            momentThrustTarget.x = rollMoment;
            momentThrustTarget.y = pitchMoment;
            momentThrustTarget.z = yawMoment;
            momentThrustTarget.w = thrust;
            guidedCommand.x = rollMoment;
            guidedCommand.y = pitchMoment;
            guidedCommand.w = yawMoment;
            guidedCommand.z = thrust;
            lastControlTime = Time.time;
        }
        public void ArmVehicle()
        {
            // controller.SetHomePosition(controller.GetLongitude(), controller.GetLatitude(), controller.GetAltitude());
            if (guided)
            {
                guidedCommand.x = controller.GetLocalNorth();
                guidedCommand.y = controller.GetLocalEast();
                guidedCommand.z = controller.GetLocalDown();

                positionTarget.x = guidedCommand.x;
                positionTarget.y = guidedCommand.y;
                positionTarget.z = guidedCommand.z;
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
                }
                else if (attitudeControl)
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
                }
                else
                {
                    currentMovementBehavior = mb_Manual;
                }
            }
            currentMovementBehavior.OnSelect(this);
        }

        public Vector3 GetPositionTarget()
        {
            return new Vector3(positionTarget.x, positionTarget.y, positionTarget.z);
        }


        public Vector3 GetVelocityTarget()
        {
            return new Vector3(velocityTarget.x, velocityTarget.y, velocityTarget.z);
        }

        public Vector3 GetAccelerationTarget()
        {
            return new Vector3(accelerationTarget.x, accelerationTarget.y, accelerationTarget.y);
        }

        public Vector3 GetAttitudeTarget()
        {
            return new Vector3(attitudeTarget.x, attitudeTarget.y, attitudeTarget.z);
        }

        public Vector3 GetBodyRateTarget()
        {
            return new Vector3(bodyRateTarget.x, bodyRateTarget.y, bodyRateTarget.z);
        }

        public Vector3 GetMomentTarget()
        {
            return new Vector3(momentThrustTarget.x, momentThrustTarget.y, momentThrustTarget.z);
        }

        public float GetThrustTarget()
        {
            return momentThrustTarget.w;
        }
    }
}