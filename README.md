# FCND Simulator

## Download and Launch the Simulator
Download the simulator build that's appropriate for your operating system from the [releases tab](https://github.com/udacity/FCND-Simulator-Releases/releases).

Unzip the compressed file and launch the simulator by double clicking on the app / executable file.

## Simulator Controls

To manually control the drone, you need to first arm the motors.  You can do this by clicking the button labeled "DISARMED" in the display window. When the motors are armed, the rotors of the drone will start spinning. You can manually control the vehicle using the buttons described below:

* **Up/Down Arrow or W/S**: move forward/backward
* **Left/Right Arrow or A/D**: move left/right
* **Q/E**: turn heading left/right
* **Space/C**: move upward/downward
* **P**: Change manual mode (position control or stabilized)
* **Shift-R**: Reset the scene.
* **Shift-S**: Save colliders file. Only applicable in `3D Motion Planning` project.
* **Shift-C**: Toggle controls menu overlay. Applicable in all projects.
* **Esc**: Exit to main menu. Applicable in all projects.

**The following may not be up to date.**

### Control Modes
There are three control modes current setup for the simulated drone. Two of the modes are meant for manual control and one is only meant for autonomous control via MAVLink commands. You can switch between the two manual modes and notice a difference in how the vehicle flies:

#### Manual Stabilized Mode:
This mode is meant for manual control of the vehicle. Commanding the vehicle forward/backward and left/right controls the pitch/roll angle of the vehicle. The vehicle tilt is controlled to a maximum angle. The heading left/right controls the heading velocity (to a maximum rate). The upward/downward commands the vehicle's vertical velocity. Since there is no control loop on the position, the drone will drift around slightly without any control added.

#### Manual Position Control Mode:
This mode is meant for manual control of the vehicle. Commanding the vehicle forward/backward, left/right, and updward/downward controls the velocity vector of the vehicle. This velocity vector is with respect to the vehicle's heading, which is controlled similarly to Stabilized mode (controlling the heading rate). If a zero velocity vector is commanded (no control), the vehicle will hold its position and thus will not drift from its current location.

#### Autonomous Guided Mode:
This mode is meant for autonomous control of the drone. The drone is guided to the position commanded by the Python script. The heading of the vehicle can still be commanded manually similar to Stabilized Mode.
