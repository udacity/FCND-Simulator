using UnityEngine;
using MovementBehaviors;
using DroneVehicles;
using DroneSensors;
using DroneInterface;
using UdaciPlot;

namespace DroneControllers
{
    public class QuadAutopilot : MonoBehaviour, IDroneController
    {
//		[System.NonSerialized]
        public QuadVehicle quadVehicle;
        public QuadSensors quadSensor;
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
            if (quadVehicle == null)
            {
                quadVehicle = GetComponent<QuadVehicle>();
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
                CommandPosition(quadVehicle.LocalCoords()); 
            }
                
            if (Input.GetButtonDown("Position Control"))
            {
                //Only switch modes if not controlling via the computer
                if (!guided)
                {
                    positionControl = !positionControl;
                    if (positionControl)
                    {
                        Vector3 LocalPosition = quadVehicle.LocalCoords();
                        posHoldLocal = new Vector3(LocalPosition.x, LocalPosition.y, LocalPosition.z);
                    }
                }

            }
            SelectMovementBehavior();

            if (quadVehicle.MotorsArmed())
            {
                currentMovementBehavior.OnLateUpdate();
            }
            else
            {
                pos_set = false;
            }
//			
        }

        /*
        // Command the quad to a GPS location (latitude, relative_altitude, longitude)
        public void CommandGPS(double latitude, double longitude, double altitude)
        {
            Vector3 localPosition;
            localPosition = FlightUtils.Conversions.GlobalToLocalCoords(longitude, latitude, altitude, quadSensor.HomeLongitude(), quadSensor.HomeLatitude());
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

                // attitudeTarget.z = 0.0f;

                guidedCommand.x = north;
                guidedCommand.y = east;
                guidedCommand.z = down;

                // print("LOCAL POSITION COMMAND: " + north + ", " + east + ", " + down);
                // print("LOCAL POSITION: " + controller.GetLocalNorth() + ", " + controller.GetLocalEast());
            }
        }

        d
        public void CommandHeading(float heading)
        {
            attitudeTarget.z = heading;
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
            if (guided)
            {
                guidedCommand = quadVehicle.LocalCoords();
                //guidedCommand.x = controller.GetLocalNorth();
                //guidedCommand.y = controller.GetLocalEast();
                //guidedCommand.z = controller.GetLocalDown();

                positionTarget.x = guidedCommand.x;
                positionTarget.y = guidedCommand.y;
                positionTarget.z = guidedCommand.z;
            }
            else
            {
                posHoldLocal = quadVehicle.LocalCoords();
                //posHoldLocal = new Vector3(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());
            }
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
                //posHoldLocal = new Vector3(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());
                posHoldLocal = quadVehicle.LocalCoords();
            }

            guided = input_guided;

            SelectMovementBehavior();
        }*/

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
            //currentMovementBehavior.OnSelect(this);
        }

        //Helper functions used
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

        //Functions requried as part of the IDroneController Interface
        /// <summary>
        /// Returns true if the vehicle is being controlled from outside the simulator
        /// </summary>
        public bool OffboardMode()
        {
            return guided;
        }

        /// <summary>
        /// Enables/disables offboard control
        /// </summary>
        /// <param name="offboard">true=enable offboard, false=disable offboard</param>
        public void SetOffboard(bool offboard)
        {
            if (!offboard)
            {
                //posHoldLocal = new Vector3(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());
                posHoldLocal = quadVehicle.LocalCoords();
            }

            guided = offboard;

            SelectMovementBehavior();
        }

        /// <summary>
        /// Command the vehicle to hover at the current position and altitude
        /// </summary>
        public void CommandHover()
        {
            if (!guided)
                return;

            CommandPosition(quadVehicle.LocalCoords());
        }

        /// <summary>
        /// Command the vehicle to the altitude, the position/attitude target does not change
        /// </summary>
        /// <param name="altitude">Altitude in m</param>
        public void CommandAltitude(float altitude)
        {
            if (!guided)
                return;
            if (!positionControl)
                return;

            guidedCommand.z = -altitude;
            positionTarget.z = guidedCommand.z;
            
        }

        /// <summary>
        /// Command the vehicle position. If in Offboard, changes the vehicle control to PositionControl
        /// </summary>
        /// <param name="localPosition">Target local NED position</param>
        public void CommandPosition(Vector3 localPosition)
        {
            if (!guided)
                return;

            positionControl = true;
            attitudeControl = false;

            guidedCommand.x = positionTarget.x = localPosition.x;
            guidedCommand.y = positionTarget.y = localPosition.y;
            guidedCommand.z = positionTarget.z = localPosition.z;

            guidedCommand.w = attitudeTarget.z = quadVehicle.AttitudeEuler().z;
        }

        /// <summary>
        /// If in PositionControl or VelocityControl mode, command the vehicle heading to the specified
        /// </summary>
        /// <param name="heading">Target vehicle heading in radians</param>
        public void CommandHeading(float heading)
        {
            if (!guided)
                return;

            guidedCommand.w = heading;
            attitudeTarget.z = guidedCommand.w;
        }

        /// <summary>
        /// Command the vehicle local velocity. If in Offboard, changes the vehicle control VelocityControl
        /// </summary>
        /// <param name="localVelocity">Target local NED velocity in m/s</param>
        public void CommandVelocity(Vector3 localVelocity)
        {
            if (!guided)
                return;

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Command the vehicle's attitude and thrust
        /// </summary>
        /// <param name="attitude">Euler angles (roll, pitch, yaw) in radians (RH 3-2-1 from world to body)</param>
        /// <param name="thrust">The total commanded thrust from all motors</param>
        public void CommandAttitude(Vector3 attitude, float thrust)
        {
            if (!guided)
                return;

            positionControl = false;

            guidedCommand.x = attitudeTarget.x = attitude.x;
            guidedCommand.y = attitudeTarget.y = attitude.y;
            guidedCommand.w = bodyRateTarget.z = attitude.z;
            guidedCommand.z = momentThrustTarget.w = thrust;

            attitudeControl = true;
        }

        /// <summary>
        /// Command the vehicle's body rates and thrust
        /// </summary>
        /// <param name="bodyrates">Body frame angular rates (p, q, r) in radians/s</param>
        /// <param name="thrust">The total commanded thrust from all motors</param>
        public void CommandAttitudeRate(Vector3 bodyrates, float thrust)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Command the vehicle's body moment and thrust
        /// </summary>
        /// <param name="bodyMoment">Body frame moments in kg*m^2/s^2</param>
        /// <param name="thrust"></param>
        public void CommandMoment(Vector3 bodyMoment, float thrust)
        {
            if (!guided)
                return;

            positionControl = false;
            attitudeControl = false;

            guidedCommand.x = momentThrustTarget.x = bodyMoment.x;
            guidedCommand.y = momentThrustTarget.y = bodyMoment.y;
            guidedCommand.w = momentThrustTarget.z = bodyMoment.z;
            guidedCommand.z = momentThrustTarget.w = thrust;
            lastControlTime = Time.time;
        }

        /// <summary>
        /// Sets the value of the position target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void LocalPositionTarget(Vector3 v)
        {
            positionTarget.x = v.x;
            positionTarget.y = v.y;
            positionTarget.z = v.z;
        }

        /// <summary>
        /// Sets the value of the velocity target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void LocalVelocityTarget(Vector3 v)
        {
            velocityTarget.x = v.x;
            velocityTarget.y = v.y;
            velocityTarget.z = v.z;
        }

        /// <summary>
        /// Sets the value of the acceleration target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void LocalAccelerationTarget(Vector3 v)
        {
            accelerationTarget.x = v.x;
            accelerationTarget.y = v.y;
            accelerationTarget.z = v.z;
        }

        /// <summary>
        /// Sets the value of the attitude target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void AttitudeTarget(Vector3 v)
        {
            attitudeTarget.x = v.x;
            attitudeTarget.y = v.y;
            attitudeTarget.z = v.z;
        }

        /// <summary>
        /// Sets the value of the body rate target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void BodyRateTarget(Vector3 v)
        {
            bodyRateTarget.x = v.x;
            bodyRateTarget.y = v.y;
            bodyRateTarget.z = v.z;
        }


    }
}