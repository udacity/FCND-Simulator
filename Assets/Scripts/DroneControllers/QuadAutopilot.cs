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
        public bool guided = false;
        public bool onGround = false;
        public bool simpleMode = true;
        public bool attitudeControl = true;
        public bool positionControl = true;


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
        //float tiltX;
        //float tiltZ;

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
            if (quadVehicle == null)
            {
                quadVehicle = GetComponent<QuadVehicle>();
            }
            SelectMovementBehavior();
        }

		void Start ()
		{

		}



        void LateUpdate()
        {

            if (!attitudeControl&&!positionControl&&((Time.time - lastControlTime) > maxTimeBetweenControl))
            {
                CommandPosition(PositionLocal()); 
            }
                
            if (Input.GetButtonDown("Position Control"))
            {
                //Only switch modes if not controlling via the computer
                if (!guided)
                {
                    positionControl = !positionControl;
                    if (positionControl)
                    {
                        Vector3 LocalPosition = PositionLocal();
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

        // Functions to pass along state information to the different controllers (simpleMode = perfect state information)

        public Vector3 AttitudeEuler()
        {
            if (simpleMode)
                return quadVehicle.AttitudeEuler();
            else
                return quadSensor.AttitudeEstimate();
        }

        public Vector3 AngularRatesBody()
        {
            if (simpleMode)
                return quadVehicle.AngularRatesBody();
            else
                return quadSensor.AngularRateEstimate();
        }

        public Vector3 VelocityLocal()
        {
            if (simpleMode)
                return quadVehicle.VelocityLocal();
            else
                return quadSensor.VelocityEstimate();
        }

        public Vector3 PositionLocal()
        {
            if (simpleMode)
                return quadSensor.PositionEstimate(); //quadVehicle.CoordsLocal();
            else
                return quadSensor.PositionEstimate();
        }

        public void CommandTorque(Vector3 torque)
        {
            quadVehicle.CmdTorque(torque);
        }

        public void CommandThrust(float thrust)
        {
            quadVehicle.CmdThrust(thrust);
        }

        // Helper functions used for plotting
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
        public bool Guided()
        {
            return guided;
        }

        /// <summary>
        /// Enables/disables guided control from outside Unity
        /// </summary>
        /// <param name="offboard">true=enable guided, false=disable guided</param>
        public void SetGuided(bool guided_in)
        {
            if (!guided_in)
            {
                //posHoldLocal = new Vector3(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());
                posHoldLocal = PositionLocal();
            }

            guided = guided_in;

            SelectMovementBehavior();
        }

        /// <summary>
        /// Arms/disarms the vehicle motors
        /// </summary>
        /// <param name="arm">true=arm, false=disarm</param>
        public void ArmDisarm(bool arm)
        {
            if (arm)
            {
                quadSensor.SetHomePosition();
                //Reset the controllers (dumps the integrators)
                attCtrl = new AttitudeControl();
                posCtrl = new PositionControl();
                posHoldLocal = PositionLocal();
                posHoldLocal.z = 0.0f;
                Debug.Log(posHoldLocal);
            }

            quadVehicle.ArmDisarm(arm);
        }

        /// <summary>
        /// Command the vehicle to hover at the current position and altitude
        /// </summary>
        public void CommandHover()
        {
            CommandPosition(PositionLocal());
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
        /// Command the vehicle position. If in guided, changes the vehicle control to PositionControl
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

            guidedCommand.w = attitudeTarget.z = AttitudeEuler().z;
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