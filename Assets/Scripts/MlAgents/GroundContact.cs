using UnityEngine;
using Unity.MLAgents;

namespace Unity.MINTNeurorobotics
{
    [DisallowMultipleComponent]
    public class GroundContact : MonoBehaviour
    {

        [HideInInspector] public Agent agent;

        [Header("Ground Check")] public bool agentDoneOnGroundContact;
        public bool penalizeGroundContact;
        public float groundContactPenalty; // Penalty amount (ex: -1).
        public bool touchingGround;
        const string k_Ground = "ground"; // Tag of ground object.


        void OnCollisionEnter(Collision col)
        {
            if (col.transform.CompareTag(k_Ground))
            {
                touchingGround = true;
                if (penalizeGroundContact)
                {
                    agent.SetReward(groundContactPenalty);
                }

                if (agentDoneOnGroundContact)
                {
                    agent.EndEpisode();
                }
            }
        }

        /// <summary>
        /// Check for end of ground collision and reset flag appropriately.
        /// </summary>
        void OnCollisionExit(Collision other)
        {
            if (other.transform.CompareTag(k_Ground))
            {
                touchingGround = false;
            }
        }

    }
}
