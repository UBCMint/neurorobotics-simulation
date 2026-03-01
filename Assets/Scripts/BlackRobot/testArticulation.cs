using System;
using UnityEngine;

public class testArticulation : MonoBehaviour
{
    [SerializeField] private ArticulationBody part1;
    [SerializeField] private ArticulationBody part2;

    [SerializeField] private float torque = 50f;
    [SerializeField] private float stiffness = 100.0f;
    [SerializeField] private float damping = 100.0f;

    private void Start()
    {
        // Diagnose the joint setup
        if (part1 != null)
        {
            Debug.Log($"Part1: {part1.name}, DoF: {part1.dofCount}, " +
                      $"X:{part1.twistLock}, Y:{part1.swingYLock}, Z:{part1.swingZLock}");
            
            // Initialize the drive
            if (part1.dofCount > 0)
            {
                if (part1.twistLock == ArticulationDofLock.LimitedMotion || part1.twistLock == ArticulationDofLock.FreeMotion)
                {
                    ArticulationDrive drive = part1.xDrive;
                    drive.driveType = ArticulationDriveType.Force;
                    drive.stiffness = stiffness;
                    drive.damping = 10f;
                    drive.target = 0f;
                    part1.xDrive = drive;
                    Debug.Log("Initialized X drive");
                }
                
                if (part1.swingYLock == ArticulationDofLock.LimitedMotion || part1.swingYLock == ArticulationDofLock.FreeMotion)
                {
                    ArticulationDrive drive = part1.yDrive;
                    drive.driveType = ArticulationDriveType.Force;
                    drive.stiffness = stiffness;
                    drive.damping = 10f;
                    drive.target = 0f;
                    part1.yDrive = drive;
                    Debug.Log("Initialized Y drive");
                }
                
                if (part1.swingZLock == ArticulationDofLock.LimitedMotion || part1.swingZLock == ArticulationDofLock.FreeMotion)
                {
                    ArticulationDrive drive = part1.zDrive;
                    drive.driveType = ArticulationDriveType.Force;
                    drive.stiffness = stiffness;
                    drive.damping = 10f;
                    drive.target = 0f;
                    part1.zDrive = drive;
                    Debug.Log("Initialized Z drive");
                }
            }
            else
            {
                Debug.LogError("Part1 has DoF = 0! Unlock an axis in the Inspector.");
            }
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.B))
        {
            Debug.Log("Moving the joint with torque: " + torque);
            ApplyTorqueToAllAxes(part1, torque);
        }
        else if (Input.GetKey(KeyCode.N))
        {
            Debug.Log("Moving the joint reverse with torque: " + -torque);
            ApplyTorqueToAllAxes(part1, -torque);
        }
        else
        {
            ApplyTorqueToAllAxes(part1, 0f);
        }
    }

    private void ApplyTorqueToAllAxes(ArticulationBody body, float torqueValue)
    {
        if (body == null || body.dofCount == 0) return;

        // Apply to whichever axis is unlocked
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

