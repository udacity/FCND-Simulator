using UnityEngine;
using FlightUtils;
using DroneInterface;


namespace DroneVehicles
{

    public class QuadVehicle : MonoBehaviour, IDroneVehicle
    {
        public int Status
        {
            get { return 0; }
            set { }
        }

        public float FlightTime()
        {
            return flightTime;
        }
		public bool Frozen
		{
			get { return rb.isKinematic; }
			set { rb.isKinematic = value; }
		}

        float flightTime;

        //Vehicle mass properties (based off https://scholar.google.com/scholar?cluster=8960065662684134743&hl=en&as_sdt=0,5)
        public float vehicleMass = 0.5f; // kg
        public float Ixx = 0.005f; // Momenet of inertia for quad forward axis in Newton*m
        public float Iyy = 0.005f; // Moment of inertia for quad up axis in Newton*m
        public float Izz = 0.01f; // Moment of inertial for quad right axis in Newton*m
        public static QuadVehicle ActiveVehicle;

        //Motor properties
        public float maxForce = 10.0f; // Max thrust force in Newtons. approximately 2-1 max thrust to weight ratio
        public float maxTorque = 1.0f; // Max magnitude of the torque in Newton*m, assuming 2 motors and a distance of about 0.2
        public float thrustOut = 0.0f; // For output/display
        public float TorqueOut = 0.0f; // For output/display

        //Sensor noise properties
        public float ForceNoise = 0.5f; // Random noise added to thrust inputs in Newton
        public float TorqueNoise = 0.005f; // Random noise added to torque inputs in Newton*meter
        public float HDOP = 0.5f; // Random noise added to the horizontal GPS position
        public float VDOP = 0.1f; // Random noise added to the vertical GPS position

        double homeLatitude = 0; // Global latitude from which the local position is calculated. Note: can be changed mid flight
        double homeLongitude = 0; // Global longitude from which the local position is calculated. Note: can be changed mid flight

        public bool RotorsEnabled { get; set; }

        public bool motorsArmed { get; set; }
        public Vector3 Force { get { return force; } }
        public Vector3 Torque { get { return torque; } }

        //Vehicle properties in Unity coordinates
        public Vector3 Position { get; protected set; }
        //public Quaternion Rotation { get; protected set; }
        public Vector3 AngularVelocity { get; protected set; }
        public Vector3 AngularVelocityBody { get; protected set; }
        public Vector3 AngularAccelerationBody { get; protected set; }
        public Vector3 LinearVelocity { get; protected set; }
        public Vector3 BodyVelocity { get; protected set; }
        public Vector3 LinearAcceleration { get; protected set; }
        public Vector3 LinearAccelerationBody { get; protected set; }

        /// <summary>
        /// x-axis -> pitch
        /// y-axis -> yaw
        /// z-axis -> roll
        /// In Unity coordinates (Left-handed)
        /// </summary>
        public Vector3 eulerAngles;

        //Drone axes and transforms (uncertain if any are still used)
        public Vector3 Forward { get; protected set; }
        public Vector3 Right { get; protected set; }
        public Vector3 Up { get; protected set; }
//        public Vector3 YAxis { get; protected set; }
//        public Vector3 XAxis { get; protected set; }

        public bool UseGravity { get; set; }
        public bool ConstrainForceX { get; set; }
        public bool ConstrainForceY { get; set; }
        public bool ConstrainForceZ { get; set; }
        public bool ConstrainTorqueX { get; set; }
        public bool ConstrainTorqueY { get; set; }
        public bool ConstrainTorqueZ { get; set; }

        public Transform frontLeftRotor;
        public Transform frontRightRotor;
        public Transform rearLeftRotor;
        public Transform rearRightRotor;
//        public Transform yAxis;
//        public Transform xAxis;
        public Transform forward;
        public Transform right;

        //The autopilot object
        //public SimpleQuadController inputCtrl;

        //Old clamp parameters Note: currently not used
        public bool clampForce = true;
        public bool clampTorque = true;
        public float maxTorqueDegrees = 17;

