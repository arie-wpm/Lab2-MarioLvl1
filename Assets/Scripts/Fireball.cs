using System;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float velocityConversation;
    [SerializeField] private float gravity = 1f;
    [SerializeField] private float angle;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 direction;
    private Vector2 velocityBeforeCollision;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void Launch(Vector2 dir)
    {
        direction = dir;
        rb.linearVelocity = new Vector2(speed * direction.x, -speed);
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(speed * direction.x, rb.linearVelocity.y);
            velocityBeforeCollision = rb.linearVelocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            Debug.Log("Fire ball landed");
            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    rb.gravityScale = gravity;
                    rb.linearVelocity = new Vector2(speed * direction.x, Mathf.Abs(velocityBeforeCollision.y) * velocityConversation );
                }
                else if (contact.normal.x > 0.5f || contact.normal.x < -0.5f || contact.normal.y < -0.5f)
                {
                    Explode();
                }
            }
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            Explode();
        }
    }


    private void Explode()
    {
        animator.SetTrigger("Explode");
        Destroy(rb);
    }

    private void Delete()
    {
        Destroy(gameObject);
    }
}
