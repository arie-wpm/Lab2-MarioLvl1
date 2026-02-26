using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMovement : MonoBehaviour
{
    public float speed = 1f;
    public Vector2 direction = Vector2.left;

    [Header("Raycast")]
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.1f;
    public LayerMask groundMask;

    private Rigidbody2D rb;
    private Vector2 velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    private void OnBecameVisible()
    {
#if UNITY_EDITOR
        enabled = !UnityEditor.EditorApplication.isPaused;
#else
        enabled = true;
#endif
    }

    private void OnBecameInvisible()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        rb.WakeUp();
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
        rb.Sleep();
    }

    private void FixedUpdate()
    {
        velocity.x = direction.x * speed;
        velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

        // grounded check
        bool grounded = Physics2D.Raycast(rb.position, Vector2.down, groundCheckDistance, groundMask);
        if (grounded)
        {
            velocity.y = Mathf.Max(velocity.y, 0f);
        }

        // wall check
        bool hitWall = Physics2D.Raycast(rb.position, direction, wallCheckDistance, groundMask);
        if (hitWall)
        {
            direction = -direction;
        }

        // face movement direction
        if (direction.x > 0f)
        {
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        }
        else if (direction.x < 0f)
        {
            transform.localEulerAngles = Vector3.zero;
        }
    }
}