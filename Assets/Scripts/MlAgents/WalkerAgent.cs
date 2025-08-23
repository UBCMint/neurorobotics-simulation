using System;
using Unity.MINTNeurorobotics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using BodyPartHinge = Unity.MINTNeurorobotics.BodyPartHinge;
using Random = UnityEngine.Random;

public class WalkerAgent : Agent
{
    [Header("Walk Speed")]
    [Range(0.1f, 10)]
    [SerializeField]
    //The walking speed to try and achieve
    private float m_TargetWalkingSpeed = 10;

    public float MTargetWalkingSpeed // property
    {
        get { return m_TargetWalkingSpeed; }
        set { m_TargetWalkingSpeed = Mathf.Clamp(value, .1f, m_maxWalkingSpeed); }
    }

    const float m_maxWalkingSpeed = 10; //The max walking speed

    //Should the agent sample a new goal velocity each episode?
    //If true, walkSpeed will be randomly set between zero and m_maxWalkingSpeed in OnEpisodeBegin()
    //If false, the goal velocity will be walkingSpeed
    public bool randomizeWalkSpeedEachEpisode;

    //The direction an agent will walk during training.
    private Vector3 m_WorldDirToWalk = Vector3.right;

    [Header("Target To Walk Towards")] public Transform target; //Target the agent will walk towards during training.

    [Header("Body Parts")]
    public Transform pelvisSlide;
    public Transform pelvis;
    public Transform leftFemur;
    public Transform rightFemur;
    public Transform rightKnee;
    public Transform leftKnee;
    public Transform leftBatteryHolder;
    public Transform rightBatterHolder;
    public Transform rightArm;
    public Transform leftArm;
    public Transform rightFoot;
    public Transform leftFoot;

    [Range(-180f, 180f)]
    public float PelvisRotationMax;

    [Range(-180f, 180f)]
    public float PelvisRotationMin;

    private Vector3 prevLeftFootPos;
    private Vector3 prevRightFootPos;

    //This will be used as a stabilized model space reference point for observations
    //Because ragdolls can move erratically during training, using a stabilized reference transform improves learning
    OrientationCubeController m_OrientationCube;

    //The indicator graphic gameobject that points towards the target
    DirectionIndicator m_DirectionIndicator;
    JointDriveControllerHinge m_JdController;
    EnvironmentParameters m_ResetParams;

    private void Start()
    {
        Time.timeScale = 5f;
    }

