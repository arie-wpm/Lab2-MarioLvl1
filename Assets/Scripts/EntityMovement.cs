using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMovement : MonoBehaviour
{
    public float speed = 3.5f;
    public Vector2 direction = Vector2.right;
    public float turnSmoothTime = 0.06f;

    [SerializeField] private LayerMask groundLayers;

    private Rigidbody2D rb;
    private float velXSmooth;
    private float bounceCooldownUntil;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnEnable()
    {
        velXSmooth = 0f;
        bounceCooldownUntil = Time.time + 0.12f;
        if (direction.x == 0f) direction = Vector2.right;
    }

    private void FixedUpdate()
    {
        float sign = Mathf.Sign(direction.x == 0f ? 1f : direction.x);
        float targetVX = sign * speed;

        float newVX = Mathf.SmoothDamp(rb.linearVelocity.x, targetVX, ref velXSmooth, turnSmoothTime);
        rb.linearVelocity = new Vector2(newVX, rb.linearVelocity.y);

        if (direction.x < 0f) transform.localEulerAngles = new Vector3(0f, -180f, 0f);
        else transform.localEulerAngles = Vector3.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryBounce(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryBounce(collision);
    }

    private void TryBounce(Collision2D collision)
    {
        if (Time.time < bounceCooldownUntil) return;
        
        for (int i = 0; i < collision.contactCount; i++)
        {
            var n = collision.GetContact(i).normal;
            if (Mathf.Abs(n.x) > 0.6f)
            {
                direction = new Vector2(-Mathf.Sign(direction.x == 0f ? 1f : direction.x), 0f);
                bounceCooldownUntil = Time.time + 0.08f;
                return;
            }
        }
    }
}