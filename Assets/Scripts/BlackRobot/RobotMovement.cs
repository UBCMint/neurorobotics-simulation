using UnityEngine;

public class RobotMovement : MonoBehaviour
{
    [SerializeField] private ArticulationBody leftFemur;
    [SerializeField] private ArticulationBody rightFemur;
    [SerializeField] private ArticulationBody leftTibia;
    [SerializeField] private ArticulationBody rightTibia;
    [SerializeField] private ArticulationBody leftFoot;
    [SerializeField] private ArticulationBody rightFoot;

    [SerializeField] private float torque = 50f;

    private void Start()
    {
        Debug.Log("=== Joint Rotation Axes ===");
        
        PrintRotationAxis("Left Femur", leftFemur);
        PrintRotationAxis("Right Femur", rightFemur);
        PrintRotationAxis("Left Tibia", leftTibia);
        PrintRotationAxis("Right Tibia", rightTibia);
        PrintRotationAxis("Left Foot", leftFoot);
        PrintRotationAxis("Right Foot", rightFoot);
        
        // Initialize drives
        InitializeDrive(leftFemur, 10000f, 100f);
        InitializeDrive(rightFemur, 10000f, 100f);
        InitializeDrive(leftTibia, 5000f, 100f);
        InitializeDrive(rightTibia, 5000f, 100f);
        InitializeDrive(leftFoot, 1000f, 100f);
        InitializeDrive(rightFoot, 1000f, 100f);
    }

    private void PrintRotationAxis(string name, ArticulationBody body)
    {
        if (body == null)
        {
            Debug.LogError($"{name} is NULL");
            return;
        }

        if (body.dofCount == 0)
        {
            Debug.LogWarning($"{name}: No rotation axis (DoF = 0)");
            return;
        }

        string axis = "";
        if (body.twistLock == ArticulationDofLock.LimitedMotion || body.twistLock == ArticulationDofLock.FreeMotion)
            axis = "X";
        else if (body.swingYLock == ArticulationDofLock.LimitedMotion || body.swingYLock == ArticulationDofLock.FreeMotion)
            axis = "Y";
        else if (body.swingZLock == ArticulationDofLock.LimitedMotion || body.swingZLock == ArticulationDofLock.FreeMotion)
            axis = "Z";

        Debug.Log($"{name}: Rotates on {axis}-axis");
    }

    private void InitializeDrive(ArticulationBody body, float stiffness, float damping)
    {
        if (body == null || body.dofCount == 0) return;

        if (body.twistLock == ArticulationDofLock.LimitedMotion || body.twistLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.xDrive;
            drive.driveType = ArticulationDriveType.Force;
            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.target = 0f;
        }
    }

    private void FixedUpdate()
    {
        // Left limbs
        if (Input.GetKey(KeyCode.W))
        {
            ApplyTorque(leftFemur, -torque);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            ApplyTorque(leftFemur, torque);
        }

        if (Input.GetKey(KeyCode.A))
        {
            ApplyTorque(leftTibia, torque);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            ApplyTorque(leftTibia, -torque);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            ApplyTorque(leftFoot, torque);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            ApplyTorque(leftFoot, -torque);
        }

        // Right limbs
        if (Input.GetKey(KeyCode.I))
        {
            ApplyTorque(rightFemur, torque);
        }
        else if (Input.GetKey(KeyCode.K))
        {
            ApplyTorque(rightFemur, -torque);
        }

        if (Input.GetKey(KeyCode.J))
        {
            ApplyTorque(rightTibia, -torque);
        }
        else if (Input.GetKey(KeyCode.L))
        {
            ApplyTorque(rightTibia, torque);
        }

        if (Input.GetKey(KeyCode.U))
        {
            ApplyTorque(rightFoot, -torque);
        }
        else if (Input.GetKey(KeyCode.O))
        {
            ApplyTorque(rightFoot, torque);
        }
    }

    private void ApplyTorque(ArticulationBody body, float torqueValue)
    {
        if (body == null || body.dofCount == 0) return;

        if (body.twistLock == ArticulationDofLock.LimitedMotion || body.twistLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.xDrive;
            drive.target = torqueValue;
            body.xDrive = drive;
        }

        if (body.swingYLock == ArticulationDofLock.LimitedMotion || body.swingYLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.yDrive;
            drive.target = torqueValue;
            body.yDrive = drive;
        }

        if (body.swingZLock == ArticulationDofLock.LimitedMotion || body.swingZLock == ArticulationDofLock.FreeMotion)
        {
            ArticulationDrive drive = body.zDrive;
            drive.target = torqueValue;
            body.zDrive = drive;
        }
    }
}