﻿using UnityEngine;
using FlightUtils;


namespace DroneControllers
{

    public class QuadController : MonoBehaviour
    {
        public static QuadController ActiveController;

        const double latitude0 = 37.412939d;

        const double longitude0 = -121.995635d;

        double homeLatitude = 0.0d;

        double homeLongitude = 0.0d;

        public float ForceNoise = 2.0f;

        public float TorqueNoise = 1.0f;

        public float HDOP = 0.5f;

        public float VDOP = 0.1f;

        public bool RotorsEnabled { get; set; }

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

        /// <summary>
        /// Defined as longitude, altitude, latitude.
        /// </summary>
        public Vector3 GPS;

        /// <summary>
        /// x-axis -> pitch
        /// y-axis -> yaw
        /// z-axis -> roll
        /// </summary>
        public Vector3 eulerAngles;

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

        public SimpleQuadController inputCtrl;

        public bool clampForce = true;
        public bool clampTorque = true;
        public float maxForce = 40;
        public float maxTorqueDegrees = 17;
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
            if (ActiveController == null)
            {
                ActiveController = this;
            }
            rb = GetComponent<Rigidbody>();
            rotors = new Transform[4] { frontLeftRotor, frontRightRotor, rearLeftRotor, rearRightRotor };
            RotorsEnabled = true;
            //		UseGravity = false;
            Forward = forward.forward;
            Right = right.forward;
            Up = transform.up;
            // transform.position = Vector3.up * 10;
            UseGravity = rb.useGravity;
            UpdateConstraints();
            rb.maxAngularVelocity = Mathf.Infinity;
            inputCtrl = GetComponent<SimpleQuadController>();
            fnNoise = new FastNoise(System.TimeSpan.FromTicks(System.DateTime.UtcNow.Ticks).Seconds);
        }

        void Start()
        {
			// for now disable this inertia thing
//            rb.inertiaTensorRotation = Quaternion.identity;
            // For whatever reason, setting inertiaTensorRotation stops the quad from accepting commands (mostly torque) until it's deactivated and activated.
//            QuadActivator.Activate(gameObject);
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
        }

        void LateUpdate()
        {
            if (resetFlag)
            {
                ResetOrientation();
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
                if (inputCtrl.armed)
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
            RotorsEnabled = inputCtrl.armed;
            if (resetFlag)
            {
                ResetOrientation();
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
            NavigationUpdate();
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
            force.y = Mathf.Max(thrust + ForceNoise * 2.0f * (Random.value - 1.0f), 0.0f);
        }

        public void CmdTorque(Vector3 t)
        {
            torque.x = -t.y;
            torque.y = t.z;
            torque.z = -t.x;
            torque = torque + TorqueNoise * Random.insideUnitSphere;
        }

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

        /// Convenience retrieval functions. These probably should be set as properties
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
        }
        public double GetLatitude()
        {
            return GPS.z + latitude0;
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
            return GPS.x + longitude0;
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
        public void NavigationUpdate()
        {
            // Update acceleration
            LinearAcceleration = (rb.velocity - lastVelocity) / Time.deltaTime;
            AngularVelocityBody = rb.transform.InverseTransformDirection(rb.angularVelocity);
            AngularAccelerationBody = (AngularVelocityBody - lastAngularVelocity) / Time.deltaTime;
            lastVelocity = rb.velocity;
            LinearVelocity = rb.velocity;
            BodyVelocity = rb.transform.InverseTransformDirection(rb.velocity);
            navTransform = rb.transform;

            eulerAngles = rb.rotation.eulerAngles;
            eulerAngles = ConstrainEuler(eulerAngles);

            // Temporary low pass filtered noise on the position (need to implement a Gaussian distribution in the future)
            // NOTE: These numbers result in nice noise values and are MAGICAL
            lat_noise = 0.9f * lat_noise + 0.04f * HDOP * fnNoise.GetSimplex(Time.time * 121.7856f, 0, 0);
            alt_noise = 0.9f * alt_noise + 0.04f * VDOP * fnNoise.GetSimplex(0, Time.time * 23.14141f, 0);
            lon_noise = 0.9f * lon_noise + 0.04f * HDOP * fnNoise.GetSimplex(0, 0, Time.time * 127.7334f);


            //            lat_noise = 0.9f * lat_noise + 0.2f * HDOP * (Random.value - 0.5f);
            //            alt_noise = 0.9f * alt_noise + 0.2f * VDOP * (Random.value - 0.5f);
            //            lon_noise = 0.9f * lon_noise + 0.2f * HDOP * (Random.value - 0.5f);

            // GPS only reported in local frame because float doesn't have precision required for full GPS coordinate
            var Meter2Latitude = FlightUtils.Conversions.Meter2Latitude;
            var Meter2Longitude = FlightUtils.Conversions.Meter2Longitude;
            GPS.x = (float)(rb.position.x * Meter2Longitude + Meter2Longitude * lon_noise);
            GPS.y = rb.position.y + alt_noise;
            GPS.z = (float)(rb.position.z * Meter2Latitude + Meter2Latitude * lat_noise);

            curSpeed = rb.velocity.magnitude;
        }
    }
}