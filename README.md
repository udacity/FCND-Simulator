# FCND Simulator
## Backyard Flyer Drone controls

To manually control the drone, you need to first arm the motors. When the motors are armed, the telemetry data will both display "Local input on" and the rotors of the Drone will be spinning. The vehicle is manually controlled using the buttons described below:

* **arrow up/down or w/s**: move forward/backward
* **arrow left/right or a/d**: move left/right
* **q/e**: turn heading left/right
* **space/c**: move upward/downward
* **P**: Change manual mode (position control or stabilized)
* **Shift-R**: Reset the scene.
* **Escape**: Exit to main menu.

### Control Modes
There are three control modes current sit up for the simulated drone. Two of the modes are meant for manual control and one is only meant for control via Mavlink commands. You can switch between the two manual modes and notice a difference in how the vehicle flies:

#### Stabilized Mode:
This mode is meant for manual control of the vehicle. Commanding the vehicle forward/backward and left/right controls the pitch/roll angle of the vehicle. The vehicle tilt is controlled to a maximum angle. The heading left/right controls the heading velocity (to a maximum rate). The upward/downward commands the vehicle's vertical velocity. Since there is no control loop on the position, the drone will drift around slightly without any control added.

#### Position Control Mode:
This mode is meant for manual control of the vehicle. Commanding the vehicle forward/backward, left/right, and updward/downward controls the velocity vector of the vehicle. This velocity vector is with respect to the vehicle's heading, which is controlled similarly to Stabilized mode (controlling the heading rate). If a zero velocity vector is commanded (no control), the vehicle will hold its position and thus will not drift from its current location.

#### Guided Mode:
This mode is meant for autonomous control of the drone. The drone is guided to the position commanded by the Python script. The heading of the vehicle can still be commanded manually similar to Stabilized Mode.
