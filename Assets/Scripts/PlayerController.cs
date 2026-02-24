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

    [Header("Mid air accelerations")] 
    [SerializeField] private float midAirForwardSlowAcceleration;
    [SerializeField] private float midAirForwardFastAcceleration;
    [SerializeField] private float midAirBackwardsFastDeceleration;
    [SerializeField] private float midAirBackwardsSlowFastJumpDeceleration;
    [SerializeField] private float midAirBackwardsSlowSlowJumpDeceleration;
    
    public bool grounded;
    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction jumpAction;
    private Vector2 moveValue;
    private Vector2 currentDirection;
    private string jumpType = "";
    private float initialJumpXVelocity;

    // testing Animator
    private Animator animator;
    [SerializeField] private bool isLarge;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        moveAction = InputSystem.actions.FindAction("Move");
        runAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
        grounded = true;
    }

    private void Update()
    {    
        moveValue = moveAction.ReadValue<Vector2>();

        // RaycastHit2D hit = Physics2D.Raycast(groundCheckPos.position, Vector2.down, groundCheckDistance, groundLayer);
        // grounded = hit.collider != null;
        // Debug.DrawRay(groundCheckPos.position, Vector2.down, Color.green, groundCheckDistance);
        // if (jumpAction.WasPressedThisFrame() && grounded)
        // {
        //     EditorDebug.Log("Im Jumping");
        //     Jump();
        // }

        // Sorry James for you to review

        Vector2 boxSize = new Vector2(0.65f, 0.5f);
        RaycastHit2D hit = Physics2D.BoxCast(groundCheckPos.position, boxSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
        grounded = hit.collider != null;
        
        if (jumpAction.WasPressedThisFrame() && grounded)
        {
            EditorDebug.Log("Im Jumping");
            Jump();
        }

        if (!grounded && (jumpAction.WasReleasedThisFrame()|| rb.linearVelocityY < 0))
        {
            EditorDebug.Log("Gravity switched to fall mode");
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

    void FreezeJumpAnim()
    {
        animator.SetBool("isJumping", true);
        animator.SetBool("isMoving", false);
    }

    void UnFreezeJumpAnim()
    {
        animator.SetBool("isJumping", false);
        animator.SetBool("isMoving", true);
    }

    private void FixedUpdate()
    {
        // V = U + at

        float a = 0;
        if (grounded) // Ground Movement
        {
            UnFreezeJumpAnim();
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
                //sliding
                animator.SetBool("isSliding", true);
            } else
            {
                animator.SetBool("isSliding", false);
            }

        }
        else //Mid AIR Movement
        {
            FreezeJumpAnim();
            if (moveValue.x > 0 && currentDirection.x > 0 || moveValue.x < 0 && currentDirection.x < 0)
            {
                if (Mathf.Abs(rb.linearVelocityX) < 5.859375)
                {
                    a = midAirForwardSlowAcceleration * currentDirection.x;
                }
                else
                {
                    a = midAirForwardFastAcceleration * currentDirection.x;
                }
            }
            else if(moveValue.x < 0 && currentDirection.x > 0 || moveValue.x > 0 && currentDirection.x < 0)
            {
                if (Mathf.Abs(rb.linearVelocityX) >= 5.859375)
                {
                    a = midAirBackwardsFastDeceleration * -currentDirection.x;
                }
                else
                {
                    if (initialJumpXVelocity >= 6.796875)
                    {
                        a = midAirBackwardsSlowFastJumpDeceleration * -currentDirection.x;
                    }
                    else
                    {
                        a = midAirBackwardsSlowSlowJumpDeceleration * -currentDirection.x;
                    }
                }
            }
        }
        
        rb.linearVelocityX = rb.linearVelocity.x + a * Time.fixedDeltaTime;
        animator.speed = Mathf.Abs(rb.linearVelocityX);
        
        if (rb.linearVelocityX > 0)
        {
            animator.SetBool("isMoving", true);
            transform.localScale = new Vector3(1, 1, 1);
            currentDirection = Vector2.right;
        }
        else if (rb.linearVelocityX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            animator.SetBool("isMoving", true);
            currentDirection = Vector2.left;
        } else
        {
            animator.SetBool("isMoving", false);
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

        initialJumpXVelocity = Mathf.Abs(rb.linearVelocityX);
    }

    void OnDrawGizmos()
    {
        if (groundCheckPos == null) return;
        Vector2 boxSize = new Vector2(0.65f, 0.5f);
        Vector2 boxCenter = groundCheckPos.position + Vector3.down * groundCheckDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
