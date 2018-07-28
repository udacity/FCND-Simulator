using UnityEngine;
using MovementBehaviors;
using DroneVehicles;
using DroneSensors;
using DroneInterface;
using UdaciPlot;

namespace DroneControllers
{

    public class CoraAutopilot : MonoBehaviour, IDroneController
    {
        public CoraVehicle vehicle;
        public CoraSensors sensor;
        public PlaneControl planeControl;
        public QuadControl quadControl;
        public QuadPlaneControl quadPlaneControl;
        public IControlLaw currentControl;
        public IControlLaw control { get { return currentControl; } }
        public bool simpleMode = true;
        public bool guided = false;

        bool inTransition;

        public Vector3 AttitudeTarget { get { return attitudeTarget; } set { attitudeTarget = value; } } //roll, pitch, yaw target in radians
        public Vector3 PositionTarget { get { return positionTarget; } set { positionTarget = value; } }//north, east, down target in meters
        public Vector3 BodyRateTarget { get { return bodyRateTarget; } set { bodyRateTarget = value; } } //p, q, r target in radians/second
        public Vector3 VelocityTarget { get { return velocityTarget; } set { velocityTarget = value; } } //north, east, down, velocity targets in meters/second
        public Vector3 AccelerationTarget { get { return accelerationTarget; } set { accelerationTarget = value; } } //north, east, down acceleration targets in meters/second^2
        public Vector4 MomentThrustTarget { get { return momentThrustTarget; } set { momentThrustTarget = value; } }

        public Vector3 ControlAttitude { get { return AttitudeEuler(); } }
        public Vector3 ControlPosition { get { return PositionLocal(); } }
        public Vector3 ControlBodyRate { get { return AngularRatesBody(); } }
        public Vector3 ControlVelocity { get { return VelocityLocal(); } }
        public Vector3 ControlAcceleration { get { return Vector3.zero; } } // Not implemented yet
        public Vector3 ControlWindData { get { return new Vector3(Airspeed(), 0f, Sideslip()); } } // Airspeed, AoA, Sideslip, AoA not implemented yet
        public float ControlMass { get { return 780; } } //Not yet implemented

        public Vector3 attitudeTarget = Vector3.zero; //roll, pitch, yaw target in radians
        public Vector3 positionTarget = Vector3.zero; //north, east, down target in meters
        public Vector3 bodyRateTarget = Vector3.zero; //p, q, r target in radians/second
        public Vector3 velocityTarget = Vector3.zero; //north, east, down, velocity targets in meters/second
        public Vector3 accelerationTarget = Vector3.zero; //north, east, down acceleration targets in meters/second^2
        public Vector4 momentThrustTarget = Vector4.zero; //body x, y, z moment target (in Newton*meters), thrust target in Newtons

        public MovementBehaviorBase<IDroneController> currentMovementBehavior;
        public PlaneMovementBehavior mb_Manual;
        public PlaneMovementBehavior mb_Longitude;
        public PlaneMovementBehavior mb_Lateral;
        public PlaneMovementBehavior mb_Stablized;
        public PlaneMovementBehavior mb_AscendDescend;
        public PlaneMovementBehavior mb_YawHold;
        public PlaneMovementBehavior mb_LineFollowing;
        public PlaneMovementBehavior mb_OrbitFollowing;
        public PlaneMovementBehavior mb_TransitionToPlane;
        public QuadMovementBehavior mb_TransitionToQuad;
        public QuadMovementBehavior mb_AttitudeControl;
        public QuadMovementBehavior mb_PositionControl;
        public int flightMode;

        enum FLIGHT_MODE : int
        {
            MANUAL = 1,
            LONGITUDE = 2,
            LATERAL = 3,
            STABILIZED = 4,
            ASCENDDESCEND = 5,
            YAWHOLD = 6,
            LINEFOLLOWING = 7,
            ORBITFOLLOWING = 8,
            ATTITUDE = 9,
            POSITION = 10,
            TOPLANE = 11,
            TOQUAD = 12,
        }
        void Awake()
        {
            //planeControl = new PlaneControl();
            quadPlaneControl.QuadControl = quadControl;
            quadPlaneControl.PlaneControl = planeControl;
            //quadControl = new QuadControl();
            flightMode = (int)FLIGHT_MODE.MANUAL;
            SelectMovementBehavior();            
            
        }

		void Start ()
		{

		}



