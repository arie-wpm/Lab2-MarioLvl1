using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    private Vector3 startPos;

    [Header("Ground Velocities")]
    [SerializeField]
    private float minWalkVelocity;

    [SerializeField]
    private float maxWalkVelocity;

    [SerializeField]
    private float maxRunningVelocity;

    [Header("Ground Accelerations")]
    [SerializeField]
    private float walkAcceleration;

    [SerializeField]
    private float runAcceleration;

    [SerializeField]
    private float deceleration;

    [SerializeField]
    private float skidDeceleration;

    [Header("Jump Values")]
    [SerializeField]
    private float slowJumpVelocity;

    [SerializeField]
    private float slowJumpHoldGravity;

    [SerializeField]
    private float slowJumpFallGravity;

    [SerializeField]
    private float walkJumpVelocity;

    [SerializeField]
    private float walkJumpHoldGravity;

    [SerializeField]
    private float walkJumpFallGravity;

    [SerializeField]
    private float runJumpVelocity;

    [SerializeField]
    private float runJumpHoldGravity;

    [SerializeField]
    private float runJumpFallGravity;

    [SerializeField]
    private Transform groundCheckPos;

    [SerializeField]
    private float groundCheckDistance = 0.2f;

    [SerializeField]
    private LayerMask groundLayer;

    [Header("Mid air accelerations")]
    [SerializeField]
    private float midAirForwardSlowAcceleration;

    [SerializeField]
    private float midAirForwardFastAcceleration;

    [SerializeField]
    private float midAirBackwardsFastDeceleration;

    [SerializeField]
    private float midAirBackwardsSlowFastJumpDeceleration;

    [SerializeField]
    private float midAirBackwardsSlowSlowJumpDeceleration;

    public bool grounded;

    [HideInInspector]
    public Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction jumpAction;
    private Vector2 moveValue;
    private Vector2 currentDirection;
    private string jumpType = "";
    private float initialJumpXVelocity;

    private Animator animator;
    private bool isJumping = false;
    private bool hasVerticalVelocity = false;
    private int facingDirection = 1;
    private float postStompTimer;
    private float postStompTime = 0.1f;
    private Vector2 velocityBeforeCollision;
    
    [SerializeField]
    private PlayerStats pStats;

    [SerializeField]
    private Collider2D mainCol;

    [SerializeField]
    private Collider2D footCol;

    [SerializeField]
    private Collider2D headCol;

    [SerializeField]
    private float deathMoveHeight = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        pStats = GetComponent<PlayerStats>();
        moveAction = InputSystem.actions.FindAction("Move");
        runAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
        startPos = transform.position;
        grounded = true;
    }

    private void Update()
    {
        if (StateManager.CurrentGameState() != StateManager.GameState.Play)
            return;
        AnimCheckVelocity();
        moveValue = moveAction.ReadValue<Vector2>();

        // RaycastHit2D hit = Physics2D.Raycast(groundCheckPos.position, Vector2.down, groundCheckDistance, groundLayer);
        // grounded = hit.collider != null;
        // Debug.DrawRay(groundCheckPos.position, Vector2.down, Color.green, groundCheckDistance);
        // if (jumpAction.WasPressedThisFrame() && grounded)
        // {
        //     Debug.Log("Im Jumping");
        //     Jump();
        // }

        // Sorry James for you to review

        Vector2 boxSize = new Vector2(0.65f, 0.5f);
        RaycastHit2D hit = Physics2D.BoxCast(
            groundCheckPos.position,
            boxSize,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        grounded = hit.collider != null;

        if (jumpAction.WasPressedThisFrame() && grounded)
        {
            //Debug.Log("Im Jumping");
            Jump();
        }

        if (!grounded && (jumpAction.WasReleasedThisFrame() || rb.linearVelocityY < 0))
        {
            //Debug.Log("Gravity switched to fall mode");
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

        postStompTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (StateManager.CurrentGameState() != StateManager.GameState.Play)
            return;
        // V = U + at

        float a = 0;
        if (grounded) // Ground Movement
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

            if (
                moveValue.x != 0
                && !(
                    moveValue.x > 0 && currentDirection.x > 0
                    || moveValue.x < 0 && currentDirection.x < 0
                )
            )
            {
                a = moveValue.x * skidDeceleration;
                //sliding
                animator.SetBool("isSliding", true);
            }
            else
            {
                animator.SetBool("isSliding", false);
            }
        }
        else //Mid AIR Movement
        {
            if (
                moveValue.x > 0 && currentDirection.x > 0
                || moveValue.x < 0 && currentDirection.x < 0
            )
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
            else if (
                moveValue.x < 0 && currentDirection.x > 0
                || moveValue.x > 0 && currentDirection.x < 0
            )
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
        
        velocityBeforeCollision = rb.linearVelocity;

        if (rb.linearVelocityX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            currentDirection = Vector2.right;
        }
        else if (rb.linearVelocityX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            currentDirection = Vector2.left;
        }
    }

    private void Jump()
    {
        isJumping = true;
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            bool stompedFromAbove = false;
            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    stompedFromAbove = true;
                    break;
                }
            }

            if (stompedFromAbove)
            {
                Stomp();
            }
            else if (postStompTimer <= 0)
            {
                Die();
            }
        }
        
        if (other.gameObject.layer == 6)
        {
            bool hitFromBelow = false;
            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    hitFromBelow = true;
                    break;
                }
            }

            if (!hitFromBelow) return;
            
            if ((other.gameObject.CompareTag("Block") && pStats.powerState == MarioPowerState.Small) || 
                (other.gameObject.CompareTag("QBlock") && other.gameObject.GetComponent<InteractableBlock>() != null))
            {
                HeadButt("00"); 
            }
            else if (other.gameObject.CompareTag("QBlock") && other.gameObject.GetComponent<InteractableBlock>() == null)
            {
                HeadButt("01"); 
            }
            
        }
    }

    public void Stomp()
    {
        rb.linearVelocityY = UnitsToHex(Mathf.Abs(velocityBeforeCollision.y), "04");
        postStompTimer = postStompTime;
    }

    public void HeadButt(string hex)
    {
        rb.linearVelocityY = -UnitsToHex(Mathf.Abs(velocityBeforeCollision.y), hex);
    }

    private void Die()
    {
        StateManager.SetDeadState();
        pStats.lives--;
        animator.SetBool("isDead", true);
        mainCol.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        StartCoroutine(MoveMarioDead());
    }

    private IEnumerator MoveMarioDead()
    {
        float newY = 0;
        UnityEngine.Vector3 aboveMario = new UnityEngine.Vector3(
            transform.position.x,
            transform.position.y + deathMoveHeight,
            0
        );
        while (Math.Abs(transform.position.y - aboveMario.y) > 0.01f)
        {
            newY = Mathf.MoveTowards(transform.position.y, aboveMario.y, 10f * Time.deltaTime);
            transform.position = new UnityEngine.Vector3(transform.position.x, newY, 0);
            yield return null;
        }
        rb.bodyType = RigidbodyType2D.Dynamic;
        yield return new WaitForSeconds(3f);
        mainCol.enabled = true;
        animator.SetBool("isDead", false);
        animator.ResetControllerState();
        if (pStats.lives >= 0)
        {
            StartCoroutine(GameManager.RestartLevel());
            transform.position = startPos;
        }
        if (pStats.lives < 0)
        {
            StartCoroutine(GameManager.RestartGame());
        }
    }

    private float UnitsToHex(float value, string hexToAdd)
    {
        string valueHex = ((long)(value * Mathf.Pow(16, 4) / 60)).ToString("X5");
        Debug.Log("valueHex: " + valueHex);

        string endHex = valueHex[^3..];

        string fullHex = hexToAdd + endHex;
        Debug.Log("fullHex: " + fullHex);

        long newValue = Convert.ToInt64(fullHex, 16);
        return (float)newValue / Mathf.Pow(16, 4) * 60;
    }

    void OnDrawGizmos()
    {
        if (groundCheckPos == null)
            return;
        Vector2 boxSize = new Vector2(0.65f, 0.5f);
        Vector2 boxCenter = groundCheckPos.position + Vector3.down * groundCheckDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }

    void AnimCheckVelocity()
    {
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.linearVelocityX));
        hasVerticalVelocity = Mathf.Abs(rb.linearVelocity.y) > 0.1f;

        if (hasVerticalVelocity && !isJumping)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isMoving", false);
            animator.speed = 0f;
        }
        else if (hasVerticalVelocity && isJumping)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isMoving", false);
        }
        else if (!hasVerticalVelocity && isJumping)
        {
            isJumping = false;
            animator.speed = 1f;
        }
        else
            animator.speed = 1f;

        if (grounded)
            animator.SetBool("isJumping", false);

        if (Mathf.Abs(rb.linearVelocityX) > 0.1f)
            animator.SetBool("isMoving", true);
        else
            animator.SetBool("isMoving", false);

        if (moveValue.x > 0 && !isJumping)
            facingDirection = 1;
        else if (moveValue.x < 0 && !isJumping)
            facingDirection = -1;
        transform.localScale = new Vector3(facingDirection, 1, 1);
    }
}
