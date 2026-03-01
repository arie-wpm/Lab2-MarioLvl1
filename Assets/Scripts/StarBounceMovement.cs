using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StarBounceMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField] private float hopVelocity = 7.5f;
    [SerializeField] private float groundNormalMin = 0.6f; // helps star decide if it's wall or ground

    private Rigidbody2D rb;
    private Vector2 velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        velocity.x = direction.x * speed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.8f)
            {
                direction.x *= -1f;
                break;
            }
        }
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y >= groundNormalMin)
            {
                rb.linearVelocity = new Vector2(direction.x * speed, hopVelocity);
                break;
            }
        }
    }
}