        void LateUpdate()
        {
            if (!guided)
            {
                if (Input.GetKey("t"))
                {
                    if (flightMode < (int)FLIGHT_MODE.ATTITUDE)
                    {
                        Debug.Log("Transition to Quad");
                        flightMode = (int)FLIGHT_MODE.TOQUAD;
                        SelectMovementBehavior();
                    }
                    else if (flightMode < (int)FLIGHT_MODE.TOPLANE)
                    {
                        Debug.Log("Transition to Plane");
                        flightMode = (int)FLIGHT_MODE.TOPLANE;
                        SelectMovementBehavior();
                    }
                    
                }

                if(Input.GetKey("0"))
                {
                    flightMode = (int)FLIGHT_MODE.POSITION;
                    SelectMovementBehavior();
                }
                if (Input.GetKey("9"))
                {
                    flightMode = (int)FLIGHT_MODE.ATTITUDE;
                    SelectMovementBehavior();
                }
                if (Input.GetKey("6"))
                {
                    flightMode = (int)FLIGHT_MODE.YAWHOLD;
                    SelectMovementBehavior();
                }
                if (Input.GetKey("5"))
                {
                    flightMode = (int)FLIGHT_MODE.ASCENDDESCEND;
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
            }

            if (flightMode == (int)FLIGHT_MODE.TOPLANE)
            {
                if (ControlWindData.x > quadPlaneControl.toPlaneAirspeed){
                    flightMode = (int)FLIGHT_MODE.STABILIZED;
                    SelectMovementBehavior();
                }
            }else if(flightMode == (int)FLIGHT_MODE.TOQUAD)
            {
                if(ControlWindData.x < quadPlaneControl.toQuadAirspeed){
                    flightMode = (int)FLIGHT_MODE.POSITION;
                    SelectMovementBehavior();
                }
            }
            if (!vehicle.IsFrozen())
                currentMovementBehavior.OnLateUpdate();

        }

        // Use this when any control variables change
        void SelectMovementBehavior()
        {
            switch (flightMode)
            {
                case (int)FLIGHT_MODE.TOQUAD:
                    currentControl = quadPlaneControl;
                    currentMovementBehavior = mb_TransitionToQuad;
                    Simulation.FixedWingUI.SetControlModeText(1);
                    break;
                case (int)FLIGHT_MODE.TOPLANE:
                    currentControl = quadPlaneControl;
                    currentMovementBehavior = mb_TransitionToPlane;
                    Simulation.FixedWingUI.SetControlModeText(1);
                    break;
                case (int)FLIGHT_MODE.POSITION:
                    currentControl = quadControl;
                    PositionTarget = ControlPosition;
                    CommandControls(0, 0, 0, 0);
                    currentMovementBehavior = mb_PositionControl;
                    Simulation.FixedWingUI.SetControlModeText(0);
                    break;
                case (int)FLIGHT_MODE.ATTITUDE:
                    currentControl = quadControl;
                    CommandControls(0, 0, 0, 0);
                    currentMovementBehavior = mb_AttitudeControl;
                    Simulation.FixedWingUI.SetControlModeText(0);
                    break;
                case (int)FLIGHT_MODE.ORBITFOLLOWING:
                    currentControl = planeControl;
                    CommandMoment(Vector3.zero, 0);
                    currentMovementBehavior = mb_OrbitFollowing;
                    Simulation.FixedWingUI.SetControlModeText(2);
                    break;
                case (int)FLIGHT_MODE.LINEFOLLOWING:
                    currentControl = planeControl;
                    CommandMoment(Vector3.zero, 0);
                    currentMovementBehavior = mb_LineFollowing;
                    Simulation.FixedWingUI.SetControlModeText(2);
                    break;
                case (int)FLIGHT_MODE.YAWHOLD:
                    currentControl = planeControl;
                    CommandMoment(Vector3.zero, 0);
                    currentMovementBehavior = mb_YawHold;
                    Simulation.FixedWingUI.SetControlModeText(2);
                    break;
                case (int)FLIGHT_MODE.ASCENDDESCEND:
                    currentControl = planeControl;
                    CommandMoment(Vector3.zero, 0);
                    currentMovementBehavior = mb_AscendDescend;
                    Simulation.FixedWingUI.SetControlModeText(2);
                    break;
                case (int)FLIGHT_MODE.STABILIZED:
                    currentControl = planeControl;
                    CommandMoment(Vector3.zero, 0);
                    currentMovementBehavior = mb_Stablized;
                    Simulation.FixedWingUI.SetControlModeText(2);
                    break;
                case (int)FLIGHT_MODE.LATERAL:
                    currentControl = planeControl;
                    CommandMoment(Vector3.zero, 0);
                    currentMovementBehavior = mb_Lateral;
                    Simulation.FixedWingUI.SetControlModeText(2);
                    break;
                case (int)FLIGHT_MODE.LONGITUDE:
                    currentControl = planeControl;
                    CommandMoment(Vector3.zero, 0);
                    currentMovementBehavior = mb_Longitude;
                    Simulation.FixedWingUI.SetControlModeText(2);
                    break;
                case (int)FLIGHT_MODE.MANUAL:
                    currentControl = planeControl;
                    CommandMoment(Vector3.zero, 0);
                    currentMovementBehavior = mb_Manual;
                    Simulation.FixedWingUI.SetControlModeText(2);
                    break;
                default:
                    currentControl = planeControl;
                    currentMovementBehavior = mb_Manual;
                    Simulation.FixedWingUI.SetControlModeText(2);
                    break;
            }
            //currentMovementBehavior = mb_Manual;

            currentMovementBehavior.OnSelect(this);
        }

        // Functions to pass along state information to the different controllers (simpleMode = perfect state information)

        public Vector3 AttitudeEuler()
        {
            if (simpleMode)
                return vehicle.AttitudeEuler();
            else
                return sensor.AttitudeEstimate();
        }

        public Vector3 AngularRatesBody()
        {
            if (simpleMode)
                return vehicle.AngularRatesBody();
            else
                return sensor.AngularRateEstimate();
        }

        public Vector3 VelocityLocal()
        {
            if (simpleMode)
                return vehicle.VelocityLocal();
            else
                return sensor.VelocityEstimate();
        }

        public Vector3 PositionLocal()
        {
            if (simpleMode)
                return vehicle.CoordsLocal();
            else
                return sensor.PositionEstimate();
        }

        public float Airspeed()
        {
            return VelocityLocal().magnitude;
        }

        public float Sideslip()
        {
            if (simpleMode)
                if (vehicle.VelocityBody().x > 0.1)
                    return Mathf.Asin(vehicle.VelocityBody().y / vehicle.VelocityBody().magnitude);
                else
                    return 0.0f;
            else
                return sensor.SideslipEstimate();
        }

        public void CommandTorque(Vector3 torque)
        {
            vehicle.CmdTorque(torque);
        }

        public void CommandThrust(float thrust)
        {
            vehicle.CmdThrust(thrust);
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
            momentThrustTarget.w = throttleRPM;
            momentThrustTarget.x = aileron;
            momentThrustTarget.y = elevator;
            momentThrustTarget.z = rudder;
            vehicle.CommandThrottle(throttleRPM);
            vehicle.CommandElevator(elevator);
            vehicle.CommandAileron(aileron);
            vehicle.CommandRudder(rudder);

            //Debug.Log("Controls Command: " + new Vector4(aileron, elevator, rudder, throttleRPM));
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
            if (flightMode != controlMode)
            {
                flightMode = controlMode;
                SelectMovementBehavior();
            }
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
                sensor.SetHomePosition();

                /*
                //Reset the controllers (dumps the integrators)
                attCtrl = new AttitudeControl();
                posCtrl = new PositionControl();
                posHoldLocal = PositionLocal();
                posHoldLocal.z = 0.0f;
                */
            }

            vehicle.ArmDisarm(arm);
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
        /// Command the vehicle along a line.
        /// </summary>
        /// <param name="localPosition">Target local NED position</param>
        public void CommandVector(Vector3 localPosition, Vector3 localVelocity) 
        {
            if (!guided)
                return;
            switch (flightMode)
            {
                case (int)FLIGHT_MODE.LINEFOLLOWING:
                    positionTarget = localPosition;
                    velocityTarget = localVelocity;
                    break;
                case (int)FLIGHT_MODE.ORBITFOLLOWING:
                    positionTarget = localPosition;
                    velocityTarget.x = localVelocity.x;
                    velocityTarget.y = 0f;
                    velocityTarget.z = 0f;
                    bodyRateTarget.z = localVelocity.z;
                    break;
            }


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
                    //Debug.Log("Longitude Mode Command");
                    attitudeTarget.x = attitude.x;
                    attitudeTarget.z = attitude.z;
                    momentThrustTarget.y = attitude.y;
                    momentThrustTarget.w = thrust;
                    break;
                case (int)FLIGHT_MODE.LATERAL:
                    //Debug.Log("Lateral Mode Command");
                    momentThrustTarget.x = attitude.x;
                    momentThrustTarget.z = attitude.z;
                    velocityTarget.x = thrust;
                    positionTarget.z = attitude.y;
                    break;
                case (int)FLIGHT_MODE.STABILIZED:
                    //Debug.Log("Stabilized Mode Command");
                    attitudeTarget.x = attitude.x;
                    attitudeTarget.z = attitude.z;
                    velocityTarget.x = thrust;
                    positionTarget.z = attitude.y;
                    break;
                case (int)FLIGHT_MODE.ASCENDDESCEND:
                    //Debug.Log("Ascend/Descend Mode Command");
                    attitudeTarget.x = attitude.x;
                    attitudeTarget.z = attitude.z;
                    momentThrustTarget.w = thrust;
                    velocityTarget.x = attitude.y;
                    break;
                case (int)FLIGHT_MODE.YAWHOLD:
                    attitudeTarget.z = attitude.x;
                    velocityTarget.y = attitude.z;
                    velocityTarget.x = thrust;
                    positionTarget.z = attitude.y;
                    break;

            }
            //Debug.Log(attitude + " " + thrust);
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
            MomentThrustTarget = new Vector4(bodyMoment.x, bodyMoment.y, bodyMoment.z, thrust);
            vehicle.CmdThrust(thrust);
            vehicle.CmdTorque(bodyMoment);
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

        public Vector3 LocalPositionTarget()
        {
            return positionTarget;
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

        public Vector3 LocalVelocityTarget()
        {
            return velocityTarget;
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

        public Vector3 LocalAccelerationTarget()
        {
            return accelerationTarget;
        }



    }
}