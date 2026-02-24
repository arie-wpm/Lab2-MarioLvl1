using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private float movespeed = 2f;
    private Vector2 moveDir;
    private Rigidbody2D rb;
    private Collider2D collider;
    private float wallCheckDist = 0.1f;
    
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
        bool wallCheck = Physics2D.Raycast(origin, moveDir, wallCheckDist);
        if (wallCheck) moveDir *= -1f;
        rb.linearVelocity = new Vector2(moveDir.x * movespeed, rb.linearVelocityY);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //if game state = invincible -> die()
            
            ContactPoint2D contact = other.contacts[0];
            if (contact.normal.y > 0.5)
            {
                //above -> stomp
                //stomp animation/sprite
                die();
            } else
            {
                // left, right or below -> damage player
                
            }
        } //else if(other.gameObject.CompareTag("fireball")) -> die()
    }

    public void die()
    {
        //death animation
        //return score?
        //destroy after delay
    }

    private void stomp()
    {
        
    }
}
