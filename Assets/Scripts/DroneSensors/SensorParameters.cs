using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DroneSensors
{
    public class SensorParameters : MonoBehaviour
    {

        QuadSensors quadSensors;

        [System.NonSerialized]
        public SimParameter paramImuRateHz;
        [System.NonSerialized]
        public SimParameter paramImuNoiseSigmaX;
        [System.NonSerialized]
        public SimParameter paramImuNoiseSigmaY;
        [System.NonSerialized]
        public SimParameter paramImuNoiseSigmaZ;

        [System.NonSerialized]
        public SimParameter paramGyroRateHz;
        [System.NonSerialized]
        public SimParameter paramGyroNoiseSigmaX;
        [System.NonSerialized]
        public SimParameter paramGyroNoiseSigmaY;
        [System.NonSerialized]
        public SimParameter paramGyroNoiseSigmaZ;


        [System.NonSerialized]
        public SimParameter paramCompassRateHz;
        [System.NonSerialized]
        public SimParameter paramCompassNoiseSigmaX;
        [System.NonSerialized]
        public SimParameter paramCompassNoiseSigmaY;
        [System.NonSerialized]
        public SimParameter paramCompassNoiseSigmaZ;


        [System.NonSerialized]
        public SimParameter paramBarometerRateHz;
        [System.NonSerialized]
        public SimParameter paramBarometerNoiseSigma;

        [System.NonSerialized]
        public SimParameter paramGpsRateHz;
        [System.NonSerialized]
        public SimParameter paramGpsNoiseSigmaN;
        [System.NonSerialized]
        public SimParameter paramGpsNoiseSigmaE;
        [System.NonSerialized]
        public SimParameter paramGpsNoiseSigmaD;
        [System.NonSerialized]
        public SimParameter paramGpsNoiseSigmaVN;
        [System.NonSerialized]
        public SimParameter paramGpsNoiseSigmaVE;
        [System.NonSerialized]
        public SimParameter paramGpsNoiseSigmaVD;

        void Awake()
        {
            quadSensors = GetComponent<QuadSensors>();

            paramImuRateHz = new SimParameter("Sensors:imu_rate_hz", quadSensors.imuRateHz, OnImuRateChanged);
            paramImuNoiseSigmaX = new SimParameter("Sensors:imu_noise_sigma_x", quadSensors.imuNoiseSigma.x, OnImuSigmaXChanged);
            paramImuNoiseSigmaY = new SimParameter("Sensors:imu_noise_sigma_y", quadSensors.imuNoiseSigma.y, OnImuSigmaYChanged);
            paramImuNoiseSigmaZ = new SimParameter("Sensors:imu_noise_sigma_z", quadSensors.imuNoiseSigma.z, OnImuSigmaZChanged);

            paramGyroRateHz = new SimParameter("Sensors:gyro_rate_hz", quadSensors.gyroRateHz, OnGyroRateChanged);
            paramGyroNoiseSigmaX = new SimParameter("Sensors:gyro_noise_sigma_x", quadSensors.gyroNoiseSigma.x, OnGyroSigmaXChanged);
            paramGyroNoiseSigmaY = new SimParameter("Sensors:gyro_noise_sigma_y", quadSensors.gyroNoiseSigma.y, OnGyroSigmaYChanged);
            paramGyroNoiseSigmaZ = new SimParameter("Sensors:gyro_noise_sigma_z", quadSensors.gyroNoiseSigma.z, OnGyroSigmaZChanged);

            paramCompassRateHz = new SimParameter("Sensors:compass_rate_hz", quadSensors.compassRateHz, OnCompassRateChanged);
            paramCompassNoiseSigmaX = new SimParameter("Sensors:compass_noise_sigma_x", quadSensors.compassNoiseSigma.x, OnCompassSigmaXChanged);
            paramCompassNoiseSigmaY = new SimParameter("Sensors:compass_noise_sigma_y", quadSensors.compassNoiseSigma.y, OnCompassSigmaYChanged);
            paramCompassNoiseSigmaZ = new SimParameter("Sensors:comass_noise_sigma_z", quadSensors.compassNoiseSigma.z, OnCompassSigmaZChanged);

            paramBarometerRateHz = new SimParameter("Sensors:barometer_rate_hz", quadSensors.barometerRateHz, OnBarometerRateChanged);
            paramBarometerNoiseSigma = new SimParameter("Sensors:barometer_noise_sigma", quadSensors.barometerNoiseSigma, OnBarometerSigmaChanged);

            paramGpsRateHz = new SimParameter("Sensors:gps_rate_hz", quadSensors.gpsRateHz, OnGpsRateChanged);
            paramGpsNoiseSigmaN = new SimParameter("Sensors:gps_noise_sigma_n", quadSensors.gpsPositionNoiseSigma.x, OnGpsSigmaNChanged);
            paramGpsNoiseSigmaE = new SimParameter("Sensors:gps_noise_sigma_e", quadSensors.gpsPositionNoiseSigma.y, OnGpsSigmaEChanged);
            paramGpsNoiseSigmaD = new SimParameter("Sensors:gps_noise_sigma_d", quadSensors.gpsPositionNoiseSigma.z, OnGpsSigmaDChanged);
            paramGpsNoiseSigmaVN = new SimParameter("Sensors:gps_noise_sigma_vn", quadSensors.gpsVelocityNoiseSigma.x, OnGpsSigmaVNChanged);
            paramGpsNoiseSigmaVE = new SimParameter("Sensors:gps_noise_sigma_ve", quadSensors.gpsVelocityNoiseSigma.y, OnGpsSigmaVEChanged);
            paramGpsNoiseSigmaVD = new SimParameter("Sensors:gps_noise_sigma_vd", quadSensors.gpsVelocityNoiseSigma.z, OnGpsSigmaVDChanged);


            
        }
        

        public void OnImuRateChanged(SimParameter p)
        {
            quadSensors.imuRateHz = p.Value;
        }

        public void OnImuSigmaXChanged(SimParameter p)
        {
            quadSensors.imuNoiseSigma.x = p.Value;
        }

        public void OnImuSigmaYChanged(SimParameter p)
        {
            quadSensors.imuNoiseSigma.z = p.Value;
        }

        public void OnImuSigmaZChanged(SimParameter p)
        {
            quadSensors.imuNoiseSigma.z = p.Value;
        }

        public void OnGyroRateChanged(SimParameter p)
        {
            quadSensors.gyroRateHz = p.Value;
        }

        public void OnGyroSigmaXChanged(SimParameter p)
        {
            quadSensors.gyroNoiseSigma.x = p.Value;
        }

        public void OnGyroSigmaYChanged(SimParameter p)
        {
            quadSensors.gyroNoiseSigma.y = p.Value;
        }

        public void OnGyroSigmaZChanged(SimParameter p)
        {
            quadSensors.gyroNoiseSigma.z = p.Value;
        }

        public void OnCompassRateChanged(SimParameter p)
        {
            quadSensors.compassRateHz = p.Value;
        }

        public void OnCompassSigmaXChanged(SimParameter p)
        {
            quadSensors.compassNoiseSigma.x = p.Value;
        }

        public void OnCompassSigmaYChanged(SimParameter p)
        {
            quadSensors.compassNoiseSigma.y = p.Value;
        }

        public void OnCompassSigmaZChanged(SimParameter p)
        {
            quadSensors.compassNoiseSigma.z = p.Value;
        }

        public void OnBarometerRateChanged(SimParameter p)
        {
            quadSensors.barometerRateHz = p.Value;
        }

        public void OnBarometerSigmaChanged(SimParameter p)
        {
            quadSensors.barometerNoiseSigma = p.Value;
        }

        public void OnGpsRateChanged(SimParameter p)
        {
            quadSensors.gpsRateHz = p.Value;
        }

        public void OnGpsSigmaNChanged(SimParameter p)
        {
            quadSensors.gpsPositionNoiseSigma.x = p.Value;
        }

        public void OnGpsSigmaEChanged(SimParameter p)
        {
            quadSensors.gpsPositionNoiseSigma.y = p.Value;
        }

        public void OnGpsSigmaDChanged(SimParameter p)
        {
            quadSensors.gpsPositionNoiseSigma.z = p.Value;
        }

        public void OnGpsSigmaVNChanged(SimParameter p)
        {
            quadSensors.gpsVelocityNoiseSigma.x = p.Value;
        }

        public void OnGpsSigmaVEChanged(SimParameter p)
        {
            quadSensors.gpsVelocityNoiseSigma.y = p.Value;
        }

        public void OnGpsSigmaVDChanged(SimParameter p)
        {
            quadSensors.gpsVelocityNoiseSigma.z = p.Value;
        }

    }
}
