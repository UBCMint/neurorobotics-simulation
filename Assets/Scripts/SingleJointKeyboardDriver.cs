// SingleJointKeyboardDriver.cs
using Unity.MINTNeurorobotics;
using UnityEngine;

public class SingleJointKeyboardDriver : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Your JointDriveControllerHinge in the scene.")]
    public JointDriveControllerHinge controller;

    [Tooltip("The Transform for the body part that has the HingeJoint(s).")]
    public Transform bodyPartTransform;

    [Header("Controls (avoid WASDQE)")]
    public KeyCode angleIncreaseKey = KeyCode.C;   // toward joint.limits.max
    public KeyCode angleDecreaseKey = KeyCode.V;   // toward joint.limits.min
    public KeyCode strengthIncreaseKey = KeyCode.Z;        // raise motor 'force' cap
    public KeyCode strengthDecreaseKey = KeyCode.X;        // lower motor 'force' cap

    [Header("Command Values")]
    [Range(-1f, 1f)] public float normalizedAngle = 0f;    // -1..1 mapped to joint limits
    [Range(-1f, 1f)] public float normalizedStrength = 0.5f; // -1..1 mapped to 0..maxForceLimit

    [Header("Step Sizes")]
    [Tooltip("How fast the angle command changes per second when holding keys.")]
    public float angleStepPerSecond = 0.8f;
    [Tooltip("How fast the strength command changes per second when holding keys.")]
    public float strengthStepPerSecond = 0.8f;

    [Header("Smoothing (optional)")]
    public bool smoothAngles = true;
    public float angleLerpSpeed = 6f;

    // internal
    private float _smoothedAngle;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<JointDriveControllerHinge>();
            if (controller == null)
            {
                Debug.LogError("SingleJointKeyboardDriver: No JointDriveControllerHinge assigned or found on this GameObject.");
                enabled = false;
                return;
            }
        }

        if (bodyPartTransform == null)
        {
            Debug.LogError("SingleJointKeyboardDriver: Please assign bodyPartTransform (the limb/segment with the HingeJoint).");
            enabled = false;
            return;
        }

        // Make sure the controller knows about this body part.
        if (!controller.bodyPartsDict.ContainsKey(bodyPartTransform))
        {
            controller.SetupBodyPart(bodyPartTransform);
        }

        _smoothedAngle = normalizedAngle;
    }

    void Update()
    {
        // --- Read input & update normalized commands ---
        float dt = Time.deltaTime;

        // Angle: Up/Down Arrow -> [-1, 1]
        if (Input.GetKey(angleIncreaseKey))
            normalizedAngle = Mathf.Clamp(normalizedAngle + angleStepPerSecond * dt, -1f, 1f);
        if (Input.GetKey(angleDecreaseKey))
            normalizedAngle = Mathf.Clamp(normalizedAngle - angleStepPerSecond * dt, -1f, 1f);

        // Optional smoothing for nicer motion
        _smoothedAngle = smoothAngles
            ? Mathf.Lerp(_smoothedAngle, normalizedAngle, 1f - Mathf.Exp(-angleLerpSpeed * dt))
            : normalizedAngle;

        // Strength: Z/X -> [-1, 1] (your code maps to 0..maxForceLimit)
        if (Input.GetKey(strengthIncreaseKey))
            normalizedStrength = Mathf.Clamp(normalizedStrength + strengthStepPerSecond * dt, -1f, 1f);
        if (Input.GetKey(strengthDecreaseKey))
            normalizedStrength = Mathf.Clamp(normalizedStrength - strengthStepPerSecond * dt, -1f, 1f);

        // --- Apply to the single body part using your existing API ---
        if (controller.bodyPartsDict.TryGetValue(bodyPartTransform, out var bp))
        {
            bp.SetJointTargetRotation(_smoothedAngle);
            bp.SetJointStrength(normalizedStrength);
        }
    }

    void OnGUI()
    {
        // Tiny on-screen readout
        GUI.Label(new Rect(10, 10, 600, 20),
            $"Joint: {bodyPartTransform.name} | Angle: {normalizedAngle:F2} | Strength: {normalizedStrength:F2}");
    }
}
