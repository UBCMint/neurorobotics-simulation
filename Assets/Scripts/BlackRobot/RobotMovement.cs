using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float jointSpeed = 100f;
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

        float step = jointSpeed * Time.fixedDeltaTime;

        // drive.target expects DEGREES, not radians!
        if (body.twistLock == ArticulationDofLock.LimitedMotion || body.twistLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.xDrive;
            drive.driveType = ArticulationDriveType.Target;
            drive.target = Mathf.MoveTowards(drive.target, angleDegrees, step);
            body.xDrive = drive;
        }

        if (body.swingYLock == ArticulationDofLock.LimitedMotion || body.swingYLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.yDrive;
            drive.driveType = ArticulationDriveType.Target;
            drive.target = Mathf.MoveTowards(drive.target, angleDegrees, step);
            body.yDrive = drive;
        }

        if (body.swingZLock == ArticulationDofLock.LimitedMotion || body.swingZLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.zDrive;
            drive.driveType = ArticulationDriveType.Target;
            drive.target = Mathf.MoveTowards(drive.target, angleDegrees, step);
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
}