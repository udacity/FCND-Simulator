using UnityEngine;
using MovementBehaviors;
using DroneVehicles;
using DroneSensors;
using DroneInterface;
using UdaciPlot;

namespace DroneControllers
{

    public class PlaneAutopilot : MonoBehaviour, IDroneController
    {
        public PlaneVehicle planeVehicle;
        public PlaneSensors planeSensor;
        public PlaneControl planeControl;
        public bool simpleMode = true;
        public bool guided = false;

        public Vector3 attitudeTarget = Vector3.zero; //roll, pitch, yaw target in radians
        public Vector3 positionTarget = Vector3.zero; //north, east, down target in meters
        public Vector3 bodyRateTarget = Vector3.zero; //p, q, r target in radians/second
        public Vector3 velocityTarget = Vector3.zero; //north, east, down, velocity targets in meters/second
        public Vector3 accelerationTarget = Vector3.zero; //north, east, down acceleration targets in meters/second^2
        public Vector4 momentThrustTarget = Vector4.zero; //body x, y, z moment target (in Newton*meters), thrust target in Newtons

        public PlaneMovementBehavior currentMovementBehavior;
        public PlaneMovementBehavior mb_Manual;
        public PlaneMovementBehavior mb_Longitude;
        public PlaneMovementBehavior mb_Lateral;
        public PlaneMovementBehavior mb_Stablized;
        public PlaneMovementBehavior mb_Waypoint;
        int flightMode;

        enum FLIGHT_MODE : int
        {
            MANUAL = 1,
            LONGITUDE = 2,
            LATERAL = 3,
            STABILIZED = 4,
            WAYPOINT = 5
        }
        void Awake()
        {

            SelectMovementBehavior();
            planeControl = new PlaneControl();
            flightMode = (int)FLIGHT_MODE.MANUAL;
            
        }

		void Start ()
		{

		}



        void LateUpdate()
        {
            if (Input.GetKey("5"))
            {
                flightMode = (int)FLIGHT_MODE.WAYPOINT;
                SelectMovementBehavior();
            }
            if (Input.GetKey("4"))
            {
                flightMode = (int)FLIGHT_MODE.STABILIZED;
                SelectMovementBehavior();
            }
            if (Input.GetKey("3"))
            {
                flightMode = (int)FLIGHT_MODE.LATERAL;
                SelectMovementBehavior();
            }
            if (Input.GetKey("2"))
            {
                flightMode = (int)FLIGHT_MODE.LONGITUDE;
                SelectMovementBehavior();
            }
            if (Input.GetKey("1"))
            {
                flightMode = (int)FLIGHT_MODE.MANUAL;
                SelectMovementBehavior();
            }

            currentMovementBehavior.OnLateUpdate();
            
//			
        }

        // Use this when any control variables change
        void SelectMovementBehavior()
        {
            switch (flightMode)
            {
                case (int)FLIGHT_MODE.WAYPOINT:
                    break;
                case (int)FLIGHT_MODE.STABILIZED:
                    break;
                case (int)FLIGHT_MODE.LATERAL:
                    currentMovementBehavior = mb_Lateral;
                    break;
                case (int)FLIGHT_MODE.LONGITUDE:
                    currentMovementBehavior = mb_Longitude;
                    break;
                case (int)FLIGHT_MODE.MANUAL:
                    currentMovementBehavior = mb_Manual;
                    break;
                default:
                    currentMovementBehavior = mb_Manual;
                    break;
            }
            //currentMovementBehavior = mb_Manual;

            currentMovementBehavior.OnSelect(this);
        }

        // Functions to pass along state information to the different controllers (simpleMode = perfect state information)

        public Vector3 AttitudeEuler()
        {
            if (simpleMode)
                return planeVehicle.AttitudeEuler();
            else
                return planeSensor.AttitudeEstimate();
        }

        public Vector3 AngularRatesBody()
        {
            if (simpleMode)
                return planeVehicle.AngularRatesBody();
            else
                return planeSensor.AngularRateEstimate();
        }

        public Vector3 VelocityLocal()
        {
            if (simpleMode)
                return planeVehicle.VelocityLocal();
            else
                return planeSensor.VelocityEstimate();
        }

        public Vector3 PositionLocal()
        {
            if (simpleMode)
                return planeVehicle.CoordsLocal();
            else
                return planeSensor.PositionEstimate();
        }

        public float Airspeed()
        {
            return VelocityLocal().magnitude;
        }

        public float Sideslip()
        {
            if (simpleMode)
                if (planeVehicle.VelocityBody().x > 0.1)
                    return planeVehicle.VelocityBody().y / planeVehicle.VelocityBody().x;
                else
                    return 0.0f;
            else
                return planeSensor.SideslipEstimate();
        }

