using UnityEngine;
using DroneVehicles;
using DroneInterface;
using UdaciPlot;

namespace DroneSensors
{
    public class QuadSensors : MonoBehaviour, IDroneSensors
    {
        public IDroneSystem drone;

        public float imuRateHz = 50;
        float timeSinceImuS;
        Vector3 imuAcceleration;
        Vector3 imuNoiseSigma = new Vector3(1.0f, 1.0f, 3.0f);

        public float gyroRateHz = 50;
        float timeSinceGyroS;
        Vector3 gyroRates;
        Vector3 gyroNoiseSigma = new Vector3(1.0f, 1.0f, 1.0f);
        
        public float compassRateHz = 1;
        float timeSinceCompassS;
        float compassHeading;
        Vector3 compassMagnetometer;
        Vector3 compassNoiseSigma = new Vector3(0.01f, 0.01f, 0.01f);

        public float barometerRateHz = 1;
        float timeSinceBarometerS;
        float barometerAltitude;
        float barometerNoiseSigma = 0.1f;
        
        public float gpsRateHz = 10;
        float timeSinceGpsS;
        double gpsLatitude;
        double gpsLongitude;
        double gpsAltitude;
        double homeLatitude;
        double homeLongitude;
        double homeAltitude;
        Vector3 localPosition;
        Vector3 gpsVelocity;
        Vector3 gpsPositionNoiseSigma = new Vector3(1.0f, 1.0f, 3.0f);
        Vector3 gpsVelocityNoiseSigma = new Vector3(0.1f, 0.1f, 0.3f);

        public float estimateRateHz = 20;
        float timeSinceEstimateS;
        Vector3 attitudeEstimate;

        void Awake()
        {
            drone = GetComponent<IDroneSystem>();
            //Reset all the time counters
            timeSinceBarometerS = 0.0f;
            timeSinceCompassS = 0.0f;
            timeSinceEstimateS = 0.0f;
            timeSinceGpsS = 0.0f;
            timeSinceImuS = 0.0f;
        }

        void FixedUpdate()
        {
            float deltaTime = Time.deltaTime;
            timeSinceBarometerS = timeSinceBarometerS + deltaTime;
            timeSinceCompassS = timeSinceCompassS + deltaTime;
            timeSinceEstimateS = timeSinceEstimateS + deltaTime;
            timeSinceGpsS = timeSinceGpsS + deltaTime;
            timeSinceImuS = timeSinceImuS + deltaTime;

            if(timeSinceBarometerS >= 1.0f / barometerRateHz)
                UpdateBarometer();                

            if(timeSinceCompassS >= 1.0f / compassRateHz)
                UpdateCompass();

            if(timeSinceEstimateS >= 1.0f / estimateRateHz)
                UpdateEstimate();

            if(timeSinceGpsS >= 1.0f / gpsRateHz)
            {
                UpdateGps();
                timeSinceGpsS = 0.0f;
            }

            if(timeSinceGyroS >= 1.0f / gyroRateHz)
            {
                UpdateGyro();
                timeSinceGyroS = 0.0f;
            }
        }

        void UpdateBarometer()
        {
            barometerAltitude = -drone.LocalCoords().z + barometerNoiseSigma * UnifSigma();
            timeSinceBarometerS = 0.0f;
        }

        void UpdateCompass()
        {
            compassHeading = drone.AttitudeEuler().z + compassNoiseSigma.z * UnifSigma();
            compassMagnetometer = new Vector3(1.0f, 0.0f, 0.0f);
            timeSinceCompassS = 0.0f;
        }

        void UpdateEstimate()
        {
            attitudeEstimate = drone.AttitudeEuler();
            timeSinceEstimateS = 0.0f;
        }

