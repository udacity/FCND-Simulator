using UnityEngine;
using FlightUtils;
using DroneInterface;


namespace DroneVehicles
{

    public class CoraVehicle : MonoBehaviour, IDroneVehicle
    {
        int status;
        public int Status
        {
            get { return status; }
            set { status = value; }
        }
		public bool Frozen
		{
			get { return rb.isKinematic; }
			set { rb.isKinematic = value; }
		}

        //Two objects used from the uSim library
        public InputsManager inputsManager;
        public AircraftControl aircraftControl;

        public SimpleProp prop;
		public MoreRotors rotors;
        Vector3 positionUnity;
        Vector3 eulerAngles;
        Vector3 localVelocity;
        Vector3 bodyVelocity;
        Vector3 localAcceleration;
        Vector3 bodyAcceleration;
        Vector3 localAngularVelocity;
        Vector3 bodyAngularVelocity;
        float curSpeed;
        float flightTime;
        Rigidbody rb;

        bool motorsArmed;

        public bool useGravity = true;

        public float aileron = 0.0f;
        public float elevator = 0.0f;
        public float rudder = 0.0f;
        public float throttleRPM = 0.0f;
        public float throttleAlpha = 0.1f;

        public float maxAileron = 30.0f;
        public float minAileron = -30.0f;

        
        public float maxRudder = 30.0f;
        public float minRudder = -30.0f;

        
        public float maxElevator = 30.0f;
        public float minElevator = -30.0f;

        public float elevatorTrim = 0.0f;
        public float maxTrim = 30.0f;
        public float minTrim = -30.0f;

        // Parameters for the lift fan outputs
        public float maxThrust = 20000;
        public Vector3 force;
        public float thrustOut;
        public float thrustNoise = 0.0f;
        public float thrustAlpha;

        public float maxTorque = 30000f;
        public float maxPitchMoment = 7500f;
        public float maxRollMoment = 2800f;
        float maxYawMoment = 2000f;
        public Vector3 torque;
        public float torqueNoise = 0.0f;
        public float torqueOut;

        public float energy;
        float thrustEnergyRate = -0.005f;
        float propEnergyRate = -0.000625f;

        Matrix4x4 rotorMat;
        

        public float maxThrottleRPM = 5000.0f;

        void Awake()
        {
            float l1 = 1.7f;
            float l2 = 3.1f;
            float l3 = 4.4f;
            float l4 = 1.25f;
            maxPitchMoment = 6 * maxThrust / 12 * l4;
            maxRollMoment = 2 * maxThrust / 12 * (l1 + l2 + l3);
            rotorMat[0, 0] = rotorMat[0, 2] = maxThrust / 12 * (l1 + l2 + l3);
            rotorMat[0, 1] = rotorMat[0, 3]  = - rotorMat[0, 0];
            rotorMat[1, 0] = rotorMat[1, 1] = 3 * l4*maxThrust / 12;
            rotorMat[1, 2] = rotorMat[1, 3] = -rotorMat[1, 0];
            rotorMat[2, 0] = rotorMat[2,3] = maxYawMoment / 12;
            rotorMat[2, 1] = rotorMat[2, 2] = -maxYawMoment / 12;
            rotorMat[3, 0] = rotorMat[3, 1] = rotorMat[3, 2] = rotorMat[3, 3] = 3*maxThrust/12;

            if (rb == null)
                rb = GetComponent<Rigidbody>();
            rb.useGravity = useGravity;

            if (inputsManager == null)
                inputsManager = rb.GetComponent<InputsManager>();

            if (prop == null)
                prop = rb.GetComponent<SimpleProp>();

            if (aircraftControl == null)
                aircraftControl = rb.GetComponent<AircraftControl>();

            flightTime = 0.0f;

        }

        void Start()
        {
        }

        private void OnDestroy()
        {
        }

        void Update()
        {
            
        }

        void LateUpdate()
        {
            
        }

        void FixedUpdate()
        {

            inputsManager.SetAileron(aileron);
            inputsManager.SetRudder(rudder);
            inputsManager.SetElevator(elevator);
            prop.SetRPM(throttleRPM);
			rotors.SetRPM ( thrustOut );
            ApplyForceTorque();

            UIUpdate();
            StateUpdate();
            TimeUpdate();
        }

