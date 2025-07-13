using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public bool requireMouseHold = true; // Only look around when holding right mouse button

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D
        float moveZ = Input.GetAxis("Vertical");   // W/S
        float moveY = 0f;

        if (Input.GetKey(KeyCode.E)) moveY += 1f; // Up
        if (Input.GetKey(KeyCode.Q)) moveY -= 1f; // Down

        Vector3 move = new Vector3(moveX, moveY, moveZ);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.Self);
    }

    void HandleRotation()
    {
        if (requireMouseHold && !Input.GetMouseButton(1)) return; // Right-click to rotate

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotationX += mouseX * lookSpeed;
        rotationY -= mouseY * lookSpeed;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }
}