        void UpdateGps()
        {
            gpsLatitude = Simulation.latitude0 + drone.LocalCoords().x + gpsPositionNoiseSigma.y * UnifSigma();
            gpsLongitude = Simulation.longitude0 + drone.LocalCoords().y + gpsPositionNoiseSigma.x * UnifSigma();
            gpsAltitude = drone.LocalCoords().z + gpsPositionNoiseSigma.z * UnifSigma();
            gpsVelocity = drone.VelocityLocal();
            gpsVelocity.x = gpsVelocity.x + gpsVelocityNoiseSigma.x * UnifSigma();
            gpsVelocity.y = gpsVelocity.y + gpsVelocityNoiseSigma.y * UnifSigma();
            gpsVelocity.z = gpsVelocity.z + gpsVelocityNoiseSigma.z * UnifSigma();
            localPosition = FlightUtils.Conversions.GlobalToLocalCoords(gpsLongitude, gpsLatitude, gpsAltitude, homeLongitude, homeLatitude);

            timeSinceGpsS = 0.0f;
        }

        void UpdateGyro()
        {
            gyroRates = drone.AngularRatesBody();
            gyroRates.x = gyroRates.x + gyroNoiseSigma.x * UnifSigma();
            gyroRates.y = gyroRates.y + gyroNoiseSigma.y * UnifSigma();
            gyroRates.z = gyroRates.z + gyroNoiseSigma.z * UnifSigma();
        }

        float UnifSigma()
        {
            return Random.Range(-Mathf.Sqrt(3), Mathf.Sqrt(3));
        }


        // Methods requiring for IDroneSensors
        public Vector3 GyroRates()
        {
            return gyroRates;
        }

        public Vector3 IMUAcceleration()
        {
            return imuAcceleration;
        }

        public float CompassHeading()
        {
            return compassHeading;
        }

        public Vector3 CompassMagnetometer()
        {
            return compassMagnetometer;
        }

        public float BarometerAltitude()
        {
            return barometerAltitude;
        }

        public Vector3 AttitudeEstimate()
        {
            return attitudeEstimate;
        }

        public double GPSLatitude()
        {
            return gpsLatitude;
        }

        public double GPSLongitude()
        {
            return gpsLongitude;
        }

        public double GPSAltitude()
        {
            return gpsAltitude;
        }

        public double HomeLatitude()
        {
            return homeLatitude;
        }

        public double HomeLongitude()
        {
            return homeLongitude;
        }

        public double HomeAltitude()
        {
            return homeAltitude;
        }

        public Vector3 LocalPosition()
        {
            return localPosition;
        }

        public Vector3 GPSVelocity()
        {
            return gpsVelocity;
        }

        public void SetHomePosition(double longitude, double latitude, double altitude)
        {
            homeLongitude = longitude;
            homeLatitude = latitude;
            homeAltitude = altitude;
        }






        /*
//		[System.NonSerialized]
        public QuadVehicle quadVehicle;
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
        public QuadSystemMovementBehavior mb_Manual;
        public QuadSystemMovementBehavior mb_ManualPosCtrl;
        public QuadSystemMovementBehavior mb_ManualAttCtrl;
        public QuadSystemMovementBehavior mb_GuidedPosCtrl;
        public QuadSystemMovementBehavior mb_GuidedAttCtrl;
        public QuadSystemMovementBehavior mb_GuidedMotors;


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
        public QuadSystemMovementBehavior currentMovementBehavior;

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
                Vector3 LocalPosition = quadVehicle.LocalCoords();
                CommandLocal(LocalPosition.x, LocalPosition.y, LocalPosition.z); 
            }
                
            if (Input.GetButtonDown("Position Control"))
            {
                positionControl = !positionControl;
                if (positionControl)
                {
                    Vector3 LocalPosition = quadVehicle.LocalCoords();
                    posHoldLocal = new Vector3(LocalPosition.x, LocalPosition.y, LocalPosition.z);
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
            localPosition = quadSensor.GlobalToLocalPosition(longitude, latitude, altitude);
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
        */
    }
}