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

        public override void Initialize()
        {
            // Called once when the agent is first enabled.
            // Good place to initialize variables, cache component references, or save starting positions.

        }

        public override void OnEpisodeBegin()
        {
            // Reset the robot's root (pelvis) to starting position and rotation
            pelvis.TeleportRoot(pelvis.transform.parent.position, Quaternion.identity);
            pelvis.velocity = Vector3.zero;
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
            sensor.AddObservation(pelvis.velocity); // 3 obs
            sensor.AddObservation(pelvis.angularVelocity); // 3 obs
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var continuousActions = actions.ContinuousActions;

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
            if (pelvis.transform.localPosition.y < 0.5f || pelvis.transform.up.y < 0.5f)
            {
                SetReward(-1f);
                EndEpisode();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // Used for manual testing/debugging before the model is trained.
            // Map your keyboard/gamepad input to the action arrays.

            var continuousActionsOut = actionsOut.ContinuousActions;
            // Example map:
            // continuousActionsOut[0] = Input.GetAxis("Vertical");
            // continuousActionsOut[1] = Input.GetAxis("Horizontal");
        }
    }
}