    public override void Initialize()
    {
        m_OrientationCube = GetComponentInChildren<OrientationCubeController>();
        m_DirectionIndicator = GetComponentInChildren<DirectionIndicator>();

        //Setup each body part
        m_JdController = GetComponent<JointDriveControllerHinge>();
        m_JdController.SetupBodyPart(pelvis);
        m_JdController.SetupBodyPart(pelvisSlide);
        m_JdController.SetupBodyPart(rightBatterHolder);
        m_JdController.SetupBodyPart(leftBatteryHolder);
        m_JdController.SetupBodyPart(rightArm);
        m_JdController.SetupBodyPart(leftArm);
        m_JdController.SetupBodyPart(rightFoot);
        m_JdController.SetupBodyPart(leftFoot);
        m_JdController.SetupBodyPart(rightFemur);
        m_JdController.SetupBodyPart(leftFemur);
        m_JdController.SetupBodyPart(rightKnee);
        m_JdController.SetupBodyPart(leftKnee);

        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //Reset all of the body parts
        foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
        {

            if (bodyPart.rb.transform == rightArm || bodyPart.rb.transform == leftArm)
            {
                bodyPart.ResetRandomRotation(bodyPart);
            }
            //else if (bodyPart.rb.transform == pelvis) {
            //    bodyPart.ResetPelvisRotation(bodyPart, PelvisRotationMax,PelvisRotationMin);
            //}
            else
            {
                bodyPart.Reset(bodyPart);
            }

        }

        UpdateOrientationObjects();

        //Set our goal walking speed
        MTargetWalkingSpeed =
            randomizeWalkSpeedEachEpisode ? Random.Range(0.1f, m_maxWalkingSpeed) : MTargetWalkingSpeed;

        prevLeftFootPos = leftFoot.position;
        prevRightFootPos = rightFoot.position;

    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPartHinge bp, VectorSensor sensor)
    {
        //GROUND CHECK
        sensor.AddObservation(bp.groundContact.touchingGround); // Is this bp touching the ground

        //Get velocities in the context of our orientation cube's space
        //Note: You can get these velocities in world space as well but it may not train as well.
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.linearVelocity));
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.angularVelocity));

        //Get position relative to hips in the context of our orientation cube's space
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.position - pelvis.position));

        sensor.AddObservation(bp.rb.transform.localRotation);
        sensor.AddObservation(bp.currentStrength / m_JdController.maxJointForceLimit);
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        var cubeForward = m_OrientationCube.transform.forward;

        //velocity we want to match
        var velGoal = cubeForward * MTargetWalkingSpeed;
        //ragdoll's avg vel
        var avgVel = GetAvgVelocity();

        //current ragdoll velocity. normalized
        sensor.AddObservation(Vector3.Distance(velGoal, avgVel));
        //avg body vel relative to cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(avgVel));
        //vel goal relative to cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(velGoal));

        //rotation deltas
        //sensor.AddObservation(Quaternion.FromToRotation(hips.forward, cubeForward));
        //sensor.AddObservation(Quaternion.FromToRotation(head.forward, cubeForward));

        //Position of target position relative to cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformPoint(target.transform.position));

        foreach (var bodyPart in m_JdController.bodyPartsList)
        {
            CollectObservationBodyPart(bodyPart, sensor);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var bpDict = m_JdController.bodyPartsDict;
        var i = -1;

        var continuousActions = actionBuffers.ContinuousActions;
        bpDict[leftFemur].SetJointTargetRotation(continuousActions[++i]);
        bpDict[rightFemur].SetJointTargetRotation(continuousActions[++i]);

        bpDict[leftFoot].SetJointTargetRotation(continuousActions[++i]);
        bpDict[rightFoot].SetJointTargetRotation(continuousActions[++i]);
        bpDict[leftKnee].SetJointTargetRotation(continuousActions[++i]);
        bpDict[rightKnee].SetJointTargetRotation(continuousActions[++i]);
        bpDict[leftArm].SetJointTargetRotation(continuousActions[++i]);
        bpDict[rightArm].SetJointTargetRotation(continuousActions[++i]);


        //update joint strength settings
        bpDict[leftFemur].SetJointStrength(continuousActions[++i]);
        bpDict[rightFemur].SetJointStrength(continuousActions[++i]);
        bpDict[leftArm].SetJointStrength(continuousActions[++i]);
        bpDict[rightArm].SetJointStrength(continuousActions[++i]);
        bpDict[leftFoot].SetJointStrength(continuousActions[++i]);
        bpDict[rightFoot].SetJointStrength(continuousActions[++i]);
        bpDict[rightKnee].SetJointStrength(continuousActions[++i]);
        bpDict[leftKnee].SetJointStrength(continuousActions[++i]);

    }

    //Update OrientationCube and DirectionIndicator
    void UpdateOrientationObjects()
    {
        m_WorldDirToWalk = target.position - pelvis.position;
        m_OrientationCube.UpdateOrientation(pelvis, target);
        if (m_DirectionIndicator)
        {
            m_DirectionIndicator.MatchOrientation(m_OrientationCube.transform);
        }
    }

    void FixedUpdate()
    {
        UpdateOrientationObjects();

        var cubeForward = m_OrientationCube.transform.forward;

        // Set reward for this step according to mixture of the following elements.
        // a. Match target speed
        //This reward will approach 1 if it matches perfectly and approach zero as it deviates
        var matchSpeedReward = GetMatchingVelocityReward(cubeForward * MTargetWalkingSpeed, GetAvgVelocity());

        //Check for NaNs
        if (float.IsNaN(matchSpeedReward))
        {
            throw new ArgumentException(
                "NaN in moveTowardsTargetReward.\n" +
                $" cubeForward: {cubeForward}\n" +
                $" hips.velocity: {m_JdController.bodyPartsDict[pelvis].rb.linearVelocity}\n" +
                $" maximumWalkingSpeed: {m_maxWalkingSpeed}"
            );
        }

        float leftFootMove = Vector3.Distance(leftFoot.position, prevLeftFootPos);
        float rightFootMove = Vector3.Distance(rightFoot.position, prevRightFootPos);

        // small shaping reward for moving feet
        AddReward(0.01f * (leftFootMove + rightFootMove));

        // update previous positions
        prevLeftFootPos = leftFoot.position;
        prevRightFootPos = rightFoot.position;


        // Keep pelvis upright
        float uprightBonus = Vector3.Dot(pelvis.up, Vector3.up);
        AddReward(0.05f * uprightBonus);

        // Penalize excessive torque
        foreach (var bp in m_JdController.bodyPartsList)
            AddReward(-0.001f * bp.currentStrength);

        AddReward(matchSpeedReward);

        if (pelvis.transform.localPosition.y <= -75)
        {
            AddReward(-10);
            EndEpisode();
        }

        if (GetAvgVelocity().magnitude < 0.01f) {
            AddReward(-2);
            EndEpisode();
        }
    }

    //Returns the average velocity of all of the body parts
    //Using the velocity of the hips only has shown to result in more erratic movement from the limbs, so...
    //...using the average helps prevent this erratic movement
    Vector3 GetAvgVelocity()
    {
        Vector3 velSum = Vector3.zero;

        //ALL RBS
        int numOfRb = 0;
        foreach (var item in m_JdController.bodyPartsList)
        {
            numOfRb++;
            velSum += item.rb.linearVelocity;
        }

        var avgVel = velSum / numOfRb;
        return avgVel;
    }

    //normalized value of the difference in avg speed vs goal walking speed.
    public float GetMatchingVelocityReward(Vector3 velocityGoal, Vector3 actualVelocity)
    {
        //distance between our actual velocity and goal velocity
        var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, MTargetWalkingSpeed);

        //return the value on a declining sigmoid shaped curve that decays from 1 to 0
        //This reward will approach 1 if it matches perfectly and approach zero as it deviates
        return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / MTargetWalkingSpeed, 2), 2);
    }

    /// <summary>
    /// Agent touched the target
    /// </summary>
    public void TouchedTarget()
    {
        AddReward(1f);
    }
}