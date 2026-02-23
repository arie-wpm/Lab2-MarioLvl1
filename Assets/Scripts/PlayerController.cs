using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Ground Velocities")]
    [SerializeField] private float maxWalkVelocity;
    [SerializeField] private float maxRunningVelocity;
    
    [Header("Ground Accelerations")]
    [SerializeField] private float walkAcceleration;
    [SerializeField] private float runAcceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float skidDeceleration;
    
    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction jumpAction;
    private Vector2 moveValue;
    private Vector2 currentDirection;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
        runAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
        
    }

    private void Update()
    {
        moveValue = moveAction.ReadValue<Vector2>();
        
    }

    private void FixedUpdate()
    {
        // V = U + at
        float a = 0;

        if (moveValue.x != 0 && !runAction.IsPressed())
        {
            a = moveValue.x * walkAcceleration;

            if (Mathf.Abs(rb.linearVelocityX) > maxWalkVelocity)
            {
                rb.linearVelocityX = maxWalkVelocity * currentDirection.x;
            }
        }
        else if (moveValue.x != 0 && runAction.IsPressed())
        {
            a = moveValue.x * runAcceleration;

            if (MathF.Abs(rb.linearVelocityX) > maxRunningVelocity)
            {
                rb.linearVelocityX = maxRunningVelocity * currentDirection.x;
            }
        }
        else
        {
            a = -currentDirection.x * deceleration;
        }
        
        if(moveValue.x != 0 && !(moveValue.x > 0 && currentDirection.x > 0 
                                 || moveValue.x < 0 && currentDirection.x < 0))
        {
            a = moveValue.x * skidDeceleration;
        }
        
        rb.linearVelocityX = rb.linearVelocity.x + a * Time.fixedDeltaTime;
        
        if (rb.linearVelocityX > 0)
        {
            currentDirection = Vector2.right;
        }
        else if (rb.linearVelocityX < 0)
        {
            currentDirection = Vector2.left;
        }
        
    }
}
