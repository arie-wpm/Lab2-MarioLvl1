using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Ground Velocities")] 
    [SerializeField] private float minWalkVelocity;
    [SerializeField] private float maxWalkVelocity;
    [SerializeField] private float maxRunningVelocity;
    
    [Header("Ground Accelerations")]
    [SerializeField] private float walkAcceleration;
    [SerializeField] private float runAcceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float skidDeceleration;

    [Header("Jump Values")] 
    [SerializeField] private float slowJumpVelocity;
    [SerializeField] private float slowJumpHoldGravity;
    [SerializeField] private float slowJumpFallGravity;
    [SerializeField] private float walkJumpVelocity;
    [SerializeField] private float walkJumpHoldGravity;
    [SerializeField] private float walkJumpFallGravity;
    [SerializeField] private float runJumpVelocity;
    [SerializeField] private float runJumpHoldGravity;
    [SerializeField] private float runJumpFallGravity;
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckDistance= 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    
    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction jumpAction;
    private Vector2 moveValue;
    private Vector2 currentDirection;
    public bool grounded;
    private string jumpType = "";
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
        runAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
        grounded = true;
    }

    private void Update()
    {
        if (jumpAction.WasReleasedThisFrame())
        {
            Debug.Log("Released Jump");
        }
        
        moveValue = moveAction.ReadValue<Vector2>();

        RaycastHit2D hit = Physics2D.Raycast(groundCheckPos.position, Vector2.down, groundCheckDistance, groundLayer);
        grounded = hit.collider != null;
        Debug.DrawRay(groundCheckPos.position, Vector2.down, Color.green, groundCheckDistance);
        if (jumpAction.WasPressedThisFrame() && grounded)
        {
            Debug.Log("Im Jumping");
            Jump();
        }

        if (!grounded && (jumpAction.WasReleasedThisFrame()|| rb.linearVelocityY < 0))
        {
            Debug.Log("Gravity switched to fall mode");
            switch (jumpType)
            {
                case "Slow":
                    rb.gravityScale = slowJumpFallGravity;
                    break;
                case "Walk":
                    rb.gravityScale = walkJumpFallGravity;
                    break;
                case "Run":
                    rb.gravityScale = runJumpFallGravity;
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        // V = U + at

        float a = 0;
        if (grounded)
        {
            
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
                if (Mathf.Abs(rb.linearVelocityX) < minWalkVelocity)
                {
                    rb.linearVelocityX = 0;
                }
                else
                {
                    a = -currentDirection.x * deceleration;
                }
            }
            
            if(moveValue.x != 0 && !(moveValue.x > 0 && currentDirection.x > 0 
                                     || moveValue.x < 0 && currentDirection.x < 0))
            {
                a = moveValue.x * skidDeceleration;
            }

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

    private void Jump()
    {
        grounded = false;
        
        if (Mathf.Abs(rb.linearVelocityX) < 3.75)
        {
            jumpType = "Slow";
            rb.gravityScale = slowJumpHoldGravity;
            rb.linearVelocityY = slowJumpVelocity;
        }
        else if (Mathf.Abs(rb.linearVelocityX) >= 3.75 && Mathf.Abs(rb.linearVelocityX) < 8.671875)
        {
            jumpType = "Walk";
            rb.gravityScale = walkJumpHoldGravity;
            rb.linearVelocityY = walkJumpVelocity;
        }
        else if (Mathf.Abs(rb.linearVelocityX) >= 8.671875)
        {
            jumpType = "Run";
            rb.gravityScale = runJumpHoldGravity;
            rb.linearVelocityY = runJumpVelocity;
        }
    }
}
