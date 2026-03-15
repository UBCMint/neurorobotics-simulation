using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.BlackRobot
{
    public class ArticulationLimb : MonoBehaviour
    {

        [SerializeField] private ArticulationBody limb;
        
        [Header("Randomization Settings")]
        [SerializeField, Range(0f, 180f)] private float randomizationRange = 45f;
        [SerializeField] bool xDriveAvailable = true;
        [SerializeField] bool yDriveAvailable = false;
        [SerializeField] bool zDriveAvailable = false;
        [SerializeField] float jointSpeed = 100.0f;

        [Header("Joint Settings")]
        [SerializeField] float forceLimit = 10000.0f;
        [SerializeField] float stiffness = 100.0f;
        [SerializeField] float damping = 50.0f;


        private void Start()
        {
            if (!limb)
            {
                limb = GetComponent<ArticulationBody>();
            }
            InitializeDrive(); // Initialize drives first
            randomizeLimbPosition();  // Then set their target angles
        }

        public void randomizeLimbPosition() {
            float xAngle = Random.Range(xDriveAvailable ? -randomizationRange : 0, xDriveAvailable ? randomizationRange : 0);
            float yAngle = Random.Range(yDriveAvailable ? -randomizationRange : 0, yDriveAvailable ? randomizationRange : 0);
            float zAngle = Random.Range(zDriveAvailable ? -randomizationRange : 0, zDriveAvailable ? randomizationRange : 0);

            if (xDriveAvailable) {
                ArticulationDrive drive = limb.xDrive;
                drive.target = xAngle;
                limb.xDrive = drive;
            }

            if (yDriveAvailable) {
                ArticulationDrive drive = limb.yDrive;
                drive.target = yAngle;
                limb.yDrive = drive;
            }

            if (zDriveAvailable) {
                ArticulationDrive drive = limb.zDrive;
                drive.target = zAngle;
                limb.zDrive = drive;
            }

            // Immediately set the joint position (in radians) to avoid it springing to the target over time
            List<float> jointPositions = new List<float>();
            if (xDriveAvailable) jointPositions.Add(xAngle * Mathf.Deg2Rad);
            if (yDriveAvailable) jointPositions.Add(yAngle * Mathf.Deg2Rad);
            if (zDriveAvailable) jointPositions.Add(zAngle * Mathf.Deg2Rad);
            
            if (jointPositions.Count == 1)
            {
                limb.jointPosition = new ArticulationReducedSpace(jointPositions[0]);
            }
            else if (jointPositions.Count == 2)
            {
                limb.jointPosition = new ArticulationReducedSpace(jointPositions[0], jointPositions[1]);
            }
            else if (jointPositions.Count == 3)
            {
                limb.jointPosition = new ArticulationReducedSpace(jointPositions[0], jointPositions[1], jointPositions[2]);
            }

            Debug.Log($"{gameObject.name}'s new drive targets -> X: {xAngle}, Y: {yAngle}, Z: {zAngle}");
        }
        private void InitializeDrive()
        {
            ArticulationBody body = GetComponent<ArticulationBody>();
            if (body == null || body.dofCount == 0) return;

            if (body.twistLock == ArticulationDofLock.LimitedMotion || body.twistLock == ArticulationDofLock.FreeMotion)
            {
                ArticulationDrive drive = body.xDrive;
                drive.driveType = ArticulationDriveType.Target;
                drive.stiffness = stiffness;
                drive.damping = damping;
                drive.forceLimit = forceLimit;
                //drive.target = 0f; // degrees
                body.xDrive = drive;
            }

            if (body.swingYLock == ArticulationDofLock.LimitedMotion || body.swingYLock == ArticulationDofLock.FreeMotion)
            {
                ArticulationDrive drive = body.yDrive;
                drive.driveType = ArticulationDriveType.Target;
                drive.stiffness = stiffness;
                drive.damping = damping;
                drive.forceLimit = forceLimit;
                //drive.target = 0f; // degrees
                body.yDrive = drive;
            }

            if (body.swingZLock == ArticulationDofLock.LimitedMotion || body.swingZLock == ArticulationDofLock.FreeMotion)
            {
                ArticulationDrive drive = body.zDrive;
                drive.driveType = ArticulationDriveType.Target;
                drive.stiffness = stiffness;
                drive.damping = damping;
                drive.forceLimit = forceLimit;
                //drive.target = 0f; // degrees
                body.zDrive = drive;
            }
        }

        public void MoveLimbToPosition(float xTargetAngle = 0.0f, float yTargetAngle = 0.0f, float zTargetAngle = 0.0f) {

            float step = jointSpeed * Time.fixedDeltaTime;

            if (!limb) {
                Debug.LogError("The limb is not initialized!");
                return;
            }

            if (xDriveAvailable) {
                ArticulationDrive drive = limb.xDrive;
                drive.driveType = ArticulationDriveType.Target;
                drive.target = Mathf.MoveTowards(drive.target, xTargetAngle, step);
                limb.xDrive = drive;
            }
            if (yDriveAvailable) {
                ArticulationDrive drive = limb.yDrive;
                drive.driveType = ArticulationDriveType.Target;
                drive.target = Mathf.MoveTowards(drive.target, yTargetAngle, step);
                limb.yDrive = drive;
            }
            if (zDriveAvailable)
            {
                ArticulationDrive drive = limb.zDrive;
                drive.driveType = ArticulationDriveType.Target;
                drive.target = Mathf.MoveTowards(drive.target, zTargetAngle, step);
                limb.zDrive = drive;
            }
        }
    }
}