        void UIUpdate()
        {
            Simulation.FixedWingUI.throttle.SetValue(throttleRPM/maxThrottleRPM);
            Simulation.FixedWingUI.elevator.SetValue(elevator);
            Simulation.FixedWingUI.rudder.SetValue(rudder);
            Simulation.FixedWingUI.aileron.SetValue(aileron);

            Simulation.FixedWingUI.thrust.SetValue(force.y / maxThrust);
            Simulation.FixedWingUI.roll.SetValue(-torque.z / maxRollMoment);
            Simulation.FixedWingUI.pitch.SetValue(-torque.x / maxPitchMoment);
            Simulation.FixedWingUI.yaw.SetValue(torque.y / maxYawMoment);
        }

        public void CommandAileron(float a)
        {
            aileron = a;
        }

        public void CommandElevator(float e)
        {
            elevator = e;
        }

        public void CommandRudder(float r)
        {
            rudder = r;
        }

        public void AddRudder(float r)
        {
            rudder = rudder + r;
        }

        public void CommandThrottle(float t)
        {
            if (t > 1.0f)
                t = 1.0f;
            else if (t < 0.0f)
                t = 0.0f;

            throttleRPM = t*maxThrottleRPM;
        }

        public void AddTrim(float t)
        {
            elevatorTrim = elevatorTrim + t;
        }

        public void FreezeDrone(bool freeze)
        {
            if (freeze)
            {
                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = false;
            }
        }

        public bool IsFrozen()
        {
            return rb.isKinematic;
        }

        /// <summary>
        /// Ensure Euler angles are in the range [-180, 180].
        /// </summary>
        Vector3 ConstrainEuler(Vector3 euler)
        {
            euler.x = ConstrainAngle(euler.x);
            euler.y = ConstrainAngle(euler.y);
            euler.z = ConstrainAngle(euler.z);
            return euler;
        }


        /// <summary>
        /// Ensure angle is in the range [-180, 180].
        /// </summary>
        float ConstrainAngle(float angle)
        {
            if (angle > 180f)
            {
                angle -= 360f;
            }
            if (angle < -180f)
            {
                angle += 360f;
            }
            return angle;
        }

        /// Convenience retrieval functions. These probably should be set as properties
        /// These functions convert all the local class variables, which are defined in Unity Left-Handed coordinate frames
        /// to the appropriate right handed coordinate frame
        /// 

        public Vector3 CoordsUnity()
        {
            //StateUpdate();
            return positionUnity;
        }

        public Vector3 CoordsLocal()
        {
            //StateUpdate();
            return positionUnity.UnityToENUDirection().ENUToNED();
        }


        public Vector3 AttitudeEuler()
        {
            //StateUpdate();
            return Mathf.Deg2Rad * eulerAngles.UnityToNEDRotation();
        }

        public Vector4 AttitudeQuaternion()
        {
            //StateUpdate();
            return AttitudeEuler().ToRHQuaternion();
        }

        public Vector3 VelocityLocal()
        {
            //StateUpdate();
            return localVelocity.UnityToENUDirection().ENUToNED();
        }

        public Vector3 VelocityBody()
        {
            //StateUpdate();
            return bodyVelocity.UnityToENUDirection().ENUToNED();
        }

		public Vector3 VelocityUnity()
		{
            //StateUpdate();
            return rb.velocity;
		}

        public Vector3 AccelerationBody()
        {
            //StateUpdate();
            return localAcceleration.UnityToENUDirection().ENUToNED();
        }

        public Vector3 AccelerationLocal()
        {
            //StateUpdate();
            return bodyAcceleration.UnityToENUDirection().ENUToNED();
        }

        public Vector3 AngularRatesBody()
        {
            //StateUpdate();
            return bodyAngularVelocity.UnityToNEDRotation();
        }

		public Vector3 AngularRatesUnity ()
		{
            //StateUpdate();
            return rb.angularVelocity;
		}

        // TODO: Implement this method
        public Vector3 MomentBody()
        {
            return Vector3.zero;
        }

        // TODO: Implement this method
        public Vector3 ForceBody()
        {
            return Vector3.zero;
        }

        public bool MotorsArmed()
        {
            return motorsArmed;
        }

        public float FlightTime()
        {
            return flightTime;
        }

        public void ArmDisarm(bool armed)
        {
            motorsArmed = armed;
        }

        public void Place(Vector3 location)
        {
            throw new System.NotImplementedException();
        }

        
        public void InitializeVehicle(Vector3 position, Vector3 velocity, Vector3 euler)
        {
            rb.position = position;
            rb.velocity = velocity;
            rb.rotation = Quaternion.Euler(euler);
            aircraftControl.speed = velocity.magnitude;
            //QuadActivator.Activate(gameObject);
        }

