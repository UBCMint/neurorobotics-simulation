using UnityEngine;

public class RobotStateManager : MonoBehaviour
{
    public static bool standing { get; private set; } // Accessible globally, read-only
    private int footContacts = 0; // Count of foot colliders touching ground/target
    private int bodyContacts = 0; // Count of non-foot colliders touching ground

    void Start()
    {
        standing = true ; // Initialize to false
    }

    // Method for children to report collisions
    public void ReportCollision(string partTag, bool isEntering, string groundTag)
    {
        if (groundTag == "ground" || groundTag == "goal")
        {
            if (partTag == "foot")
            {
                footContacts += isEntering ? 1 : -1;
            }
            else
            {
                bodyContacts += isEntering ? 1 : -1;
            }

            // Update standing state: true only if at least one foot is touching and no body parts are
            standing = footContacts > 0 && bodyContacts == 0;

            
        }
        Debug.Log($"Foot contacts: {footContacts}, Body contacts: {bodyContacts}, Standing: {standing}");
    }
}