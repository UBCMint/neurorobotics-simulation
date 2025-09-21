# neurorobotics-simulation



A real-time simulation environment for testing bipedal robot joint control using keyboard inputs and camera navigation in Unity.

---

## Controls

### Camera
- **W** / **A** / **S** / **D** – Move camera
- **Mouse Movement** – Look around

### Robot Joint Control

| Key        | Joint         | Action              |
|------------|---------------|---------------------|
| R / F      | Left Hip      | Forward / Backward  |
| T / G      | Left Knee     | Forward / Backward  |
| Y / H      | Left Ankle    | Forward / Backward  |
| U / J      | Right Hip     | Forward / Backward  |
| I / K      | Right Knee    | Forward / Backward  |
| O / L      | Right Ankle   | Forward / Backward  |

---


## Headset Functionality

September 20th:
Headset output is used to move a sphere object bi-directionally for demonstration purposes for now.  
How to run: Run neurofeedback.py in Assets/Python_Scripts with the BlueMuse software and then Play the simulation in Unity.   
Minor issues: The python script for the headset crashes after about 20 seconds.


## Global Variables

```csharp
// Camera settings
float cameraSpeed = 15.0f;
float mouseSensitivity = 2.0f;

// Joint control
Motor Force = 300.0f
Motor Speed = 30.0f
