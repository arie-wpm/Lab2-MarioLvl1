using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Vector2 moveDir;
    private Rigidbody2D rb;
    private Collider2D collider;
    private Animator anim;
    
    [SerializeField] private EnemyType enemy;
    [SerializeField] private int score;
    [SerializeField] private float movespeed = 2f;
    [SerializeField] private float wallCheckDist = 0.1f;
    [SerializeField] private float deathDelay = 2f;
    [SerializeField] private float knockbackForceX;
    [SerializeField] private float knockbackForceY;
    
    [SerializeField] private float shellMovespeed = 6f;
    [SerializeField] private float shellReformDelay = 5f;
    [SerializeField] private float shellReformTime = 2f;
    
    private enum EnemyType
    {
        Goomba,
        KoopaTroopa
    }
    private bool idle;
    private bool isDead;
    private enum KoopaState
    {
        Walking,
        ShellMoving,
        ShellIdle
    }

    private KoopaState koopaState;
    private Coroutine koopaReformCoroutine;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        idle = true;
        isDead = false;
        moveDir = Vector2.left;
        koopaState = KoopaState.Walking;
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
        if (isDead) return; 
        if (idle) return;
        if (enemy == EnemyType.KoopaTroopa)
        {
            switch (koopaState)
            {
                case KoopaState.Walking:
                    Move(movespeed);
                    break;
                case KoopaState.ShellMoving:
                    Move(shellMovespeed);
                    break;
                case KoopaState.ShellIdle:
                    rb.linearVelocityX = 0f;
                    break;
            }
        }
        else
        {
            Move(movespeed);
        }
    }

    private void Move(float ms)
    {
        Vector2 origin = collider.bounds.center;
        origin.x += moveDir.x * (collider.bounds.extents.x + 0.01f);
        RaycastHit2D hit = Physics2D.Raycast(origin, moveDir, wallCheckDist);
        
        if (hit.collider && !hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Projectile"))
        {
            moveDir *= -1f;
        }
        rb.linearVelocity = new Vector2(moveDir.x * ms, rb.linearVelocityY);
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
                        GoombaStomp();
                        break;
                    case EnemyType.KoopaTroopa:
                        KoopaStomp(other.transform);
                        break;
                }

            } else
            {
                if (koopaState == KoopaState.ShellIdle)
                {
                    StartMoveShell(other.transform);
                }
                //damage player -> player gets i-frames
            }
        } else if (other.gameObject.CompareTag("Projectile")) //projectile being a fireball or shell
        {
            ContactPoint2D contact = other.contacts[0];
            Vector2 knockbackDir = new Vector2(contact.normal.x, 0);
            //if fireball -> destroy(fireball)
            DieKnockback(knockbackDir);
        }
    }

    private void GoombaStomp() //only used for Goomba
    {
        
        rb.linearVelocity = Vector2.zero;
        collider.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        anim.SetBool("isMoving", false);
        anim.SetBool("isDead", true);
        GiveScore();
        Despawn();
    }

    private void KoopaStomp(Transform player)
    {
        switch (koopaState)
        {
            case KoopaState.Walking:
                ShellTransform();
                break;
            case KoopaState.ShellMoving:
                StopShell();
                break;
            case KoopaState.ShellIdle:
                StartMoveShell(player);
                break;
        }
    }
    private void ShellTransform()
    {
        rb.linearVelocityX = 0f;
        koopaState = KoopaState.ShellIdle;
        anim.SetBool("isMoving", false);
        anim.SetBool("isShell", true);
        
        koopaReformCoroutine = StartCoroutine(ShellReform());
    }

    private void StopShell()
    {
        rb.linearVelocityX = 0f;
        koopaState = KoopaState.ShellIdle;
        koopaReformCoroutine = StartCoroutine(ShellReform());
    }

    private void StartMoveShell(Transform player)
    {
        koopaState = KoopaState.ShellMoving;
        //change tag to projectile -> change back to enemy if stop moving
        if (koopaReformCoroutine != null) StopCoroutine(koopaReformCoroutine);
        anim.SetBool("isShell", true);
        anim.SetBool("isReforming", false);
        
        if (player.position.x < transform.position.x)
        {
            moveDir = Vector2.right;
        }
        else
        {
            moveDir = Vector2.left;
        }
    }
    
    private IEnumerator ShellReform(){
        yield return new WaitForSeconds(shellReformDelay);
        if (koopaState == KoopaState.ShellIdle)
        {
            anim.SetBool("isShell", false);
            anim.SetBool("isReforming", true);
        }
        yield return new WaitForSeconds(shellReformTime);
        if (koopaState == KoopaState.ShellIdle)
        {
            anim.SetBool("isReforming", false);
            anim.SetBool("isMoving", true);
        }
        koopaState = KoopaState.Walking;
        moveDir = Vector2.left;
    }

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