        public ForceMode forceMode = ForceMode.Force;
        public ForceMode torqueMode = ForceMode.Force;
        public bool rotateWithTorque;
        public bool spinRotors = true;
        public float maxRotorRPM = 150;

        [SerializeField]
        float curRotorSpeed;

        ///
        /// Recording vars
        ///

        [System.NonSerialized]
        public Rigidbody rb;
        Transform[] rotors;
        Vector3 force;
        Vector3 torque;
        Vector3 lastVelocity;
        Vector3 lastAngularVelocity;

        [SerializeField]
        float curSpeed;

        bool resetFlag;
        bool setPoseFlag;
        bool useTwist;

        Vector3 posePosition;
        Quaternion poseOrientation;

        float lat_noise = 0.0f;
        float lon_noise = 0.0f;
        float alt_noise = 0.0f;

        FastNoise fnNoise;

        void Awake()
        {
            if (ActiveVehicle == null)
            {
                ActiveVehicle = this;
            }
            rb = GetComponent<Rigidbody>();
            rotors = new Transform[4] { frontLeftRotor, frontRightRotor, rearLeftRotor, rearRightRotor };
            RotorsEnabled = true;
            UseGravity = true;
            Forward = forward.forward;
            Right = right.forward;
            Up = transform.up;
            // transform.position = Vector3.up * 10;
            UseGravity = rb.useGravity;
            UpdateConstraints();
            rb.maxAngularVelocity = Mathf.Infinity;
            //inputCtrl = GetComponent<SimpleQuadController>();
            motorsArmed = false;
            fnNoise = new FastNoise(System.TimeSpan.FromTicks(System.DateTime.UtcNow.Ticks).Seconds);
            flightTime = 0.0f;
        }

        void Start()
        {
            rb.inertiaTensor = new Vector3(Izz, Iyy, Ixx);
            rb.mass = vehicleMass;
            // For whatever reason, setting inertiaTensorRotation stops the quad from accepting commands (mostly torque) until it's deactivated and activated.
			QuadActivator.Activate(gameObject);
        }

        private void OnDestroy()
        {
        }

        void Update()
        {
            if (resetFlag)
            {
                // ResetOrientation();
                resetFlag = false;
            }
            CheckSetPose();

            Position = transform.position;
            //Rotation = transform.rotation;
            Forward = forward.forward;
            Right = right.forward;
            Up = transform.up;
//            XAxis = xAxis.forward;
//            YAxis = yAxis.forward;
        }

        void LateUpdate()
        {
            if (resetFlag)
            {
                // ResetOrientation();
                resetFlag = false;
            }
            CheckSetPose();

            // Use this to have a follow camera rotate with the quad. not proper torque!
            if (rotateWithTorque)
            {
                float zAngle = 0;
                Vector3 up = transform.up;
                if (up.y >= 0)
                {
                    zAngle = transform.localEulerAngles.z;
                }
                else
                {
                    zAngle = -transform.localEulerAngles.z;
                }
                while (zAngle > 180)
                {
                    zAngle -= 360;
                }
                while (zAngle < -360)
                {
                    zAngle += 360;
                }
                transform.Rotate(Vector3.up * -zAngle * Time.deltaTime, Space.World);
            }

            // Spin rotors if we need
            if (spinRotors)
            {
                float rps = maxRotorRPM / 60f;
                float degPerSec = rps * 360f;
                if (motorsArmed)
                {
                    curRotorSpeed = degPerSec;
                }
                else
                {
                    curRotorSpeed = 0.0f;
                    /*
                    if (useTwist)
                    {
                        curRotorSpeed = Mathf.InverseLerp(Physics.gravity.y, -Physics.gravity.y, rb.velocity.y) * degPerSec;
                        //				curRotorSpeed = 0.5f * degPerSec * ( rb.velocity.y + Physics.gravity.y ) / -Physics.gravity.y / rb.mass;
                    }
                    else
                    {
                        curRotorSpeed = 0.5f * degPerSec * force.y / -Physics.gravity.y / rb.mass;
                    }
                    */

                }

                // use forward for now because rotors are rotated -90x
                Vector3 rot = Vector3.forward * curRotorSpeed * Time.deltaTime;
                frontLeftRotor.Rotate(rot);
                frontRightRotor.Rotate(-rot);
                rearLeftRotor.Rotate(-rot);
                rearRightRotor.Rotate(rot);
            }
        }