        public void StateUpdate()
        {
            positionUnity = rb.position;

            // Differentiate to get acceleration, filter at tau equal twice the sampling frequency
            localAcceleration = 0.6f*localAcceleration + 0.4f*((rb.velocity - localVelocity) / Time.fixedDeltaTime + new Vector3(0.0f, 9.81f, 0.0f));
            bodyAcceleration = rb.transform.InverseTransformDirection(localAcceleration);

            localVelocity = rb.velocity;
            bodyVelocity = rb.transform.InverseTransformDirection(rb.velocity);

            localAngularVelocity = rb.angularVelocity;
            bodyAngularVelocity = rb.transform.InverseTransformDirection(rb.angularVelocity);                

            eulerAngles = ConstrainEuler(rb.rotation.eulerAngles);

            curSpeed = rb.velocity.magnitude;
            
        }

        public void TimeUpdate()
        {
            if (!Frozen)
            {
                //Debug.Log("Fixed Delta Time = " + Time.fixedDeltaTime);
                flightTime = Time.fixedDeltaTime + flightTime;

                //Debug.Log("Thrust Out: " + thrustOut + " Torque Out: " + torqueOut);
                if (force.y > 0 || torqueOut > 0)
                    energy = energy + Time.fixedDeltaTime * thrustEnergyRate;

                if (throttleRPM > 0)
                    energy = energy + Time.fixedDeltaTime * propEnergyRate;
            }
        }

        public void CmdThrust(float thrust)
        {
            if (thrust > 1)
            {
                //thrust = (1-thrustAlpha)*thrustOut + thrustAlpha*maxThrust;
                thrust = maxThrust;
            } else if (thrust <= 0)
            {
                thrust = 0f;
            }
            else
            {
                //thrust = (1-thrustAlpha)*thrustOut + thrustAlpha*thrust * maxThrust;
                thrust = thrust * maxThrust;
            }

            //force.y = Mathf.Max(thrust + thrustNoise * 2.0f * (Random.value - 1.0f), 0.0f);
            thrustOut = thrust;

            //thrustOut = force.y;
            if (rb == null)
                rb = GetComponent<Rigidbody>();            
            
            
        }

        public void CmdTorque(Vector3 t)
        {
            if (Mathf.Abs(t.y) > 1)
                torque.x = -maxPitchMoment* Mathf.Sign(t.y);
            else
                torque.x = -maxPitchMoment*t.y;

            if (Mathf.Abs(t.z) > 1)
                torque.y = maxYawMoment * Mathf.Sign(t.z);
            else
                torque.y = maxYawMoment * t.z;

            if (Mathf.Abs(t.x) > 1)
                torque.z = -maxRollMoment * Mathf.Sign(t.x);
            else
                torque.z = -maxRollMoment * t.x;

            torque = torque + torqueNoise * Random.insideUnitSphere;
            if (torque.magnitude > maxTorque)
            {
                //Debug.Log("Maximum Torque Commanded: " + t);
                torque = torque * maxTorque / torque.magnitude;
            }

            if (force.y == 0f)
                torque = Vector3.zero;
            //torqueOut = torque.magnitude;

        }

        public void ApplyForceTorque()
        {
            Vector4 momentForcePer = rotorMat.inverse*(new Vector4(-torque.z, -torque.x, torque.y, thrustOut));
            //Debug.Log("rotorMat: " + rotorMat + " Inverse: " + rotorMat.inverse);
            //Debug.Log("Torque: " + torque + " Force: " + force.y);
            //Debug.Log("Moment Force Per: " + momentForcePer);
            for (int i = 0; i < 4; i++)
                momentForcePer[i] = Mathf.Clamp01(momentForcePer[i]);
            Vector4 momentForce = rotorMat*(momentForcePer);

            if (thrustOut > 0)
            {
                force.y = (1 - thrustAlpha) * force.y + thrustAlpha * momentForce.w;
                torque.x = -momentForce.y;
                torque.y = momentForce.z;
                torque.z = -momentForce.x;
                torqueOut = torque.magnitude;
            }
            else
            {
                force.y = 0;
                torque = Vector3.zero;
                torqueOut = torque.magnitude;
            }

            

            //Debug.Log("Moment Force Per: " + momentForcePer + " Moment Force: " + momentForce);
            
            rb.AddRelativeForce(force, ForceMode.Force);
            rb.AddRelativeTorque(torque, ForceMode.Force);

            if (force.magnitude > 0 || torque.magnitude > 0)
                rb.drag = 0.1f;
            else
                rb.drag = 0.015f;

        }



    }
}