using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour, IBumpable
{
    private Vector2 moveDir;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private UIManager _uiManager;

    [Header("Enemy Type Vars")]
    [SerializeField]
    private EnemyType enemy;

    [SerializeField]
    private int score;

    [SerializeField]
    private float movespeed = 2f;

    [SerializeField]
    private float wallCheckDist = 0.1f;

    [SerializeField]
    private LayerMask groundLayers;

    [Header("Death Vars")]
    [SerializeField]
    private float deathDelay = 1f;
    private float instantDespawn = 0f;

    [SerializeField]
    private float knockbackForceX;

    [SerializeField]
    private float knockbackForceY;

    [Header("Koopa Shell Vars")]
    [SerializeField]
    private float shellMovespeed = 6f;

    [SerializeField]
    private float shellReformDelay = 5f;

    [SerializeField]
    private float shellReformTime = 2f;

    private float camPadding = 4f;
    private Camera mainCam;

    private enum EnemyType
    {
        Goomba,
        KoopaTroopa,
    }

    private bool isDead;

    private enum KoopaState
    {
        Walking,
        ShellMoving,
        ShellIdle,
    }

    private KoopaState koopaState;
    private Coroutine koopaReformCoroutine;
    private bool isActive = false;

    [SerializeField]
    private float movingShellGracePeriod;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _uiManager = GameObject.FindWithTag("GameManager").GetComponent<UIManager>();
        isDead = false;
        moveDir = Vector2.left;
        koopaState = KoopaState.Walking;
        mainCam = Camera.main;
    }

    void Update()
    {
        CheckCameraBounds();
    }

    void CheckCameraBounds()
    {
        Vector3 camMin = mainCam.ViewportToWorldPoint(new Vector3(0, 0, mainCam.nearClipPlane));
        Vector3 camMax = mainCam.ViewportToWorldPoint(new Vector3(1, 0, mainCam.nearClipPlane));
        Vector3 pos = transform.position;

        float leftBound = camMin.x - camPadding;
        float rightBound = camMax.x + camPadding;

        bool inside = transform.position.x >= leftBound && transform.position.x <= rightBound;

        if (inside && !isActive)
            Activate();
        else if (!inside && isActive)
            Deactivate();
    }

    void Activate()
    {
        if (GameManager.Instance.isUnderground)
            return;
        isActive = true;
        movespeed = 2f;
        anim.SetBool("isMoving", true);
    }

    void Deactivate()
    {
        if (GameManager.Instance.isUnderground)
            return;
        isActive = false;
        movespeed = 0f;
        anim.SetBool("isMoving", false);
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;
        if (!isActive)
            return;
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
        RaycastHit2D hit = Physics2D.Raycast(origin, moveDir, wallCheckDist, groundLayers);

        if (hit.collider && hit.collider.CompareTag("Respawn"))
            return;

        if (koopaState == KoopaState.ShellMoving)
        {
            if (hit.collider)
            {
                moveDir *= -1f;
            }
        }
        else
        {
            if (hit.collider)
            {
                moveDir *= -1f;
                spriteRenderer.flipX = !spriteRenderer.flipX;
            }
        }
        rb.linearVelocity = new Vector2(moveDir.x * ms, rb.linearVelocityY);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "BottomDeathBox")
            Despawn(0f);
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
                Vector2 knockbackDir =
                    other.transform.position.x < transform.position.x
                        ? Vector2.right
                        : Vector2.left;
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
            }
            else
            {
                if (koopaState == KoopaState.ShellIdle)
                {
                    StartCoroutine(StartMoveShell(other.transform));
                }
                //damage player -> player gets i-frames
                if (koopaState == KoopaState.ShellMoving)
                {
                    other.gameObject.GetComponent<PlayerController>().Die();
                }
            }
        }
        else if (other.gameObject.CompareTag("Fireball") || other.gameObject.CompareTag("Shell"))
        {
            Vector2 knockbackDir =
                other.transform.position.x < transform.position.x ? Vector2.right : Vector2.left;
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
        anim.SetBool("isStomped", true);
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
                StartCoroutine(StartMoveShell(player));
                break;
        }
    }

    private void ShellTransform()
    {
        rb.linearVelocityX = 0f;
        koopaState = KoopaState.ShellIdle;
        anim.SetBool("isMoving", false);
        anim.SetBool("isShell", true);

        groundLayers &= ~(1 << LayerMask.NameToLayer("Enemy"));

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

    IEnumerator StartMoveShell(Transform player)
    {
        AudioManager.Instance.Play("kick");
        gameObject.tag = "Shell";
        if (koopaReformCoroutine != null)
            StopCoroutine(koopaReformCoroutine);
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
        yield return new WaitForSeconds(movingShellGracePeriod);
        koopaState = KoopaState.ShellMoving;
    }

    private IEnumerator ShellReform()
    {
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
        groundLayers |= 1 << LayerMask.NameToLayer("Enemy");
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
        ScoreManager.AddScoreWithModifier(
            score,
            transform.position + new Vector3(0, col.bounds.extents.y, 0)
        );
    }

    public void OnBump()
    {
        if (isDead)
            return;

        DieKnockback(new Vector2(-0.3f, 0f));
    }

    public bool isKoopaMoving()
    {
        return koopaState != KoopaState.ShellIdle;
    }
}