        void FixedUpdate()
        {
            RotorsEnabled = motorsArmed;
            if (resetFlag)
            {
                // ResetOrientation();
                resetFlag = false;
            }
            CheckSetPose();

            rb.useGravity = UseGravity;
            CheckConstraints();

            if (RotorsEnabled)
            {
                // add force
                if (clampForce)
                {
                    force = Vector3.ClampMagnitude(force, maxForce);
                }
                rb.AddRelativeForce(force, forceMode);

                var maxTorqueRadians = maxTorqueDegrees * Mathf.Deg2Rad;

                if (clampTorque)
                {
                    torque = Vector3.ClampMagnitude(torque, maxTorqueRadians);
                }
                //				rb.AddRelativeTorque ( newTorque, torqueMode );
                rb.AddRelativeTorque(torque, torqueMode);
                
            }
            StateUpdate();
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

        public void ApplyMotorForce(Vector3 v)
        {
            useTwist = false;
            force = v + ForceNoise * Random.insideUnitSphere;
            
        }

        public void CmdThrust(float thrust)
        {
            if(thrust > maxForce)
            {
                //Debug.Log("Max Thrust Commanded: " + thrust);
                thrust = maxForce;
            }
            force.y = Mathf.Max(thrust + ForceNoise * 2.0f * (Random.value - 1.0f), 0.0f);
            if(force.y < 0.0f)
            {
                force.y = 0.0f;
            }
            thrustOut = force.y;
        }

        public void CmdTorque(Vector3 t)
        {
            torque.x = -t.y;
            torque.y = t.z;
            torque.z = -t.x;
            torque = torque + TorqueNoise * Random.insideUnitSphere;
            if(torque.magnitude > maxTorque)
            {
                //Debug.Log("Maximum Torque Commanded: " + t);
                torque = torque * maxTorque / torque.magnitude;
            }
            TorqueOut = torque.magnitude;
            
        }

        /*
        public void ApplyMotorTorque(Vector3 v)
        {
            useTwist = false;
            torque = v + TorqueNoise * Random.insideUnitSphere;
        }

        public void SetLinearVelocity(Vector3 v)
        {
            useTwist = true;
            force = torque = Vector3.zero;
            LinearVelocity = v;
        }

        public void SetAngularVelocity(Vector3 v)
        {
            useTwist = true;
            force = torque = Vector3.zero;
            AngularVelocityBody = v;
        }
        */
        public void TriggerReset()
        {
            resetFlag = true;
        }

        public void ResetOrientation()
        {
            transform.rotation = Quaternion.identity;
            force = Vector3.zero;
            torque = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            LinearAcceleration = Vector3.zero;
            LinearVelocity = Vector3.zero;
            AngularVelocityBody = Vector3.zero;
            rb.isKinematic = true;
            rb.isKinematic = false;
        }

        void CheckSetPose()
        {
            if (setPoseFlag)
            {
                transform.position = posePosition;
                transform.rotation = poseOrientation;
                force = Vector3.zero;
                torque = Vector3.zero;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                LinearAcceleration = Vector3.zero;
                setPoseFlag = false;
            }
        }

        public void SetPositionAndOrientation(Vector3 pos, Quaternion orientation)
        {
            setPoseFlag = true;
            posePosition = pos;
            poseOrientation = orientation;
        }

        void CheckConstraints()
        {
            RigidbodyConstraints c = RigidbodyConstraints.None;
            if (ConstrainForceX)
            {
                c |= RigidbodyConstraints.FreezePositionZ;
            }
            if (ConstrainForceY)
            {
                c |= RigidbodyConstraints.FreezePositionX;
            }
            if (ConstrainForceZ)
            {
                c |= RigidbodyConstraints.FreezePositionY;
            }
            if (ConstrainTorqueX)
            {
                c |= RigidbodyConstraints.FreezeRotationZ;
            }
            if (ConstrainTorqueY)
            {
                c |= RigidbodyConstraints.FreezeRotationX;
            }
            if (ConstrainTorqueZ)
            {
                c |= RigidbodyConstraints.FreezeRotationY;
            }
            rb.constraints = c;
        }

        public void UpdateConstraints()
        {
            ConstrainForceX = (rb.constraints & RigidbodyConstraints.FreezePositionZ) != 0;
            ConstrainForceY = (rb.constraints & RigidbodyConstraints.FreezePositionX) != 0;
            ConstrainForceZ = (rb.constraints & RigidbodyConstraints.FreezePositionY) != 0;
            ConstrainTorqueX = (rb.constraints & RigidbodyConstraints.FreezeRotationZ) != 0;
            ConstrainTorqueY = (rb.constraints & RigidbodyConstraints.FreezeRotationX) != 0;
            ConstrainTorqueZ = (rb.constraints & RigidbodyConstraints.FreezeRotationY) != 0;
        }

        /*
        public void SetHomePosition(double longitude, double latitude, double altitude)
        {
            // NOTE: Currently you can only set the home lat/lon, not altitude
            SetHomeLongitude(longitude);
            SetHomeLatitude(latitude);

            // NOTE: without this the target position isn't properly reset
            // and the drone ends up repeating the previous local command
            inputCtrl.guidedCommand.x = GetLocalNorth();
            inputCtrl.guidedCommand.y = GetLocalEast();
            inputCtrl.guidedCommand.z = GetLocalDown();
        }*/

        /// Convenience retrieval functions. These probably should be set as properties
        /// These functions convert all the local class variables, which are defined in Unity Left-Handed coordinate frames
        /// to the appropriate right handed coordinate frame
        /// 

        public Vector3 CoordsUnity()
        {
            return Position;
        }

        public Vector3 CoordsLocal()
        {
            return Position.UnityToENUDirection().ENUToNED();
        }


        public Vector3 AttitudeEuler()
        {
            return Mathf.Deg2Rad * eulerAngles.UnityToNEDRotation();
        }

        public Vector4 AttitudeQuaternion()
        {
            return AttitudeEuler().ToRHQuaternion();
        }

        public Vector3 VelocityLocal()
        {
            return LinearVelocity.UnityToENUDirection().ENUToNED();
        }

        public Vector3 VelocityBody()
        {
            return BodyVelocity.UnityToENUDirection().ENUToNED();
        }

		public Vector3 VelocityUnity()
		{
			return rb.velocity;
		}

        public Vector3 AccelerationBody()
        {
            return LinearAcceleration.UnityToENUDirection().ENUToNED();
        }

        public Vector3 AccelerationLocal()
        {
            return LinearAccelerationBody.UnityToENUDirection().ENUToNED();
        }

        public Vector3 AngularRatesBody()
        {
            return AngularVelocityBody.UnityToNEDRotation();
        }

		public Vector3 AngularRatesUnity()
		{
			return rb.angularVelocity;
		}

        public Vector3 MomentBody()
        {
            return torque.UnityToNEDRotation();
        }

        public Vector3 ForceBody()
        {
            return force.UnityToENUDirection().ENUToNED();
        }

        public bool MotorsArmed()
        {
            return motorsArmed;
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
        }

        /*
        public double GetLatitude()
        {
            // return GPS.z + homeLatitude;
            return GPS.z + Simulation.latitude0;
        }

        public void SetHomeLatitude(double latitude)
        {
            homeLatitude = latitude;
        }

        public double GetHomeLatitude()
        {
            return homeLatitude;
        }

        public float GetLocalNorth()
        {
            return GlobalToLocalPosition(GetLongitude(), GetLatitude(), GetAltitude()).x;
        }

        public float GetLocalDown()
        {
            return GlobalToLocalPosition(GetLongitude(), GetLatitude(), GetAltitude()).z;
        }

        public double GetLongitude()
        {
            // return GPS.x + homeLongitude;
			return GPS.x + Simulation.longitude0;
        }

        public void SetHomeLongitude(double longitude)
        {
            homeLongitude = longitude;
        }

        public double GetHomeLongitude()
        {
            return homeLongitude;
        }

        public float GetLocalEast()
        {
            return GlobalToLocalPosition(GetLongitude(), GetLatitude(), GetAltitude()).y;
        }

        public Vector3 GlobalToLocalPosition(double longitude, double latitude, double altitude)
        {
            // Debug.Log(string.Format("lon = {0}, lat = {1}, alt = {2}, hlon = {3}, hlat = {4}", longitude, latitude, altitude, GetHomeLongitude(), GetHomeLatitude()));
            var v = FlightUtils.Conversions.GlobalToLocalCoords(longitude, latitude, altitude, GetHomeLongitude(), GetHomeLatitude());
            // Debug.Log("here " + v);
            return v;
        }

        public double GetAltitude()
        {
            return GPS.y;
        }

        public float GetNorthVelocity()
        {
            return LinearVelocity.z;
        }

        public float GetEastVelocity()
        {
            return LinearVelocity.x;
        }

        public float GetDownVelocity()
        {
            return -LinearVelocity.y;
        }

        public float GetNorthAcceleration()
        {
            return LinearAcceleration.z;
        }

        public float GetEastAcceleration()
        {
            return LinearAcceleration.x;
        }

        public float GetDownAcceleration()
        {
            return -LinearAcceleration.y;
        }

        public float GetFrontAcceleration()
        {
            return LinearAccelerationBody.z;
        }

        public float GetRightAcceleration()
        {
            return LinearAccelerationBody.x;
        }

        public float GetBottomAcceleration()
        {
            return -LinearAccelerationBody.y;
        }

        public float GetVerticalVelocity()
        {
            return LinearVelocity.y;
        }

        public float GetYaw()
        {
            return eulerAngles.y * Mathf.Deg2Rad;
        }

        public float GetPitch()
        {
            return -eulerAngles.x * Mathf.Deg2Rad;
        }

        public float GetRoll()
        {
            return -eulerAngles.z * Mathf.Deg2Rad;
        }

        public float GetRollrate()
        {
            return -AngularVelocityBody.z;
        }

        public float GetPitchrate()
        {
            return -AngularVelocityBody.x;
        }

        public float GetYawrate()
        {
            return AngularVelocityBody.y;
        }

        public Vector4 QuaternionAttitude()
        {
            return (new Vector3(GetRoll(), GetPitch(), GetYaw())).ToRHQuaternion();
        }
        */
        public void StateUpdate()
        {
            Position = rb.position;

            // Differentiate to get acceleration, filter at tau equal twice the sampling frequency
            LinearAcceleration = 0.6f*LinearAcceleration + 0.4f*((rb.velocity - LinearVelocity) / Time.fixedDeltaTime + new Vector3(0.0f, 9.81f, 0.0f));
            LinearAccelerationBody = rb.transform.InverseTransformDirection(LinearAcceleration);

            LinearVelocity = rb.velocity;
            BodyVelocity = rb.transform.InverseTransformDirection(rb.velocity);

            AngularVelocity = rb.angularVelocity;
            AngularVelocityBody = rb.transform.InverseTransformDirection(rb.angularVelocity);                

            eulerAngles = ConstrainEuler(rb.rotation.eulerAngles);

            curSpeed = rb.velocity.magnitude;

            if (!Frozen)
                flightTime = flightTime + Time.fixedDeltaTime;
        }
    }
}