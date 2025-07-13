using UnityEngine;


public class LeftLeftControl : MonoBehaviour
{
    public HingeJoint hipJoint;
    public HingeJoint kneeJoint;


    public float motorForce = 5000f;
    public float motorSpeed = 500f;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Create motor structs
        JointMotor hipMotor = hipJoint.motor;
        JointMotor kneeMotor = kneeJoint.motor;

        // HIP CONTROL (e.g., A/D)
        if (Input.GetKey(KeyCode.R))
        {
            hipMotor.targetVelocity = motorSpeed;
        }
        else if (Input.GetKey(KeyCode.F))
        {
            hipMotor.targetVelocity = -motorSpeed;
        }
        else
        {
            hipMotor.targetVelocity = 0f;
        }

        // KNEE CONTROL (e.g., W/S)
        if (Input.GetKey(KeyCode.T))
        {
            kneeMotor.targetVelocity = motorSpeed;
        }
        else if (Input.GetKey(KeyCode.G))
        {
            kneeMotor.targetVelocity = -motorSpeed;
        }
        else
        {
            kneeMotor.targetVelocity = 0f;
        }

        // Apply force and enable motors
        hipMotor.force = motorForce;
        hipJoint.motor = hipMotor;
        hipJoint.useMotor = true;

        kneeMotor.force = motorForce;
        kneeJoint.motor = kneeMotor;
        kneeJoint.useMotor = true;

    }
}
