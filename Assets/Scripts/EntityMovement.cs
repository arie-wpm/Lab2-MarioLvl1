using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMovement : MonoBehaviour
{
    public float speed = 1f;
    public Vector2 direction = Vector2.left;
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.1f;
    public LayerMask groundMask;

    private Rigidbody2D rb;
    private Collider2D col;
    private float wallCooldown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        var cols = GetComponents<Collider2D>();
        col = null;
        for (int i = 0; i < cols.Length; i++)
        {
            if (!cols[i].isTrigger)
            {
                col = cols[i];
                break;
            }
        }
        if (col == null && cols.Length > 0) col = cols[0];

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (col == null) return;

        if (direction.x == 0f) direction = Vector2.right;

        wallCooldown -= Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(Mathf.Sign(direction.x) * speed, rb.linearVelocity.y);

        Bounds b = col.bounds;

        Vector2 groundOrigin = new Vector2(b.center.x, b.min.y + 0.02f);
        bool grounded = Physics2D.Raycast(groundOrigin, Vector2.down, groundCheckDistance, groundMask);

        if (grounded && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        float side = Mathf.Sign(direction.x);
        float wallY = b.min.y + b.size.y * 0.75f;
        Vector2 wallOrigin = new Vector2(b.center.x + side * (b.extents.x + 0.02f), wallY);
        bool hitWall = Physics2D.Raycast(wallOrigin, new Vector2(side, 0f), wallCheckDistance, groundMask);

        if (hitWall && wallCooldown <= 0f)
        {
            direction = new Vector2(-direction.x, direction.y);
            wallCooldown = 0.1f;
            rb.position += new Vector2(Mathf.Sign(direction.x) * 0.03f, 0f);
        }

        if (direction.x > 0f) transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        else transform.localEulerAngles = Vector3.zero;
    }
}