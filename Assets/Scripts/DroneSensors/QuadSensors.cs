using UnityEngine;
using DroneVehicles;
using DroneInterface;
using UdaciPlot;

namespace DroneSensors
{
    public class QuadSensors : MonoBehaviour, IDroneSensors
    {
        public IDrone drone;

        public float imuRateHz = 500;
        float timeSinceImuS;
        Vector3 imuAcceleration;
        public Vector3 imuNoiseSigma = new Vector3(1.0f, 1.0f, 3.0f);

        public float gyroRateHz = 500;
        float timeSinceGyroS;
        Vector3 gyroRates;
        public Vector3 gyroNoiseSigma = (new Vector3(0.01f, 0.01f, 0.01f))*0.0f;
        
        public float compassRateHz = 1;
        float timeSinceCompassS;
        float compassHeading;
        Vector3 compassMagnetometer;
        public Vector3 compassNoiseSigma = new Vector3(0.01f, 0.01f, 0.01f);

        public float barometerRateHz = 1;
        float timeSinceBarometerS;
        float barometerAltitude;
        public float barometerNoiseSigma = 0.1f;
        
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
        public Vector3 gpsPositionNoiseSigma = new Vector3(1.0f, 1.0f, 3.0f);
        public Vector3 gpsVelocityNoiseSigma = new Vector3(0.1f, 0.1f, 0.3f);

        public float estimateRateHz = 500;
        float timeSinceEstimateS;
        Vector3 attitudeEstimate;
        Vector3 positionEstimate;
        Vector3 velocityEstimate;
        Vector3 angularRateEstimate;

        //Legacy noise parameters
        float HDOP = 0.5f; // Random noise added to the horizontal GPS position
        float VDOP = 0.5f; // Random noise added to the vertical GPS position
        float lat_noise = 0.0f;
        float lon_noise = 0.0f;
        float alt_noise = 0.0f;
        FastNoise fnNoise;

        void Awake()
        {
            drone = GetComponent<IDrone>();
            //Reset all the time counters
            timeSinceBarometerS = 0.0f;
            timeSinceCompassS = 0.0f;
            timeSinceEstimateS = 0.0f;
            timeSinceGpsS = 0.0f;
            timeSinceImuS = 0.0f;

            //Legacy calculation
            fnNoise = new FastNoise(System.TimeSpan.FromTicks(System.DateTime.UtcNow.Ticks).Seconds);
        }

        void FixedUpdate()
        {
            float deltaTime = Time.deltaTime;
            timeSinceBarometerS = timeSinceBarometerS + deltaTime;
            timeSinceCompassS = timeSinceCompassS + deltaTime;
            timeSinceEstimateS = timeSinceEstimateS + deltaTime;
            timeSinceGpsS = timeSinceGpsS + deltaTime;
            timeSinceGyroS = timeSinceGyroS + deltaTime;
            timeSinceImuS = timeSinceImuS + deltaTime;

            if(timeSinceBarometerS >= (1.0f / barometerRateHz))
                UpdateBarometer();                

            if(timeSinceCompassS >= (1.0f / compassRateHz))
                UpdateCompass();

            if(timeSinceEstimateS >= (1.0f / estimateRateHz))
                UpdateEstimate();

            if(timeSinceGpsS >= (1.0f / gpsRateHz))
            {
                UpdateGps();
                timeSinceGpsS = 0.0f;
            }

            if(timeSinceGyroS >= (1.0f / gyroRateHz))
            {
                UpdateGyro();
                timeSinceGyroS = 0.0f;
            }

            if(timeSinceImuS >= (1.0f / imuRateHz))
            {
                UpdateImu();
                timeSinceImuS = 0.0f;
            }
        }

        void UpdateBarometer()
        {
            barometerAltitude = -drone.CoordsLocal().z + barometerNoiseSigma * UnifSigma();
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

            //Temp based on legacy calculations 
            lat_noise = 0.9f * lat_noise + 0.04f * HDOP * fnNoise.GetSimplex(Time.time * 121.7856f, 0, 0);
            alt_noise = 0.9f * alt_noise + 0.04f * VDOP * fnNoise.GetSimplex(0, Time.time * 23.14141f, 0);
            lon_noise = 0.9f * lon_noise + 0.04f * HDOP * fnNoise.GetSimplex(0, 0, Time.time * 127.7334f);
            var Meter2Latitude = FlightUtils.Conversions.Meter2Latitude;
            var Meter2Longitude = FlightUtils.Conversions.Meter2Longitude;
            double latitude = Simulation.latitude0 + Meter2Latitude * drone.CoordsLocal().x;
            double longitude = Simulation.longitude0 + Meter2Longitude * drone.CoordsLocal().y;
            positionEstimate = FlightUtils.Conversions.GlobalToLocalCoords(longitude, latitude, -drone.CoordsLocal().z, homeLongitude, homeLatitude);
            positionEstimate.x = positionEstimate.x + lat_noise;
            positionEstimate.y = positionEstimate.y + lon_noise;
            positionEstimate.z = positionEstimate.z + alt_noise;
            velocityEstimate = drone.VelocityLocal();
            angularRateEstimate = drone.AngularRatesBody();

            timeSinceEstimateS = 0.0f;
        }

        void UpdateGps()
        {
            var Meter2Latitude = FlightUtils.Conversions.Meter2Latitude;
            var Meter2Longitude = FlightUtils.Conversions.Meter2Longitude;
            gpsLatitude = Simulation.latitude0 + Meter2Latitude*(drone.CoordsLocal().x + gpsPositionNoiseSigma.y * UnifSigma());
            gpsLongitude = Simulation.longitude0 + Meter2Longitude*(drone.CoordsLocal().y + gpsPositionNoiseSigma.x * UnifSigma());
            gpsAltitude = -drone.CoordsLocal().z + gpsPositionNoiseSigma.z * UnifSigma();
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

        void UpdateImu()
        {
            imuAcceleration = drone.AccelerationBody();
            imuAcceleration.x = imuAcceleration.x + imuNoiseSigma.x * UnifSigma();
            imuAcceleration.y = imuAcceleration.y + imuNoiseSigma.y * UnifSigma();
            imuAcceleration.z = imuAcceleration.z + imuNoiseSigma.z * UnifSigma();
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

        public Vector3 PositionEstimate()
        {
            return positionEstimate;
        }

        public Vector3 VelocityEstimate()
        {
            return velocityEstimate;
        }

        public Vector3 AngularRateEstimate()
        {
            return angularRateEstimate;
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

        public void SetHomePosition()
        {
            UpdateGps();
            homeLatitude = gpsLongitude;
            homeLatitude = gpsLatitude;
            homeAltitude = 0.0;
            localPosition = FlightUtils.Conversions.GlobalToLocalCoords(gpsLongitude, gpsLatitude, gpsAltitude, homeLongitude, homeLatitude);
        }

        public void SetHomePosition(double longitude, double latitude, double altitude)
        {
            homeLongitude = longitude;
            homeLatitude = latitude;
            homeAltitude = altitude;
            localPosition = FlightUtils.Conversions.GlobalToLocalCoords(gpsLongitude, gpsLatitude, gpsAltitude, homeLongitude, homeLatitude);
        }

    }
}