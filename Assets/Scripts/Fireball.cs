using System;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float velocityConversation;
    [SerializeField] private float gravity = 1f;
    [SerializeField] private float angle;
    [SerializeField] private LayerMask groundLayer;

    private Camera cam;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 direction;
    private Vector2 velocityBeforeCollision;
    private Vector3 bottomLeft;
    private Vector3 topRight;
    private bool isExploding = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        cam = Camera.main;
        bottomLeft = cam.ViewportToWorldPoint(new Vector3(-0.1f, -0.1f, cam.nearClipPlane));
        topRight = cam.ViewportToWorldPoint(new Vector3(1.1f, 1.1f, cam.nearClipPlane));
    }

    public void Launch(Vector2 dir)
    {
        direction = dir;
        rb.linearVelocity = new Vector2(speed * direction.x, -speed);
    }

    private void Update()
    {
        if ((transform.position.x < bottomLeft.x || transform.position.x > topRight.x) ||
            (transform.position.y < bottomLeft.y || transform.position.y > topRight.y))
        {
            Destroy(gameObject);
        }
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
        HandleCollision(other);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        HandleCollision(other);
    }

    private void HandleCollision(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            bool shouldExplode = false;
            bool shouldBounce = false;

            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    shouldBounce = true;
                }
                else if (contact.normal.x > 0.5f || contact.normal.x < -0.5f || contact.normal.y < -0.5f)
                {
                    shouldExplode = true;
                }
            }

            if (shouldExplode)
            {
                Explode();
            }
            else if (shouldBounce)
            {
                rb.gravityScale = gravity;
                rb.linearVelocity = new Vector2(
                    speed * direction.x,
                    currentSpeed *= velocityConversation
                );
            }
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            Explode();
        }
    }


    private void Explode()
    {
        if (isExploding) return;
        isExploding = true;
        
        animator.SetTrigger("Explode");
        GetComponent<Collider2D>().enabled = false;
        Destroy(rb);
    }

    private void Delete()
    {
        Destroy(gameObject);
    }
}
