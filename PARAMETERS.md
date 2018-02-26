# Control Parameters
The Unity simulator includes configurable parameters for control system (control gains and thresholds).
To access them click the PARAMETERS buton the bottom of the simulator window. The following PARAMETERS window will pop up:
![Image of PARAMETERWINDOW](parameterWindow.png)
## Attitude Control 

### Control Gains
* rollrate_gain_P/pitchrate_gain_P/yawrate_gain_P: proportional gain on roll rate/pitch rate/yaw rate error respectively
* roll_gain_P/pitch_gain_P: proportional gain on roll/pitch angle error 
* hdot_gain_P: proportional gain on the vertical velocity error
* hdot_gain_I: integral gain on the vertical velocity error

### Other Parameters
* Max Ascent Rate: limit on upward vertical velocity (m/s)
* Max Descent Rate: limit on downward vertical velocity (m/s), usually lower than the ascent rate to prevent downwash effects
* Max Tilt: limit on the tilt angle (combined pitch/roll) of the quadcopter (radians)

## Position Control

### Control Gains
* yaw_gain_P: proportional gain on the yaw angle error
* position_gain_P/position_gain_P2: proportional gain on horizontal position error for large (position_gain_P) errors and small (position_gain_P2) errors. The threshold is set by Position Gain Radius
* velocity_gain_P: proportional gain on the horizontal velocity error

### Other Parameters
* Position Gain Radius: horizontal error to switch between position_gain_P and position_gain_P2  (m)
* Max Speed: limit on maximum horizontal speed (m/s)
