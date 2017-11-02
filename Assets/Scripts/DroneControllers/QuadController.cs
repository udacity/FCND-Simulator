using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using System.IO;


namespace DroneControllers
{

    public class QuadController : MonoBehaviour
    {
        const float MPHToMS = 2.23693629205f;
        public static QuadController ActiveController;
        public static int ImageWidth = 640;
        public static int ImageHeight = 480;

        const float M2Latitude = 1.0f / 111111.0f;
        const double latitude0 = 37.412939d;
        const double longitude0 = 121.995635d;

        double homeLatitude = 0.0d;
        double homeLongitude = 0.0d;
        const float M2Longitude = 1.0f / (0.8f * 111111.0f);

        public float ForceNoise = 2.0f;
        public float TorqueNoise = 1.0f;
        public float HDOP = 0.5f;
        public float VDOP = 0.1f;

        public bool MotorsEnabled { get; set; }
        public Vector3 Force { get { return force; } }
        public Vector3 Torque { get { return torque; } }
        public Vector3 Position { get; protected set; }
        public Quaternion Rotation { get; protected set; }
        public Vector3 AngularVelocity { get; protected set; }
        public Vector3 AngularVelocityBody { get; protected set; }
        public Vector3 AngularAccelerationBody { get; protected set; }
        public Vector3 LinearVelocity { get; protected set; }
        public Vector3 BodyVelocity { get; protected set; }
        public Vector3 LinearAcceleration { get; protected set; }
        public Vector3 GPS;// defined as latitude, altitude, longitude
        public Vector3 eulerAngles; //yaw, roll, pitch, in degrees



        public Vector3 Forward { get; protected set; }
        public Vector3 Right { get; protected set; }
        public Vector3 Up { get; protected set; }
        public Vector3 YAxis { get; protected set; }
        public Vector3 XAxis { get; protected set; }
        public bool UseGravity { get; set; }
        public bool ConstrainForceX { get; set; }
        public bool ConstrainForceY { get; set; }
        public bool ConstrainForceZ { get; set; }
        public bool ConstrainTorqueX { get; set; }
        public bool ConstrainTorqueY { get; set; }
        public bool ConstrainTorqueZ { get; set; }

        public Transform navTransform;

        public Transform frontLeftRotor;
        public Transform frontRightRotor;
        public Transform rearLeftRotor;
        public Transform rearRightRotor;
        public Transform yAxis;
        public Transform xAxis;
        public Transform forward;
        public Transform right;

        public Camera droneCam1;
        public SimpleQuadController inputCtrl;

        public bool clampForce = true;
        public bool clampTorque = true;
        public float maxForce = 100;
        public float maxTorqueDegrees = 17;
        public float maxTorqueRadians;
        //	public bool clampMaxSpeed = true;
        //	public bool clampAngularVelocity = true;
        //	public float maxSpeedMPH = 60;
        //	public float maxSpeedMS;
        //	public float maxAngularDegrees = 17;
        //	public float maxAngularRadians;
        //	public float thrustForce = 2000;
        //	public float torqueForce = 500;
        public ForceMode forceMode = ForceMode.Force;
        public ForceMode torqueMode = ForceMode.Force;

        public Texture2D[] axisArrows;
        public Color[] axisColors;
        public float arrowScreenSize = 100f;
        public bool drawArrows;
        public bool drawArrowsAlways;
        public bool showLegend;

        public bool rotateWithTorque;

        public bool spinRotors = true;
        public float maxRotorRPM = 3600;
        [SerializeField]
        float curRotorSpeed;

        // recording vars
        public float pathRecordFrequency = 3;
        [System.NonSerialized]
        public bool isRecordingPath;
        float nextNodeTime;

        [System.NonSerialized]
        public Rigidbody rb;
        Transform[] rotors;
        Vector3 force;
        Vector3 torque;
        Vector3 lastVelocity;
        Vector3 lastAngularVelocity;
        Ray ray;
        RaycastHit rayHit;
        BinarySerializer b = new BinarySerializer(1000);

        [SerializeField]
        float curSpeed;

        byte[] cameraData;
        bool resetFlag;
        bool setPoseFlag;
        bool useTwist;

        Vector3 posePosition;
        Quaternion poseOrientation;
        Texture2D dot;

        float lat_noise = 0.0f;
        float lon_noise = 0.0f;
        float alt_noise = 0.0f;

		FastNoise fnNoise;

        void Awake()
        {
            if (ActiveController == null)
                ActiveController = this;
            rb = GetComponent<Rigidbody>();
            rotors = new Transform[4] {
            frontLeftRotor,
            frontRightRotor,
            rearLeftRotor,
            rearRightRotor
        };
            MotorsEnabled = true;
            //		UseGravity = false;
            Forward = forward.forward;
            Right = right.forward;
            Up = transform.up;
            CreateCameraTex();
            // transform.position = Vector3.up * 10;
            UseGravity = rb.useGravity;
            UpdateConstraints();
            rb.maxAngularVelocity = Mathf.Infinity;
            inputCtrl = GetComponent<SimpleQuadController>();
			fnNoise = new FastNoise ( System.TimeSpan.FromTicks ( System.DateTime.UtcNow.Ticks ).Seconds );
        }

