using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class RobotMovement : MonoBehaviour
{
    [Header("Files")]
    [SerializeField] private TextAsset csvFile;

    [Header("Bodypart Joints")]
    [SerializeField] private ArticulationBody leftFemur;
    [SerializeField] private ArticulationBody rightFemur;
    [SerializeField] private ArticulationBody leftTibia;
    [SerializeField] private ArticulationBody rightTibia;
    [SerializeField] private ArticulationBody leftFoot;
    [SerializeField] private ArticulationBody rightFoot;

    [Header("Joint Configurations")]
    [SerializeField] private float targetAngle = 45f;
    [SerializeField] private float femurStiffness = 500f;
    [SerializeField] private float tibiaStiffness = 200f;
    [SerializeField] private float feetStiffness = 100f;
    [SerializeField] private float femurDamping = 100f; 
    [SerializeField] private float tibiaDamping = 80f;
    [SerializeField] private float feetDamping = 50f;
    [SerializeField] private float forceLimit = 100000f;

    [Header("Joint Resetting")]
    [SerializeField] private bool jointResetMode = false;

    [Header("Arduino ↔ Unity calibration (per joint)")]
    [Tooltip("Arduino angle that corresponds to 0 degrees in Unity for this joint")]
    [SerializeField] private float leftFemurArduinoAngleAtUnityZero = 0f;
    [SerializeField] private float rightFemurArduinoAngleAtUnityZero = 0f;
    [SerializeField] private float leftTibiaArduinoAngleAtUnityZero = 0f;
    [SerializeField] private float rightTibiaArduinoAngleAtUnityZero = 0f;
    [SerializeField] private float leftFootArduinoAngleAtUnityZero = 0f;
    [SerializeField] private float rightFootArduinoAngleAtUnityZero = 0f;
    [Tooltip("True = increasing Unity angle corresponds to increasing Arduino angle")]
    [SerializeField] private bool leftFemurUnityIncreaseMatchesArduino = true;
    [SerializeField] private bool rightFemurUnityIncreaseMatchesArduino = true;
    [SerializeField] private bool leftTibiaUnityIncreaseMatchesArduino = true;
    [SerializeField] private bool rightTibiaUnityIncreaseMatchesArduino = true;
    [SerializeField] private bool leftFootUnityIncreaseMatchesArduino = true;
    [SerializeField] private bool rightFootUnityIncreaseMatchesArduino = true;

    private Dictionary<string, ArticulationBody> jointDict;

    private void Start()
    {
        CreateJointDictionary();

        // Initialize drives for POSITION control
        InitializeDrive(leftFemur, femurStiffness, femurDamping);
        InitializeDrive(rightFemur, femurStiffness, femurDamping);
        InitializeDrive(leftTibia, tibiaStiffness, tibiaDamping);
        InitializeDrive(rightTibia, tibiaStiffness, tibiaDamping);
        InitializeDrive(leftFoot, feetStiffness, feetDamping);
        InitializeDrive(rightFoot, feetStiffness, feetDamping);

        StartCoroutine(processInputCSV());
    }

    private void CreateJointDictionary()
    {
        jointDict = new Dictionary<string, ArticulationBody>();
        jointDict["leftFemur"] = leftFemur;
        jointDict["rightFemur"] = rightFemur;
        jointDict["leftTibia"] = leftTibia;
        jointDict["rightTibia"] = rightTibia;
        jointDict["leftFoot"] = leftFoot;
        jointDict["rightFoot"] = rightFoot;
    }

    private void InitializeDrive(ArticulationBody body, float stiffness, float damping)
    {
        if (body == null || body.dofCount == 0) return;

        if (body.twistLock == ArticulationDofLock.LimitedMotion || body.twistLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.xDrive;
            drive.driveType = ArticulationDriveType.Target;
            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            drive.target = 0f; // degrees
            body.xDrive = drive;
        }

        if (body.swingYLock == ArticulationDofLock.LimitedMotion || body.swingYLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.yDrive;
            drive.driveType = ArticulationDriveType.Target;
            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            drive.target = 0f; // degrees
            body.yDrive = drive;
        }

        if (body.swingZLock == ArticulationDofLock.LimitedMotion || body.swingZLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.zDrive;
            drive.driveType = ArticulationDriveType.Target;
            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            drive.target = 0f; // degrees
            body.zDrive = drive;
        }
    }

    private void FixedUpdate()
    {
        // Left limbs
        if (Input.GetKey(KeyCode.W))
        {
            SetTargetAngle(leftFemur, -targetAngle);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            SetTargetAngle(leftFemur, targetAngle);
        }
        else if (jointResetMode)
        {
            SetTargetAngle(leftFemur, 0f);
        }

        if (Input.GetKey(KeyCode.A))
        {
            SetTargetAngle(leftTibia, targetAngle);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            SetTargetAngle(leftTibia, -targetAngle);
        }
        else if (jointResetMode)
        {
            SetTargetAngle(leftTibia, 0f);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            SetTargetAngle(leftFoot, targetAngle);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            SetTargetAngle(leftFoot, -targetAngle);
        }
        else if (jointResetMode)
        {
            SetTargetAngle(leftFoot, 0f);
        }

        // Right limbs
        if (Input.GetKey(KeyCode.I))
        {
            SetTargetAngle(rightFemur, targetAngle);
        }
        else if (Input.GetKey(KeyCode.K))
        {
            SetTargetAngle(rightFemur, -targetAngle);
        }
        else if (jointResetMode)
        {
            SetTargetAngle(rightFemur, 0f);
        }

        if (Input.GetKey(KeyCode.J))
        {
            SetTargetAngle(rightTibia, targetAngle);
        }
        else if (Input.GetKey(KeyCode.L))
        {
            SetTargetAngle(rightTibia, -targetAngle);
        }
        else if (jointResetMode)
        {
            SetTargetAngle(rightTibia, 0f);
        }

        if (Input.GetKey(KeyCode.U))
        {
            SetTargetAngle(rightFoot, targetAngle);
        }
        else if (Input.GetKey(KeyCode.O))
        {
            SetTargetAngle(rightFoot, -targetAngle);
        }
        else if (jointResetMode)
        {
            SetTargetAngle(rightFoot, 0f);
        }

        // Debug key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LogCurrentPositions();
        }
    }

    private IEnumerator processInputCSV()
    {
        // Split lines properly across operating systems
        string[] lines = csvFile.text.Split(
            new[] { "\r\n", "\n", "\r" },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        // Read headers
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            for (int j = 0; j < headers.Length; j++)
            {
                string headerVal = headers[j];
                string lineVal = values[j];
                if (!float.TryParse(lineVal, out float lineFloatRes))
                {
                    Debug.LogError("Non header values are not floats!");
                    yield return null;
                }

                if (jointDict == null)
                {
                    Debug.LogError("Joint Dictionary is not initialized!");
                }
                else if (headerVal == "wait")
                {
                    if (float.TryParse(headerVal, out float floatResult))
                    {
                        yield return new WaitForSeconds(floatResult / 1000.0f);
                    } else
                    {
                        yield return null;
                    }
                }
                else
                {
                    Debug.Log(headerVal + ": " + lineFloatRes);

                    SetTargetAngle(jointDict[headerVal], lineFloatRes);
                    yield return new WaitForSeconds(2.0f);
                }

            }
        }
    }

    private void SetTargetAngle(ArticulationBody body, float angleDegrees)
    {
        if (body == null || body.dofCount == 0) return;

        // drive.target expects DEGREES, not radians!
        if (body.twistLock == ArticulationDofLock.LimitedMotion || body.twistLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.xDrive;
            drive.driveType = ArticulationDriveType.Target;
            drive.target = angleDegrees;
            body.xDrive = drive;
        }

        if (body.swingYLock == ArticulationDofLock.LimitedMotion || body.swingYLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.yDrive;
            drive.driveType = ArticulationDriveType.Target;
            drive.target = angleDegrees;
            body.yDrive = drive;
        }

        if (body.swingZLock == ArticulationDofLock.LimitedMotion || body.swingZLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.zDrive;
            drive.driveType = ArticulationDriveType.Target;
            drive.target = angleDegrees;
            body.zDrive = drive;
        }
    }

    private void LogCurrentPositions()
    {
        Debug.Log("=== Current Joint Positions ===");
        LogJointPosition("Left Femur", leftFemur);
        LogJointPosition("Right Femur", rightFemur);
        LogJointPosition("Left Tibia", leftTibia);
        LogJointPosition("Right Tibia", rightTibia);
        LogJointPosition("Left Foot", leftFoot);
        LogJointPosition("Right Foot", rightFoot);
    }

    private void LogJointPosition(string name, ArticulationBody body)
    {
        if (body == null || body.dofCount == 0) return;

        if (body.jointPosition.dofCount > 0)
        {
            // jointPosition is in radians, convert to degrees for display
            float currentAngleDegrees = body.jointPosition[0] * Mathf.Rad2Deg;
            
            // drive.target is already in degrees
            float targetDegrees = 0f;
            if (body.twistLock == ArticulationDofLock.LimitedMotion || body.twistLock == ArticulationDofLock.FreeMotion)
                targetDegrees = body.xDrive.target;
            else if (body.swingYLock == ArticulationDofLock.LimitedMotion || body.swingYLock == ArticulationDofLock.FreeMotion)
                targetDegrees = body.yDrive.target;
            else if (body.swingZLock == ArticulationDofLock.LimitedMotion || body.swingZLock == ArticulationDofLock.FreeMotion)
                targetDegrees = body.zDrive.target;
            
            Debug.Log($"{name}: Current={currentAngleDegrees:F2}°, Target={targetDegrees:F2}°, Error={Mathf.Abs(targetDegrees - currentAngleDegrees):F2}°");
        }
    }

    /// <summary>
    /// Records the angle of all 6 joints (with respect to their set points) to a CSV over a given duration.
    /// </summary>
    /// <param name="filePath">Full path for the output CSV. If null or empty, uses Application.persistentDataPath + "/joint_positions_YYYYMMdd_HHmmss.csv"</param>
    /// <param name="durationSeconds">How long to record (simulation time).</param>
    /// <param name="recordIntervalSeconds">How often to record one frame (simulation time). e.g. 0.02 for 50 Hz.</param>
    public void StoreJointPosition(string filePath, float durationSeconds, float recordIntervalSeconds)
    {
        if (durationSeconds <= 0f || recordIntervalSeconds <= 0f)
        {
            Debug.LogWarning("StoreJointPosition: duration and recordInterval must be positive.");
            return;
        }
        StartCoroutine(StoreJointPositionCoroutine(filePath, durationSeconds, recordIntervalSeconds));
    }

    private IEnumerator StoreJointPositionCoroutine(string filePath, float durationSeconds, float recordIntervalSeconds)
    {
        if (string.IsNullOrEmpty(filePath))
            filePath = Path.Combine(Application.persistentDataPath, $"joint_positions_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        float simulationSpeed = Time.timeScale;
        float recordingFrequencyHz = 1f / recordIntervalSeconds;

        var lines = new List<string>();
        // Metadata
        lines.Add("# joint_position_recording");
        lines.Add($"# simulation_speed,{simulationSpeed.ToString(CultureInfo.InvariantCulture)}");
        lines.Add($"# record_interval_sec,{recordIntervalSeconds.ToString(CultureInfo.InvariantCulture)}");
        lines.Add($"# record_frequency_hz,{recordingFrequencyHz.ToString(CultureInfo.InvariantCulture)}");
        lines.Add($"# duration_sec,{durationSeconds.ToString(CultureInfo.InvariantCulture)}");
        lines.Add("# frame = 0-based index from start of recording; angles = degrees relative to 0° (current joint angle)");
        lines.Add("frame,leftFemur,rightFemur,leftTibia,rightTibia,leftFoot,rightFoot");

        int frame = 0;
        float elapsed = 0f;

        while (elapsed < durationSeconds)
        {
            float lf = GetAngleDegrees(leftFemur);
            float rf = GetAngleDegrees(rightFemur);
            float lt = GetAngleDegrees(leftTibia);
            float rt = GetAngleDegrees(rightTibia);
            float lfoot = GetAngleDegrees(leftFoot);
            float rfoot = GetAngleDegrees(rightFoot);

            lines.Add(string.Format(CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4},{5},{6}",
                frame, lf, rf, lt, rt, lfoot, rfoot));

            frame++;
            elapsed += recordIntervalSeconds;
            yield return new WaitForSeconds(recordIntervalSeconds);
        }

        try
        {
            File.WriteAllLines(filePath, lines);
            Debug.Log($"StoreJointPosition: wrote {frame} frames to {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"StoreJointPosition: failed to write CSV: {e.Message}");
        }
    }

    /// <summary>
    /// Returns current joint angle in degrees.
    /// </summary>
    private float GetAngleDegrees(ArticulationBody body)
    {
        if (body == null || body.dofCount == 0 || body.jointPosition.dofCount == 0)
            return float.NaN;

        float currentDegrees = body.jointPosition[0] * Mathf.Rad2Deg;
        return currentDegrees;
    }

    /// <summary>
    /// Converts an angle from Arduino space to Unity space for the given joint.
    /// Uses per-joint calibration: Arduino angle at Unity zero and direction flag (whether Unity increase matches Arduino increase).
    /// Motion in Unity is along a single DOF (X, Y, or Z) determined by the joint's articulation body.
    /// </summary>
    /// <param name="jointName">leftFemur, rightFemur, leftTibia, rightTibia, leftFoot, rightFoot</param>
    /// <param name="arduinoAngleDegrees">Angle in Arduino degrees</param>
    /// <returns>Angle in Unity degrees, or float.NaN if joint not found</returns>
    public float ArduinoToUnityAngle(string jointName, float arduinoAngleDegrees)
    {
        if (!GetArduinoCalibration(jointName, out float arduinoAngleAtUnityZero, out bool unityIncreaseMatchesArduino))
            return float.NaN;
        float sign = unityIncreaseMatchesArduino ? 1f : -1f;
        return (arduinoAngleDegrees - arduinoAngleAtUnityZero) * sign;
    }

    /// <summary>
    /// Converts an angle from Unity space to Arduino space for the given joint.
    /// </summary>
    public float UnityToArduinoAngle(string jointName, float unityAngleDegrees)
    {
        if (!GetArduinoCalibration(jointName, out float arduinoAngleAtUnityZero, out bool unityIncreaseMatchesArduino))
            return float.NaN;
        float sign = unityIncreaseMatchesArduino ? 1f : -1f;
        return arduinoAngleAtUnityZero + unityAngleDegrees * sign;
    }

    /// <summary>
    /// Returns which axis (X, Y, or Z) the joint's motion is along in Unity (single DOF).
    /// </summary>
    public string GetJointMotionAxis(string jointName)
    {
        if (jointDict == null || !jointDict.TryGetValue(jointName, out ArticulationBody body) || body == null)
            return null;
        if (body.twistLock == ArticulationDofLock.LimitedMotion || body.twistLock == ArticulationDofLock.FreeMotion)
            return "X";
        if (body.swingYLock == ArticulationDofLock.LimitedMotion || body.swingYLock == ArticulationDofLock.FreeMotion)
            return "Y";
        if (body.swingZLock == ArticulationDofLock.LimitedMotion || body.swingZLock == ArticulationDofLock.FreeMotion)
            return "Z";
        return null;
    }

    private bool GetArduinoCalibration(string jointName, out float arduinoAngleAtUnityZero, out bool unityIncreaseMatchesArduino)
    {
        arduinoAngleAtUnityZero = 0f;
        unityIncreaseMatchesArduino = true;

        switch (jointName)
        {
            case "leftFemur":
                arduinoAngleAtUnityZero = leftFemurArduinoAngleAtUnityZero; unityIncreaseMatchesArduino = leftFemurUnityIncreaseMatchesArduino; return true;
            case "rightFemur":
                arduinoAngleAtUnityZero = rightFemurArduinoAngleAtUnityZero; unityIncreaseMatchesArduino = rightFemurUnityIncreaseMatchesArduino; return true;
            case "leftTibia":
                arduinoAngleAtUnityZero = leftTibiaArduinoAngleAtUnityZero; unityIncreaseMatchesArduino = leftTibiaUnityIncreaseMatchesArduino; return true;
            case "rightTibia":
                arduinoAngleAtUnityZero = rightTibiaArduinoAngleAtUnityZero; unityIncreaseMatchesArduino = rightTibiaUnityIncreaseMatchesArduino; return true;
            case "leftFoot":
                arduinoAngleAtUnityZero = leftFootArduinoAngleAtUnityZero; unityIncreaseMatchesArduino = leftFootUnityIncreaseMatchesArduino; return true;
            case "rightFoot":
                arduinoAngleAtUnityZero = rightFootArduinoAngleAtUnityZero; unityIncreaseMatchesArduino = rightFootUnityIncreaseMatchesArduino; return true;
            default:
                return false;
        }
    }
}