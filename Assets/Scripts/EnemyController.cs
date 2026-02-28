using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IBumpable
{
    private Vector2 moveDir;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private UIManager _uiManager;
    
    [SerializeField] private EnemyType enemy;
    [SerializeField] private int score;
    [SerializeField] private float movespeed = 2f;
    [SerializeField] private float wallCheckDist = 0.1f;
    [SerializeField] private float deathDelay = 1f;
    private float instantDespawn = 0f;
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
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _uiManager = GameObject.FindWithTag("GameManager").GetComponent<UIManager>();
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

    private void OnBecameInvisible()
    {
        if (idle) return;
        Despawn(instantDespawn);
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
        Vector2 origin = col.bounds.center;
        origin.x += moveDir.x * (col.bounds.extents.x + 0.01f);
        RaycastHit2D hit = Physics2D.Raycast(origin, moveDir, wallCheckDist);

        if (hit.collider && hit.collider.CompareTag("Respawn")) return;

        if (koopaState == KoopaState.ShellMoving)
        {
            if (hit.collider && !hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Fireball") && !hit.collider.CompareTag("Shell") && !hit.collider.CompareTag("Enemy"))
            {
                moveDir *= -1f;
            }
            rb.linearVelocity = new Vector2(moveDir.x * ms, rb.linearVelocityY);
        }
        else
        {
            if (hit.collider && !hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Fireball") && !hit.collider.CompareTag("Shell"))
            {
                moveDir *= -1f;
                spriteRenderer.flipX = !spriteRenderer.flipX;
            }
            rb.linearVelocity = new Vector2(moveDir.x * ms, rb.linearVelocityY);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = other.gameObject.GetComponent<PlayerStats>();
            float enemyTopY = col.bounds.max.y;
            float playerBotY = other.collider.bounds.min.y;
            
            if (playerStats.isInvincible)
            {
                Vector2 knockbackDir = other.transform.position.x < transform.position.x ? Vector2.right : Vector2.left;
                DieKnockback(knockbackDir);
            } 
            else if (playerBotY > enemyTopY) //above -> stomp
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
        } else if (other.gameObject.CompareTag("Fireball") || other.gameObject.CompareTag("Shell"))
        {
            Vector2 knockbackDir = other.transform.position.x < transform.position.x ? Vector2.right : Vector2.left;
            DieKnockback(knockbackDir);
        }
    }

    private void GoombaStomp()
    {
        GiveScore();
        AudioManager.Instance.Play("stomp");
        isDead = true;
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        anim.SetBool("isMoving", false);
        anim.SetBool("isDead", true);
        Despawn(deathDelay);
    }

    private void KoopaStomp(Transform player)
    {
        switch (koopaState)
        {
            case KoopaState.Walking:
                AudioManager.Instance.Play("stomp");
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
        AudioManager.Instance.Play("stomp");
        rb.linearVelocityX = 0f;
        koopaState = KoopaState.ShellIdle;
        gameObject.tag = "Enemy";
        koopaReformCoroutine = StartCoroutine(ShellReform());
    }

    private void StartMoveShell(Transform player)
    {
        AudioManager.Instance.Play("kick");
        koopaState = KoopaState.ShellMoving;
        gameObject.tag = "Shell";
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
        GiveScore();
        AudioManager.Instance.Play("kick");
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;
        rb.AddForce(dir * knockbackForceX + Vector2.up * knockbackForceY, ForceMode2D.Impulse);
        anim.SetBool("isMoving", false);
        anim.SetBool("isDead", true);
        Despawn(deathDelay);
    }

    private void Despawn(float delay)
    {
        Destroy(gameObject, delay);
    }

    private void GiveScore()
    {
        ScoreManager.AddScoreWithModifier(score, transform.position + new Vector3(0, col.bounds.extents.y, 0));
    }


    public void OnBump()
    {
        if (isDead) return;
        
        DieKnockback(new Vector2(-0.3f, 0f));
    }
}