        void Start()
        {
            rb.inertiaTensorRotation = Quaternion.identity;
            // for whatever reason, setting inertiaTensorRotation stops the quad from accepting commands (mostly torque) until it's deactivated and activated
            QuadActivator.Activate(gameObject);
        }

        private void OnDestroy()
        {

        }

        void Update()
        {
            if (resetFlag)
            {
                ResetOrientation();
                resetFlag = false;
            }
            CheckSetPose();

            Position = transform.position;
            Rotation = transform.rotation;
            Forward = forward.forward;
            Right = right.forward;
            Up = transform.up;
            XAxis = xAxis.forward;
            YAxis = yAxis.forward;

            if (isRecordingPath && Time.time > nextNodeTime)
            {
                PathPlanner.AddNode(Position, Rotation);
                nextNodeTime = Time.time + pathRecordFrequency;
            }
        }

        void LateUpdate()
        {
            if (resetFlag)
            {
                ResetOrientation();
                resetFlag = false;
            }
            CheckSetPose();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
            }

            if (Input.GetKeyDown(KeyCode.L))
                showLegend = !showLegend;

            // use this to have a follow camera rotate with the quad. not proper torque!
            if (rotateWithTorque)
            {
                float zAngle = 0;
                Vector3 up = transform.up;
                if (up.y >= 0)
                    zAngle = transform.localEulerAngles.z;
                else
                    zAngle = -transform.localEulerAngles.z;
                while (zAngle > 180)
                    zAngle -= 360;
                while (zAngle < -360)
                    zAngle += 360;
                transform.Rotate(Vector3.up * -zAngle * Time.deltaTime, Space.World);
            }

            // spin rotors if we need
            if (spinRotors)
            {
                float rps = maxRotorRPM / 60f;
                float degPerSec = rps * 360f;
                if (inputCtrl.motors_armed)
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
            MotorsEnabled = inputCtrl.motors_armed;
            if (resetFlag)
            {
                ResetOrientation();
                resetFlag = false;
            }
            CheckSetPose();

            rb.useGravity = UseGravity;
            CheckConstraints();

            if (MotorsEnabled)
            {
                // add force
                if (clampForce)
                    force = Vector3.ClampMagnitude(force, maxForce);
                rb.AddRelativeForce(force, forceMode);

                // add torque. but first clamp it
                if (maxTorqueDegrees != 0)
                    maxTorqueRadians = maxTorqueDegrees * Mathf.Deg2Rad;

                if (clampTorque)
                    torque = Vector3.ClampMagnitude(torque, maxTorqueRadians);
                //				rb.AddRelativeTorque ( newTorque, torqueMode );
                rb.AddRelativeTorque(torque, torqueMode);



            }

            NavigationUpdate();
        }

        Vector3 FixEuler(Vector3 euler)
        {
            euler.x = FixAngle(euler.x);
            euler.y = FixAngle(euler.y);
            euler.z = FixAngle(euler.z);
            return euler;
        }

        float FixAngle(float angle)
        {
            if (angle > 180f)
                angle -= 360f;
            if (angle < -180f)
                angle += 360f;

            return angle;
        }

        public void ApplyMotorForce(Vector3 v, bool convertFromRos = false)
        {
            useTwist = false;
            force = v + ForceNoise * Random.insideUnitSphere;

        }

        public void ApplyMotorTorque(Vector3 v, bool convertFromRos = false)
        {
            useTwist = false;
            torque = v + TorqueNoise * Random.insideUnitSphere;
        }

        public void SetLinearVelocity(Vector3 v, bool convertFromRos = false)
        {
            useTwist = true;
            force = torque = Vector3.zero;
            LinearVelocity = convertFromRos ? v.ToUnity() : v;
        }

        public void SetAngularVelocity(Vector3 v, bool convertFromRos = false)
        {
            useTwist = true;
            force = torque = Vector3.zero;
            AngularVelocityBody = convertFromRos ? v.ToUnity() : v;
        }

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

        public void SetPositionAndOrientation(Vector3 pos, Quaternion orientation, bool convertFromRos = false)
        {
            setPoseFlag = true;
            if (convertFromRos)
            {
                posePosition = pos.ToUnity();
                poseOrientation = orientation.ToUnity();
            }
            else
            {
                posePosition = pos;
                poseOrientation = orientation;
            }
        }

        void CreateCameraTex()
        {
            // for now, just prep a byte[] that we can put raycast data into


            //		cameraTex = new RenderTexture ( ImageWidth, ImageHeight, 0, RenderTextureFormat.RHalf );
            //		cameraTex.enableRandomWrite = true;
            //		cameraTex.Create ();
        }

