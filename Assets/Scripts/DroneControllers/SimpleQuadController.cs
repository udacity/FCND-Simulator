using UnityEngine;

namespace DroneControllers
{
    public class SimpleQuadController : MonoBehaviour
    {

        const float M2Latitude = 1.0f / 111111.0f;
        const float M2Longitude = 1.0f / (0.8f * 111111.0f);
        double latitude0 = 37.412939d;
        double longitude0 = 121.995635d;
        public Transform camTransform;
        public QuadController controller;
        // public FollowCamera followCam;
        //Vehicle status indicators
        public bool motors_armed = false;
        //Flight modes
        public bool guided = false;
        public bool stabilized = true;
        public bool posctl = true;
        //Control Gains
        public float Kp_hdot = 10.0f;
        public float Kp_r = 20.0f;
        public float Kp_roll = 6.5f;
        public float Kp_p = 10.0f;
        public float Kp_pitch = 6.5f;
        public float Kp_q = 10.0f;
        public float Kp_pos = 0.1f;
        public float Kp_vel = 0.3f;//-0.05f;
        public float Kd_vel = 0.0f;
        public float Kp_alt = 1.0f;
        public float Ki_hdot = 0.1f;
        //Vehicle control thresholds
        public float posctl_band = 0.1f;
        public float posHoldDeadband = 1.0f;
        public float moveSpeed = 10;
        // in radians
        public float turnSpeed = 2.0f;
        public float thrustForce = 25.0f;
        public float thrustMoment = 2.0f;
        // in radians
        public float maxTilt = 0.5f;
        public float maxAscentRate = 5.0f;
        public float maxDescentRate = 2.0f;
        Rigidbody rb;
        float tiltX;
        float tiltZ;

        private float h_des = 0.0f;



        private bool pos_set = false;
        Vector3 posHoldLocal = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 lastVelocityErrorBody = new Vector3(0.0f, 0.0f, 0.0f);
        float hDotInt = 0.0f;


        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            if (controller == null)
                controller = GetComponent<QuadController>();
            // if (followCam == null)
            //     followCam = camTransform.GetComponent<FollowCamera>();
            motors_armed = false;
        }

        void FixedUpdate()
        {
            moveSpeed = 15.0f;
            turnSpeed = 2.0f;
            maxTilt = 0.5f;
            if (Input.GetKeyDown(KeyCode.F12))
            {
                motors_armed = !motors_armed;

                if (motors_armed)
                {
                    controller.UseGravity = true;
                    controller.rb.isKinematic = false;
                    controller.rb.freezeRotation = false;
                    controller.rb.velocity = Vector3.zero;
                    controller.MotorsEnabled = true;
                }
                else
                {
                    controller.rb.freezeRotation = false;
                    controller.MotorsEnabled = false;
                }

            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                posctl = !posctl;

                pos_set = false;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                pos_set = false;
                controller.ResetOrientation();
                // followCam.ChangePoseType(CameraPoseType.Iso);
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                controller.UseGravity = !controller.UseGravity;
            }

            //TODO: Remove this, it is for testing the Guided Mode
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                guided = true;
                posHoldLocal.x = rb.position.x + 20.0f;
                posHoldLocal.y = rb.position.y + 5.0f;
                posHoldLocal.z = rb.position.z + 20.0f;
                pos_set = true;
                Debug.Log(posHoldLocal);
            }


            /*
            Vector3 rollYawPitch = controller.navTransform.eulerAngles*Mathf.PI/180.0f;
            for (int i = 0;i < 3; i++)
            {
                if (rollYawPitch[i] > Mathf.PI)
                    rollYawPitch[i] = rollYawPitch[i] - 2.0f*Mathf.PI;
                else if (rollYawPitch[i] < -Mathf.PI)
                    rollYawPitch[i] = rollYawPitch[i] + 2.0f*Mathf.PI;
            }
            */
            Vector3 rollYawPitch = controller.eulerAngles * Mathf.PI / 180.0f;
            Vector3 prq = controller.AngularVelocityBody;
            Vector3 prqRate = controller.AngularAccelerationBody;
            Vector3 localPosition = controller.GPS;
            Vector3 bodyVelocity = controller.BodyVelocity;

