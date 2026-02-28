using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class StarmanVelocityPreserver : MonoBehaviour
{
    [SerializeField] private float enforceDuration = 0.06f;
    [SerializeField] private bool preserveYVelocity = false;

    private Rigidbody2D rb;
    private PlayerStats stats;

    private Vector2 v;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
    }

    private void FixedUpdate()
    {
        v = rb.linearVelocity;
        if (timer > 0f)
        {
            timer -= Time.fixedDeltaTime;

            if (preserveYVelocity)
                rb.linearVelocity = v;
            else
                rb.linearVelocity = new Vector2(v.x, rb.linearVelocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!stats.isInvincible) return;
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Shell"))
        {
            timer = enforceDuration;
            if (preserveYVelocity)
                rb.linearVelocity = v;
            else
                rb.linearVelocity = new Vector2(v.x, rb.linearVelocity.y);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!stats.isInvincible) return;

        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Shell"))
        {
            timer = Mathf.Max(timer, enforceDuration);
        }
    }
}