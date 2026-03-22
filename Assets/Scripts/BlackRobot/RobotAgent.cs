using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace Assets.Scripts.BlackRobot
{
    public class RobotAgent : Agent
    {
        [Header("Robot Components")]
        public ArticulationBody pelvis;
        public ArticulationLimb leftFemur;
        public ArticulationLimb rightFemur;
        public ArticulationLimb leftTibia;
        public ArticulationLimb rightTibia;
        public ArticulationLimb leftFoot;
        public ArticulationLimb rightFoot;

        [Header("Agent Parameters")]
        [SerializeField] private Vector3 resetPosition = new Vector3(0.0f, 7.0f, 0.0f);

        public override void Initialize()
        {
            // Called once when the agent is first enabled.
            // Good place to initialize variables, cache component references, or save starting positions.

        }

        public override void OnEpisodeBegin()
        {
            // Reset the robot's root (pelvis) to starting position and rotation
            pelvis.TeleportRoot(resetPosition, Quaternion.identity);
            pelvis.linearVelocity = Vector3.zero;
            pelvis.angularVelocity = Vector3.zero;

            // Randomize limbs
            leftFemur.randomizeLimbPosition();
            rightFemur.randomizeLimbPosition();
            leftTibia.randomizeLimbPosition();
            rightTibia.randomizeLimbPosition();
            leftFoot.randomizeLimbPosition();
            rightFoot.randomizeLimbPosition(); // Fixed typo here (was leftFoot twice before)
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Pelvis positioning and balancing
            sensor.AddObservation(pelvis.transform.localPosition.y); // 1 obs: Height
            sensor.AddObservation(pelvis.transform.up); // 3 obs: Is it upright?
            
            // Pelvis velocities
            sensor.AddObservation(pelvis.linearVelocity); // 3 obs
            sensor.AddObservation(pelvis.angularVelocity); // 3 obs
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var continuousActions = actions.ContinuousActions;  // actions is read only, whereas actionsOut is write only.

            // Map actions to limb angles (continuous actions come as a float from -1.0 to 1.0)
            // Multiplying by, say, 90 degrees to give it a sensible movement range
            float multiplier = 90f;

            leftFemur.MoveLimbToPosition(continuousActions[0] * multiplier, continuousActions[0] * multiplier, continuousActions[0] * multiplier);
            rightFemur.MoveLimbToPosition(continuousActions[1] * multiplier, continuousActions[1] * multiplier, continuousActions[1] * multiplier);
            leftTibia.MoveLimbToPosition(continuousActions[2] * multiplier, continuousActions[2] * multiplier, continuousActions[2] * multiplier);
            rightTibia.MoveLimbToPosition(continuousActions[3] * multiplier, continuousActions[3] * multiplier, continuousActions[3] * multiplier);
            leftFoot.MoveLimbToPosition(continuousActions[4] * multiplier, continuousActions[4] * multiplier, continuousActions[4] * multiplier);
            rightFoot.MoveLimbToPosition(continuousActions[5] * multiplier, continuousActions[5] * multiplier, continuousActions[5] * multiplier);

            // Reward agent for staying alive (balancing). 
            AddReward(0.01f);

            // Check if the robot has fallen over (height too low or tilted too far)
            if (pelvis.transform.localPosition.y < 3.0f || pelvis.transform.up.y < 0.5f)
            {
                SetReward(-1f);
                EndEpisode();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // Used for manual testing/debugging before the model is trained.
            // Map your keyboard/gamepad input to the action arrays.

            float targetAngle = 45.0f;

            var continuousActionsOut = actionsOut.ContinuousActions;

            continuousActionsOut[0] = Input.GetKey(KeyCode.W) ? -targetAngle : Input.GetKey(KeyCode.S) ? targetAngle : 0f;
            continuousActionsOut[1] = Input.GetKey(KeyCode.I) ? targetAngle : Input.GetKey(KeyCode.K) ? -targetAngle : 0f;
            continuousActionsOut[2] = Input.GetKey(KeyCode.D) ? targetAngle : Input.GetKey(KeyCode.A) ? -targetAngle : 0f;
            continuousActionsOut[3] = Input.GetKey(KeyCode.L) ? targetAngle : Input.GetKey(KeyCode.J) ? -targetAngle : 0f;
            continuousActionsOut[4] = Input.GetKey(KeyCode.E) ? targetAngle : Input.GetKey(KeyCode.Q) ? -targetAngle : 0f;
            continuousActionsOut[5] = Input.GetKey(KeyCode.O) ? -targetAngle : Input.GetKey(KeyCode.U) ? targetAngle : 0f;

        }
    }
}