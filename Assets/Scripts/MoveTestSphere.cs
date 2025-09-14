using UnityEngine;
using System.IO;

public class MoveTestSphere : MonoBehaviour
{
    public Vector3 direction = Vector3.forward; // default: z-axis
    public float speed = 2f; // units per second
    public string sharedPath;

    void Start()
    {
        // shared_data.txt is directly in Assets/
        sharedPath = Path.Combine(Application.dataPath, "shared_data.txt");
    }
    
    void Update()
    {
        if (File.Exists(sharedPath))
        {
            string content = File.ReadAllText(sharedPath).Trim();

            int value = (content == "1") ? 1 : -1;

            transform.Translate(direction * speed * value * Time.deltaTime);
        }
    }
}