        public byte[] GetImageData()
        {
            return cameraData;
        }

        public void BeginRecordPath()
        {
            isRecordingPath = true;
            PathPlanner.AddNode(Position, Rotation);
            nextNodeTime = Time.time + pathRecordFrequency;
        }

        public void EndRecordPath()
        {
            PathPlanner.AddNode(Position, Rotation);
            isRecordingPath = false;
        }

        void CheckConstraints()
        {
            RigidbodyConstraints c = RigidbodyConstraints.None;
            if (ConstrainForceX)
                c |= RigidbodyConstraints.FreezePositionZ;
            if (ConstrainForceY)
                c |= RigidbodyConstraints.FreezePositionX;
            if (ConstrainForceZ)
                c |= RigidbodyConstraints.FreezePositionY;
            if (ConstrainTorqueX)
                c |= RigidbodyConstraints.FreezeRotationZ;
            if (ConstrainTorqueY)
                c |= RigidbodyConstraints.FreezeRotationX;
            if (ConstrainTorqueZ)
                c |= RigidbodyConstraints.FreezeRotationY;
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



        //Convenience retrieval functions. These probably should be set as properties
        public void SetHomePosition(double longitude, double latitude, double altitude)
        {
            SetHomeLongitude(longitude);
            SetHomeLatitude(latitude);
            //Currently you can only set the home lat/lon, not altitude
        }
        public double GetLatitude()
        {
            return GPS.x + latitude0;
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

        public double GetLongitude()
        {
            return -1.0d * (GPS.z + longitude0);
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
            Vector3 localPosition;
            
            localPosition.x = (float)(latitude - GetHomeLatitude()) / M2Latitude;
            localPosition.y = (float)(longitude - GetHomeLongitude()) / M2Longitude;
            localPosition.z = (float)(-altitude);
            return localPosition;
            
        }

        public double GetAltitude()
        {
            return GPS.y;
        }

        public float GetNorthVelocity()
        {
            return LinearVelocity.x;
        }

        public float GetEastVelocity()
        {
            return -LinearVelocity.z;
        }

        public float GetVerticalVelocity()
        {
            return LinearVelocity.y;
        }

        public float GetYaw()
        {
            float yaw = eulerAngles.y;
            if (yaw < 0)
                yaw = yaw + 360.0f;
            return yaw;
        }

        public float GetPitch()
        {
            return eulerAngles.z;
        }

        public float GetRoll()
        {
            return eulerAngles.x;
        }

        public void NavigationUpdate()
        {
            // update acceleration
            LinearAcceleration = (rb.velocity - lastVelocity) / Time.deltaTime;
            AngularVelocityBody = rb.transform.InverseTransformDirection(rb.angularVelocity);
            AngularAccelerationBody = (AngularVelocityBody - lastAngularVelocity) / Time.deltaTime;
            lastVelocity = rb.velocity;
            LinearVelocity = rb.velocity;
            BodyVelocity = rb.transform.InverseTransformDirection(rb.velocity);
            navTransform = rb.transform;

            eulerAngles = rb.rotation.eulerAngles;

            for (int i = 0; i < 3; i++)
            {
                if (eulerAngles[i] > 180.0f)
                    eulerAngles[i] = eulerAngles[i] - 360.0f;
                else if (eulerAngles[i] < -180.0f)
                    eulerAngles[i] = eulerAngles[i] + 360.0f;
            }

            //Temporary low pass filtered noise on the position (need to implement a Gaussian distribution in the future)
			lat_noise = 0.9f * lat_noise + 0.1f * 0.4f * HDOP * fnNoise.GetSimplex ( Time.time * 121.7856f, 0, 0 );
			alt_noise = 0.9f * alt_noise + 0.1f * 0.4f * VDOP * fnNoise.GetSimplex ( 0, Time.time * 23.14141f, 0 );
			lon_noise = 0.9f * lon_noise + 0.1f * 0.4f * HDOP * fnNoise.GetSimplex ( 0, 0, Time.time * 127.7334f );

//            lat_noise = 0.9f * lat_noise + 0.1f * 2.0f * HDOP * (Random.value - 0.5f);
//            alt_noise = 0.9f * alt_noise + 0.1f * 2.0f * VDOP * (Random.value - 0.5f);
//            lon_noise = 0.9f * lon_noise + 0.1f * 2.0f * HDOP * (Random.value - 0.5f);

            //GPS only reported in local frame because float doesn't have precision required for full GPS coordinate
            GPS.x = rb.position.x * M2Latitude + M2Latitude * lat_noise;
            GPS.y = rb.position.y + alt_noise;
            GPS.z = rb.position.z * M2Longitude + M2Longitude * lon_noise;

            curSpeed = rb.velocity.magnitude;
        }
    }
}