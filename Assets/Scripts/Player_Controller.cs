using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //public float moveSpeed = 10f;
    //public float rotateSpeed = 500f;

    //private Rigidbody rb;
    //private Vector2 movementInput;

    //private void Start()
    //{
    //    rb = GetComponent<Rigidbody>();
    //}

    //public void OnMove(InputValue value)
    //{
    //    movementInput = value.Get<Vector2>();
    //}

    //private void FixedUpdate()
    //{
    //    // Move forward based on vertical input (W/S)
    //    Vector3 moveDirection = transform.forward * movementInput.y;
    //    rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);

    //    // Rotate based on horizontal input (A/D)
    //    float turn = movementInput.x * rotateSpeed * Time.fixedDeltaTime;
    //    Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
    //    rb.MoveRotation(rb.rotation * turnRotation);
    //}
}