            localPosition.x = (localPosition.x) / M2Latitude;
            localPosition.z = (localPosition.z) / M2Longitude;


            //Direct Control of the moments
            var thrust = new Vector3(0.0f, 0.0f, 0.0f);
            var yaw_moment = new Vector3(0.0f, 0.0f, 0.0f);
            var pitch_moment = new Vector3(0.0f, 0.0f, 0.0f);
            var roll_moment = new Vector3(0.0f, 0.0f, 0.0f);
            var angle_input = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            if (motors_armed)
            {
                //Outer control loop for from a position/velocity command to a hdot, yaw rate, pitch, roll command
                if (posctl || guided)
                {
                    Vector3 velCmdBody = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Thrust"), -Input.GetAxis("Horizontal"));

                    float yawCmd = Input.GetAxis("Yaw");

                    //If no control input provided (or in guided mode), use position hold
                    if (guided || Mathf.Sqrt(Mathf.Pow(velCmdBody.x, 2.0f) + Mathf.Pow(velCmdBody.y, 2.0f) + Mathf.Pow(velCmdBody.z, 2.0f)) < posctl_band)
                    {

                        //Set position
                        if (!pos_set)
                        {
                            posHoldLocal = localPosition;
                            pos_set = true;
                            //                        Debug.Log(posHoldLocal);
                        }

                        Vector3 posErrorLocal = posHoldLocal - localPosition;
                        Vector3 velCmdLocal;

                        //Deadband around the position hold
                        if (Mathf.Sqrt(Mathf.Pow(posErrorLocal.x, 2.0f) + Mathf.Pow(posErrorLocal.z, 2.0f)) < posHoldDeadband)
                        {
                            velCmdLocal.x = 0.0f;
                            velCmdLocal.z = 0.0f;
                        }
                        else
                        {
                            velCmdLocal.x = Kp_pos * posErrorLocal.x;
                            velCmdLocal.z = Kp_pos * posErrorLocal.z;
                        }

                        velCmdLocal.y = Kp_alt * posErrorLocal.y;


                        //Rotate into the local heading frame
                        float cosYaw = Mathf.Cos(rollYawPitch.y);
                        float sinYaw = Mathf.Sin(rollYawPitch.y);
                        velCmdBody.x = cosYaw * velCmdLocal.x - sinYaw * velCmdLocal.z;
                        velCmdBody.z = sinYaw * velCmdLocal.x + cosYaw * velCmdLocal.z;

                        velCmdBody.y = velCmdLocal.y;

                    }
                    else
                    {
                        pos_set = false;
                    }


                    //Control loop from a body velocity command to a Hdot, yaw rate, pitch, and roll command
                    Vector3 velocityErrorBody = new Vector3(0.0f, 0.0f, 0.0f);
                    Vector3 velocityErrorBodyD = new Vector3(0.0f, 0.0f, 0.0f);
                    velocityErrorBody.x = moveSpeed * velCmdBody.x - bodyVelocity.x;
                    velocityErrorBody.z = moveSpeed * velCmdBody.z - bodyVelocity.z;
                    velocityErrorBodyD = (velocityErrorBody - lastVelocityErrorBody) / Time.deltaTime;
                    lastVelocityErrorBody = velocityErrorBody;

                    angle_input[2] = -Kp_vel * velocityErrorBody.x - Kd_vel * velocityErrorBodyD.x;
                    angle_input[3] = Kp_vel * velocityErrorBody.z + Kd_vel * velocityErrorBodyD.z;

                    float angle_magnitude = Mathf.Sqrt(Mathf.Pow(angle_input[2], 2.0f) + Mathf.Pow(angle_input[3], 2.0f));
                    if (angle_magnitude > maxTilt)
                    {
                        angle_input[2] = maxTilt * angle_input[2] / angle_magnitude;
                        angle_input[3] = maxTilt * angle_input[3] / angle_magnitude;
                    }


                    angle_input[0] = velCmdBody.y;
                    angle_input[1] = yawCmd;
                }
                else
                {
                    pos_set = false;

                    //Pilot Input: Hdot, Yawrate, pitch, roll
                    angle_input = new Vector4(Input.GetAxis("Thrust"), Input.GetAxis("Yaw"), -Input.GetAxis("Vertical") * maxTilt, -Input.GetAxis("Horizontal") * maxTilt);
                }


                //Constrain the angle inputs between -1 and 1 (tilt, turning speed, and vert speed taken into account later)
                for (int i = 1; i < 2; i++)
                {
                    if (angle_input[i] > 1.0f)
                        angle_input[i] = 1.0f;
                    else if (angle_input[i] < -1.0f)
                        angle_input[i] = -1.0f;
                }


                //Inner control loop: angle commands to forces
                if (guided || posctl || stabilized)
                {

                    float thrust_nom = -1.0f * rb.mass * Physics.gravity[1];
                    float hDotError = 0.0f;
                    if (angle_input[0] > 0.0f)
                    {
                        hDotError = (maxAscentRate * angle_input[0] - 1.0f * controller.LinearVelocity.y);
                    }
                    else
                    {
                        hDotError = (maxDescentRate * angle_input[0] - 1.0f * controller.LinearVelocity.y);
                    }
                    hDotInt = hDotInt + hDotError * Time.deltaTime;

                    //hdot to thrust
                    thrust[1] = (Kp_hdot * hDotError + Ki_hdot * hDotInt + thrust_nom) / (Mathf.Cos(rollYawPitch.x) * Mathf.Cos(rollYawPitch.z));

                    //yaw rate to yaw moment
                    yaw_moment[1] = Kp_r * (turnSpeed * angle_input[1] - prq[1]);


                    //angle to angular rate command (for pitch and roll)
                    float pitchError = angle_input[2] - rollYawPitch.z;
                    float rollError = angle_input[3] - rollYawPitch.x;
                    float pitchRateError = Kp_pitch * pitchError - prq.z;
                    float rollRateError = Kp_roll * rollError - prq.x;

                    //angular rate to moment (pitch and roll)
                    pitch_moment[2] = Kp_q * pitchRateError;
                    roll_moment[0] = Kp_p * rollRateError;




                }
                else //User controls forces directly (not updated, do not use)
                {
                    thrust = thrustForce * (new Vector3(0.0f, angle_input[0], 0.0f));
                    yaw_moment = thrustMoment * (new Vector3(0.0f, angle_input[1], 0.0f));
                    pitch_moment = thrustMoment * (new Vector3(angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f));
                    roll_moment = thrustMoment * (new Vector3(angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, -1.0f * angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f));
                }
                //rb.AddRelativeForce(thrust);
                //rb.AddRelativeTorque(yaw_moment + pitch_moment + roll_moment);
                Vector3 total_moment = yaw_moment + pitch_moment + roll_moment;

                controller.ApplyMotorForce(thrust);
                controller.ApplyMotorTorque(total_moment);
            }
            else
            {
                pos_set = false;
            }

        }

        //Command the quad to a GPS location (latitude, relative_altitude, longitude)
        public void CommandGPS(double latitude, double longitude, double altitude)
        {
            posHoldLocal.x = (float)(latitude - latitude0) / M2Latitude;
            posHoldLocal.y = (float)(altitude);
            posHoldLocal.z = (float)(-longitude - longitude0) / M2Longitude;
            pos_set = true;
        }

        public void ArmVehicle()
        {
            motors_armed = true;
        }

        public void DisarmVehicle()
        {
            motors_armed = false;
        }

        public void SetGuidedMode(bool input_guided)
        {
            guided = input_guided;
        }
    }
}