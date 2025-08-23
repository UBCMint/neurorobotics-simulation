using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.MLAgents;
using System.Diagnostics.Contracts;

namespace Unity.MINTNeurorobotics
{
    [System.Serializable]
    public class BodyPartHinge
    {
        [Header("Body Part Info")]
        public List<HingeJoint> joints = new List<HingeJoint>();
        public Rigidbody rb;

        [HideInInspector] public Vector3 startingPos;
        [HideInInspector] public Quaternion startingRot;

        [Header("Ground & Target Contact")]
        public GroundContact groundContact;
        public TargetContact targetContact;

        [HideInInspector] public JointDriveControllerHinge thisJdController;

        [Header("Current Joint Settings")]
        public float currentStrength;
        public List<float> currentTargetAngles = new List<float>();

        [Header("Other Debug Info")]
        public List<float> currentJointForces = new List<float>();

        public void Reset(BodyPartHinge bp)
        {
            bp.rb.transform.position = bp.startingPos;
            bp.rb.transform.rotation = bp.startingRot;
            bp.rb.linearVelocity = Vector3.zero;
            bp.rb.angularVelocity = Vector3.zero;

            if (bp.groundContact) bp.groundContact.touchingGround = false;
            if (bp.targetContact) bp.targetContact.touchingTarget = false;
        }

        public void ResetRandomRotation(BodyPartHinge bp)
        {

            bp.rb.transform.position = bp.startingPos;
            float randomAngle = Random.Range(bp.joints[0].limits.min, bp.joints[0].limits.max);
            Quaternion randomRot = Quaternion.AngleAxis(randomAngle, bp.joints[0].axis);

            bp.rb.transform.rotation = bp.startingRot * randomRot;  // this rotates the joint by the randomRotational amount relative to t he startingRot.

            bp.rb.linearVelocity = Vector3.zero;
            bp.rb.angularVelocity = Vector3.zero;

            if (bp.groundContact) bp.groundContact.touchingGround = false;
            if (bp.targetContact) bp.targetContact.touchingTarget = false;
        }

        public void ResetPelvisRotation(BodyPartHinge bp, float maxAngle, float minAngle)
        {
            bp.rb.transform.position = bp.startingPos;
            float randomAngle = Random.Range(minAngle, maxAngle);

            bp.rb.transform.rotation = new Quaternion(startingRot.x + randomAngle, startingRot.y, startingRot.z, startingRot.w);  // this rotates the joint by the randomRotational amount relative to t he startingRot.

            bp.rb.linearVelocity = Vector3.zero;
            bp.rb.angularVelocity = Vector3.zero;

            if (bp.groundContact) bp.groundContact.touchingGround = false;
            if (bp.targetContact) bp.targetContact.touchingTarget = false;
        }

        /// <summary>
        /// Set target angles for all hinge joints in this body part.
        /// Angles are normalized from -1 to 1 and mapped to joint limits.
        /// </summary>
        public void SetJointTargetRotation(float normalizedAngle)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                var joint = joints[i];
                if (joint == null) continue;

                // Map normalized [-1,1] to joint limits
                float min = joint.limits.min;
                float max = joint.limits.max;
                float targetAngle = Mathf.Lerp(min, max, (normalizedAngle + 1f) * 0.5f);

                JointSpring spring = joint.spring;
                spring.spring = thisJdController.maxJointSpring;
                spring.damper = thisJdController.jointDampen;
                spring.targetPosition = targetAngle;
                joint.spring = spring;
                joint.useSpring = true;
                joint.useMotor = false;

                if (currentTargetAngles.Count <= i)
                    currentTargetAngles.Add(targetAngle);
                else
                    currentTargetAngles[i] = targetAngle;
            }
        }

        /// <summary>
        /// Set maximum motor force (strength) for all hinge joints in this body part.
        /// </summary>
        public void SetJointStrength(float normalizedStrength)
        {
            float rawVal = (normalizedStrength + 1f) * 0.5f * thisJdController.maxJointForceLimit;
            currentStrength = rawVal;

            foreach (var joint in joints)
            {
                if (joint == null) continue;

                JointSpring spring = joint.spring;
                spring.spring = rawVal;
                spring.damper = thisJdController.jointDampen;
                joint.spring = spring;

                joint.useSpring = true;
                joint.useMotor = false;
            }
        }
    }

    public class JointDriveControllerHinge : MonoBehaviour
    {
        [Header("Joint Drive Settings")]
        public float maxJointSpring = 100f;
        public float jointDampen = 5f;
        public float maxJointForceLimit = 100f;

        [HideInInspector] public Dictionary<Transform, BodyPartHinge> bodyPartsDict = new Dictionary<Transform, BodyPartHinge>();
        [HideInInspector] public List<BodyPartHinge> bodyPartsList = new List<BodyPartHinge>();

        const float k_MaxAngularVelocity = 50.0f;

        /// <summary>
        /// Create BodyPartHinge object and add it to dictionary.
        /// </summary>
        public void SetupBodyPart(Transform t)
        {
            var bp = new BodyPartHinge
            {
                rb = t.GetComponent<Rigidbody>(),
                startingPos = t.position,
                startingRot = t.rotation
            };
            bp.rb.maxAngularVelocity = k_MaxAngularVelocity;

            // Collect all hinge joints on this transform
            HingeJoint[] hingeJoints = t.GetComponents<HingeJoint>();
            if (hingeJoints != null && hingeJoints.Length > 0)
            {
                bp.joints.AddRange(hingeJoints);
            }

            // Ground contact
            bp.groundContact = t.GetComponent<GroundContact>();
            if (!bp.groundContact)
            {
                bp.groundContact = t.gameObject.AddComponent<GroundContact>();
                bp.groundContact.agent = gameObject.GetComponent<Agent>();
            }
            else
            {
                bp.groundContact.agent = gameObject.GetComponent<Agent>();
            }

            bp.thisJdController = this;
            bodyPartsDict.Add(t, bp);
            bodyPartsList.Add(bp);
        }

        /// <summary>
        /// Get current forces acting on hinge joints.
        /// </summary>
        public void GetCurrentJointForces()
        {
            foreach (var bodyPart in bodyPartsList)
            {
                bodyPart.currentJointForces.Clear();
                foreach (var joint in bodyPart.joints)
                {
                    if (joint != null)
                    {
                        // HingeJoint does not have direct "currentForce" property
                        // but we can approximate torque from Rigidbody
                        float torqueMag = bodyPart.rb.angularVelocity.magnitude * bodyPart.rb.mass;
                        bodyPart.currentJointForces.Add(torqueMag);
                    }
                    else
                    {
                        bodyPart.currentJointForces.Add(0f);
                    }
                }
            }
        }
    }
}