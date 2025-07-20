using UnityEngine;

public class KeyboardControl : MonoBehaviour
{

    public HingeJoint leftHipJoint;
    public HingeJoint leftKneeJoint;
    public HingeJoint leftAnkleJoint;

    public HingeJoint rightHipJoint;
    public HingeJoint rightKneeJoint;
    public HingeJoint rightAnkleJoint;


    public float motorForce = 1000f;
    public float motorSpeed = 100f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


    //*************************************LEFT LEG*************************************************
        // Create motor structs
        JointMotor leftHipMotor = leftHipJoint.motor;
        JointMotor leftKneeMotor = leftKneeJoint.motor;
        JointMotor leftAnkleMotor = leftAnkleJoint.motor;

        // HIP CONTROL (e.g., R/F)
        if (Input.GetKey(KeyCode.R))
        {
            leftHipMotor.targetVelocity = motorSpeed;
        }
        else if (Input.GetKey(KeyCode.F))
        {
            leftHipMotor.targetVelocity = -motorSpeed;
        }
        else
        {
            leftHipMotor.targetVelocity = 0f;
        }

        // KNEE CONTROL (e.g., T/G)
        if (Input.GetKey(KeyCode.T))
        {
            leftKneeMotor.targetVelocity = motorSpeed;
        }
        else if (Input.GetKey(KeyCode.G))
        {
            leftKneeMotor.targetVelocity = -motorSpeed;
        }
        else
        {
            leftKneeMotor.targetVelocity = 0f;
        }
        // ANKLE CONTROL (e.g., Y/H)
        if (Input.GetKey(KeyCode.Y))
        {
            leftAnkleMotor.targetVelocity = motorSpeed;
        }
        else if (Input.GetKey(KeyCode.H))
        {
            leftAnkleMotor.targetVelocity = -motorSpeed;
        }
        else
        {
            leftAnkleMotor.targetVelocity = 0f;
        }





        //**********************************RIGHT LEG***********************************************
        // Create motor structs
        JointMotor rightHipMotor = rightHipJoint.motor;
        JointMotor rightKneeMotor = rightKneeJoint.motor;
        JointMotor rightAnkleMotor = rightAnkleJoint.motor;

        // HIP CONTROL (e.g., U/J)
        if (Input.GetKey(KeyCode.U))
        {
            rightHipMotor.targetVelocity = motorSpeed;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            rightHipMotor.targetVelocity = -motorSpeed;
        }
        else
        {
            rightHipMotor.targetVelocity = 0f;
        }

        // KNEE CONTROL (e.g., I/K)
        if (Input.GetKey(KeyCode.I))
        {
            rightKneeMotor.targetVelocity = motorSpeed;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            rightKneeMotor.targetVelocity = -motorSpeed;
        }
        else
        {
            rightKneeMotor.targetVelocity = 0f;
        }

        // KNEE CONTROL (e.g., O/L)
        if (Input.GetKey(KeyCode.O))
        {
            rightAnkleMotor.targetVelocity = motorSpeed;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            rightAnkleMotor.targetVelocity = -motorSpeed;
        }
        else
        {
            rightAnkleMotor.targetVelocity = 0f;
        }






        //************************ Apply force and enable motors******************************************

        leftHipMotor.force = motorForce;
        leftHipJoint.motor = leftHipMotor;
        leftHipJoint.useMotor = true;

        leftKneeMotor.force = motorForce;
        leftKneeJoint.motor = leftKneeMotor;
        leftKneeJoint.useMotor = true;

        leftAnkleMotor.force = motorForce;
        leftAnkleJoint.motor = leftAnkleMotor;
        leftAnkleJoint.useMotor = true;



        rightHipMotor.force = motorForce;
        rightHipJoint.motor = rightHipMotor;
        rightHipJoint.useMotor = true;

        rightKneeMotor.force = motorForce;
        rightKneeJoint.motor = rightKneeMotor;
        rightKneeJoint.useMotor = true;

        rightAnkleMotor.force = motorForce;
        rightAnkleJoint.motor = rightAnkleMotor;
        rightAnkleJoint.useMotor = true;

    }
}