        public void CommandTorque(Vector3 torque)
        {

        }

        public void CommandThrust(float thrust)
        {

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

        public void CommandControls(float aileron, float elevator, float rudder, float throttleRPM)
        {
            //Debug.Log("Commanding Controls");
            //if(planeVehicle.MotorsArmed())
            momentThrustTarget.w = throttleRPM;
            momentThrustTarget.x = aileron;
            momentThrustTarget.y = elevator;
            momentThrustTarget.z = rudder;
            planeVehicle.CommandThrottle(throttleRPM);
            planeVehicle.CommandElevator(elevator);
            planeVehicle.CommandAileron(aileron);
            planeVehicle.CommandRudder(rudder);

            Debug.Log("Controls Command: " + new Vector4(aileron, elevator, rudder, throttleRPM));
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
            /*
            if (!guided_in)
            {
                //posHoldLocal = new Vector3(controller.GetLocalNorth(), controller.GetLocalEast(), controller.GetLocalDown());
                posHoldLocal = PositionLocal();
            }
            */
            guided = guided_in;


            SelectMovementBehavior();
        }

        /// <summary>
        /// Used to enable different modes of control (for example stabilized vs position control)
        /// </summary>
        /// <param name="controlMode"></param>
        public void SetControlMode(int controlMode)
        { 
            flightMode = controlMode;
            SelectMovementBehavior();
        }

        /// <summary>
        /// Returns an integer corresponding to the mode of control
        /// </summary>
        /// <returns></returns>
        public int ControlMode()
        {
            return flightMode;
        }

        /// <summary>
        /// Arms/disarms the vehicle motors
        /// </summary>
        /// <param name="arm">true=arm, false=disarm</param>
        public void ArmDisarm(bool arm)
        {
            if (arm)
            {
                planeSensor.SetHomePosition();

                /*
                //Reset the controllers (dumps the integrators)
                attCtrl = new AttitudeControl();
                posCtrl = new PositionControl();
                posHoldLocal = PositionLocal();
                posHoldLocal.z = 0.0f;
                */
            }

            planeVehicle.ArmDisarm(arm);
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
            /*
            if (!guided)
                return;
            if (!positionControl)
                return;

            guidedCommand.z = -altitude;
            positionTarget.z = guidedCommand.z;
            */
            
        }

        /// <summary>
        /// Command the vehicle position. If in guided, changes the vehicle control to PositionControl
        /// </summary>
        /// <param name="localPosition">Target local NED position</param>
        public void CommandPosition(Vector3 localPosition)
        {
            if (!guided)
                return;

            /*
            positionControl = true;
            attitudeControl = false;

            guidedCommand.x = positionTarget.x = localPosition.x;
            guidedCommand.y = positionTarget.y = localPosition.y;
            guidedCommand.z = positionTarget.z = localPosition.z;

            guidedCommand.w = attitudeTarget.z = AttitudeEuler().z;
            */
        }

        /// <summary>
        /// If in PositionControl or VelocityControl mode, command the vehicle heading to the specified
        /// </summary>
        /// <param name="heading">Target vehicle heading in radians</param>
        public void CommandHeading(float heading)
        {
            if (!guided)
                return;
            /*
            guidedCommand.w = heading;
            attitudeTarget.z = guidedCommand.w;
            */
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

            switch (flightMode)
            {
                case (int)FLIGHT_MODE.LONGITUDE:
                    Debug.Log("Longitude Mode Command");
                    attitudeTarget.x = attitude.x;
                    attitudeTarget.z = attitude.z;
                    momentThrustTarget.y = attitude.y;
                    momentThrustTarget.w = thrust;
                    break;
                case (int)FLIGHT_MODE.LATERAL:
                    Debug.Log("Lateral Mode Command");
                    momentThrustTarget.x = attitude.x;
                    momentThrustTarget.z = attitude.z;
                    velocityTarget.x = thrust;
                    positionTarget.z = attitude.y;
                    break;
                case (int)FLIGHT_MODE.STABILIZED:
                    Debug.Log("Stabilized Mode Command");
                    attitudeTarget.x = attitude.x;
                    attitudeTarget.z = attitude.z;
                    velocityTarget.x = thrust;
                    positionTarget.z = attitude.y;
                    break;
            }
            Debug.Log(attitude + " " + thrust);
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

            /*
            positionControl = false;
            attitudeControl = false;

            guidedCommand.x = momentThrustTarget.x = bodyMoment.x;
            guidedCommand.y = momentThrustTarget.y = bodyMoment.y;
            guidedCommand.w = momentThrustTarget.z = bodyMoment.z;
            guidedCommand.z = momentThrustTarget.w = thrust;
            lastControlTime = Time.time;
            */
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