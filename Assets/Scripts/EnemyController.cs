using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float movespeed = 2f;
    private Vector2 moveDir;
    private Rigidbody2D rb;
    private Collider2D collider;
    private Animator anim;
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
    private bool isShell;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        idle = true;
        isShell = false;
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
        anim.SetBool("isMoving", true);
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
            if (contact.normal.y > 0.5) //above -> stomp
            {
                switch (enemy)
                {
                    case EnemyType.Goomba:
                        DieStomp();
                        break;
                    case EnemyType.KoopaTroopa:
                        ShellTransform();
                        break;
                }

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

    private void DieStomp() //only used for Goomba
    {
        
        rb.linearVelocity = Vector2.zero;
        collider.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        anim.SetBool("isMoving", false);
        anim.SetBool("isDead", true);
        GiveScore();
        Despawn();
    }

    private void ShellTransform()
    {
        rb.linearVelocity = Vector2.zero;
        if (isShell)
        {
            
            
        }
        else
        {
            anim.SetBool("isMoving", false);
            anim.SetBool("isShell", true);
            isShell = true;
        }
    }
    
    /*private IEnumerator ShellReform(float reformTime){
        anim.SetBool("isShell", false);
        anim.SetBool("isReforming", true);

        yield return new WaitForSeconds(reformTime);
        if (rb.linearVelocity == Vector2.zero)
        {
            anim.SetBool("isReforming", false);
        }
    }*/

    private void DieKnockback(Vector2 dir)
    {
        rb.linearVelocity = Vector2.zero;
        collider.enabled = false;
        rb.AddForce(dir * knockbackForceX + Vector2.up * knockbackForceY, ForceMode2D.Impulse);
        anim.SetBool("isMoving", false);
        anim.SetBool("isDead", true);
        GiveScore();
        Despawn();
    }

    private void Despawn() //use when enemy falls out of level to not give score
    {
        Destroy(gameObject, deathDelay);
    }

    private void GiveScore()
    {
        //score popup
        //+score to score manager
    }
}