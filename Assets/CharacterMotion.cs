using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]

public class CharacterMotion : MonoBehaviour
{

    public Vector2 moveInput;

    public float walkSpeed;
    public float runSpeed;


    public float rotationSpeed;
    public float dampingTime = 0.2f;
    private float targetAngle;
    private float currentAngle;
    private float currentAngularVelocity;
    private Quaternion originalRotation;

    public float angleKoeff = 0.9f;

    private float move;
    private float deltaAngle;
    public bool isRun;

    public Animator animator;

    void Start()
    {
        originalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        move = 0;
        deltaAngle = 0;

        Move(moveInput);
        if (animator != null)
        {
            animator.SetFloat("Move", move);
            animator.SetFloat("Rotate", deltaAngle);
        }
    }

    public void Move(Vector2 direction)
    {
        if (direction.magnitude > 1) direction.Normalize();

        Vector3 localDirection = transform.InverseTransformDirection(direction.x, 0, direction.y);

        deltaAngle = (direction.magnitude > 0.00001f) ? Mathf.Atan2(localDirection.x, localDirection.z) : 0;
        float deltaAngleDeg = deltaAngle * Mathf.Rad2Deg;

        move = Vector3.Dot(localDirection, Vector3.forward);
        move = move > angleKoeff ? move : 0;

        targetAngle = currentAngle + deltaAngleDeg;

        currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref currentAngularVelocity, dampingTime, rotationSpeed);
        transform.localRotation = Quaternion.Euler(originalRotation.eulerAngles.x,
                                                    originalRotation.eulerAngles.y + currentAngle,
                                                    originalRotation.eulerAngles.z);

        rigidbody.velocity = (new Vector3(direction.x, 0, direction.y)) * move * (isRun ? runSpeed : walkSpeed) + rigidbody.velocity.y * Vector3.up;

        if (isRun) move *= 2;
    }
}