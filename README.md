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

## Global Variables

```csharp
// Camera settings
float cameraSpeed = 15.0f;
float mouseSensitivity = 2.0f;

// Joint control
Motor Force = 300.0f
Motor Speed = 30.0f




