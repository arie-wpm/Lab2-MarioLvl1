using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float movespeed = 2f;
    private Vector2 moveDir;
    private Rigidbody2D rb;
    private Collider2D collider;
    [SerializeField] private float wallCheckDist = 0.1f;
    [SerializeField] private float deathDelay = 2f;
    [SerializeField] private float knockbackForceX;
    [SerializeField] private float knockbackForceY;
    
    private enum EnemyType
    {
        Goomba,
        KoopaTroopa
    }

    [SerializeField] private EnemyType enemy;
    private int score;

    private bool idle;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        idle = true;
        moveDir = Vector2.left;
        
        switch (enemy)
        {
            case EnemyType.Goomba:
                score = 100;
                break;
            case EnemyType.KoopaTroopa:
                score = 200;
                break;
        }
    }

    private void OnBecameVisible()
    {
        idle = false;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void FixedUpdate()
    {
        if (idle) return;
        Vector2 origin = collider.bounds.center;
        origin.x += moveDir.x * (collider.bounds.extents.x + 0.01f);
        RaycastHit2D hit = Physics2D.Raycast(origin, moveDir, wallCheckDist);
        if(hit.collider && !hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Projectile")) moveDir *= -1f;
        rb.linearVelocity = new Vector2(moveDir.x * movespeed, rb.linearVelocityY);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = other.contacts[0];
            /*if(game state = invincible)
            {
                Vector2 knockbackDir = new Vector2(contact.normal.x, 0);
                dieKnockback(knockbackDir);
            } else
            */
            if (contact.normal.y > 0.5)
            {
                //above -> stomp
                DieStomp();

            } else
            {
                // left, right or below -> damage player
                
            }
        } else if (other.gameObject.CompareTag("Projectile")) //projectile being a fireball or shell
        {
            ContactPoint2D contact = other.contacts[0];
            Vector2 knockbackDir = new Vector2(contact.normal.x, 0);
            //if fireball -> destroy(fireball)
            DieKnockback(knockbackDir);
        }
    }

    private void DieStomp()
    {
        rb.linearVelocity = Vector2.zero;
        collider.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        switch (enemy)
        {
            case EnemyType.Goomba:
                //stomp anim
                break;
            case EnemyType.KoopaTroopa:
                //change to shell anim?
                //create shell object
                break;
        }
        //give score
        Despawn();
    }

    private void DieKnockback(Vector2 dir)
    {
        rb.linearVelocity = Vector2.zero;
        collider.enabled = false;
        rb.AddForce(dir * knockbackForceX + Vector2.up * knockbackForceY, ForceMode2D.Impulse);
        //give score
        Despawn();
    }

    private void Despawn() //use when enemy falls out of level to not give score
    {
        Destroy(gameObject, deathDelay);
    }
}
