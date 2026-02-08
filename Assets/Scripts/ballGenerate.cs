using UnityEngine;

public class ballGenerate : MonoBehaviour
{
    public GameObject ballPrefab; // Assign the ball prefab from Project window
    public Transform target; // Assign the robot's transform in the Inspector
    public float minThrowInterval = 3f; // Minimum time between throws (seconds)
    public float maxThrowInterval = 10f; // Maximum time between throws (seconds)
    public float throwForce = 5f; // Force applied to the ball
    public Vector3 spawnOffset = new Vector3(0, 1, 0); // Offset for spawning balls relative to thrower
    public Vector3 robotOffset = new Vector3(-5, 1, 3); // Offset from robot's position

    public bool enable = false;

    private float timer;
    private float currentThrowInterval;

    public Vector3[] spawnOffsets = new Vector3[]
    {
        new Vector3(-5, 1, 3),
        new Vector3(0, 1, -5),
        new Vector3(2, 1, 9)
    };

    void Start()
    {
      
        currentThrowInterval = Random.Range(minThrowInterval, maxThrowInterval);
        timer = currentThrowInterval;
    }

    void Update()
    {
        UpdatePosition();


        if (enable && RobotStateManager.standing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                robotOffset = spawnOffsets[Random.Range(0, spawnOffsets.Length)];
                ThrowBall();
                currentThrowInterval = Random.Range(minThrowInterval, maxThrowInterval);
                timer = currentThrowInterval;
            }
        }
    }

    void UpdatePosition()
    {
        transform.position = target.position + robotOffset;

    }

    void ThrowBall()
    {
       
        Vector3 spawnPos = transform.position + spawnOffset;
        GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity); //spawn new ball

        Rigidbody rb = ball.GetComponent<Rigidbody>();
            
        Vector3 targetOriginOffset = new Vector3(0.0f, 0.0f, -3.5f); //offset to center origin of robot to center of mass
        Vector3 direction = (target.position - spawnPos -targetOriginOffset).normalized;

        rb.AddForce(direction * throwForce, ForceMode.Impulse);

        // Destroy after 3 seconds
        Destroy(ball, 3f);
    }
}