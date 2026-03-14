using UnityEngine;

/// <summary>
/// PID controller that applies direct torque to a hinge joint to reach a target angle.
/// Attach to the same GameObject as the HingeJoint. Disables the joint's spring while active.
/// </summary>
[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(Rigidbody))]
public class JointPidController : MonoBehaviour
{
    [Header("PID Gains (tune per joint)")]
    [Tooltip("Proportional gain: torque per degree of error.")]
    public float Kp = 100f;
    [Tooltip("Integral gain: reduces steady-state error.")]
    public float Ki = 10f;
    [Tooltip("Derivative gain: dampens velocity (in deg/s).")]
    public float Kd = 5f;

    [Header("Limits")]
    [Tooltip("Maximum torque magnitude applied (N·m).")]
    public float maxTorque = 1000f;
    [Tooltip("Integral term clamp for anti-windup.")]
    public float maxIntegral = 500f;

    [Header("Target (set by motion script or other driver)")]
    [Tooltip("Target angle in degrees (clamped to joint limits at runtime).")]
    public float targetDegrees;

    // TODO: REMOVE DEBUG
    [Header("Print for debug")]
    [Tooltip("Whether we print for debugging.")]
    public bool printForDebug = false;
    // TODO: REMOVE DEBUG END

    HingeJoint _joint;
    Rigidbody _rb;
    float _integral;

    void Awake()
    {
        _joint = GetComponent<HingeJoint>();
        _rb = GetComponent<Rigidbody>();
        if (_joint == null || _rb == null)
        {
            Debug.LogError("JointPidController: HingeJoint and Rigidbody required.", this);
            enabled = false;
        }
    }

    void OnEnable()
    {
        _integral = 0f;
        if (_joint != null)
        {
            _joint.useSpring = false;
            _joint.useMotor = false;
        }
    }

    void FixedUpdate()
    {
        if (_joint == null || _rb == null) return;

        JointLimits limits = _joint.limits;
        float min = limits.min;
        float max = limits.max;

        // Seems to be a bug where, in early simulation, _joint.angle is nan
        // Will thus ignore updates while this is the case
        float currentDeg = _joint.angle;
        if (float.IsNaN(currentDeg)) return; 
        
        float targetDeg = Mathf.Clamp(targetDegrees, min, max);
        float errorDeg = targetDeg - currentDeg;

        Vector3 axisWorld = transform.TransformDirection(_joint.axis);
        float angularVelRadPerSec = Vector3.Dot(_rb.angularVelocity, axisWorld);
        float angularVelDegPerSec = angularVelRadPerSec * Mathf.Rad2Deg;

        float p = Kp * errorDeg;
        _integral += Ki * errorDeg * Time.fixedDeltaTime;
        _integral = Mathf.Clamp(_integral, -maxIntegral, maxIntegral);
        float d = -Kd * angularVelDegPerSec;

        float torque = p + _integral + d;
        torque = Mathf.Clamp(torque, -maxTorque, maxTorque);

        // // TODO REMOVE DEBUG
        // if (printForDebug)
        // {
        //     print($"Joint limits: {max} to {min}!");
        //     print($"Current Deg: {currentDeg}, target Deg: {targetDeg}, error Deg: {errorDeg}!");
        //     print($"Angular Velocity Deg/Sec: {angularVelDegPerSec}!");
        //     print($"p: {p}, _integral: {_integral}, d: {d}, torque: {torque}!");
        //     print($"Ki: {Ki}, Time.fixedDeltaTime: {Time.fixedDeltaTime}");
        // }
        // // TODO REMOVE DEBUG END

        _rb.AddTorque(axisWorld * torque);
    }

    /// <summary>
    /// Set target angle in degrees. Clamped to joint limits when applied.
    /// </summary>
    public void SetTargetFromDegrees(float degrees)
    {
        if (_joint != null)
            targetDegrees = Mathf.Clamp(degrees, _joint.limits.min, _joint.limits.max);
        else
            targetDegrees = degrees;
    }
}
