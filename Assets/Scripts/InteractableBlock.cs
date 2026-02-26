using System;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableBlock : MonoBehaviour
{
    private enum BlockState
    {
        QBlock,
        QBlockInvis,
        BrickBreakable,
        BrickUnbreakable
    }

    [SerializeField] private BlockState blockState;
    [SerializeField] private float destroyDelay;
    [SerializeField] private int hp = 1;
    
    [SerializeField] private GameObject heldPickup;
    [SerializeField] private GameObject upgradedPickup;
    
    private SpriteRenderer rend;
    private BoxCollider2D col;
    private Animator anim;
    
    void Start()
    {
        rend = gameObject.GetComponent<SpriteRenderer>();
        col = gameObject.GetComponent<BoxCollider2D>();
        anim = gameObject.GetComponent<Animator>();
        if (blockState == BlockState.QBlockInvis)
        {
            rend.enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            float blockBotY = col.bounds.min.y;
            float playerTopY = other.collider.bounds.max.y;
            bool playerIsBelow = blockBotY > playerTopY;
            
            if (playerIsBelow)
            {
                PlayerStats playerStats = other.gameObject.GetComponent<PlayerStats>();
                Debug.Log("player form: "+playerStats.powerState);
                switch (playerStats.powerState)
                {
                    case MarioPowerState.Small:
                        if (blockState == BlockState.QBlock || blockState == BlockState.QBlockInvis)
                        {
                            HitBlock();
                        }
                        else
                        {
                            //bump hit anim, no hit function
                        }
                        break;
                    case MarioPowerState.Super:
                        HitBlock();
                        break;
                    case MarioPowerState.Fire:
                        HitBlock();
                        break;
                }
                
            }
        }
    }

    private void HitBlock()
    {
        if (blockState == BlockState.QBlockInvis)
        {
            rend.enabled = true;
            blockState = BlockState.QBlock;
        }
        
        //bump hit anim
        hp -= 1;
        if (heldPickup)
        {
            SpawnPickup();
        }

        switch (blockState)
        {
            case BlockState.QBlock:
                SetInactive();
                break;
            case BlockState.BrickBreakable:
                BreakableBlock();
                break;
            case BlockState.BrickUnbreakable:
                SetInactive();
                break;
        }
    }

    private void BreakableBlock()
    {
        if (hp != 0) return;
        //anim -> break
        Destroy(gameObject, destroyDelay);
    }

    private void SetInactive()
    {
        if (hp != 0) return;
        anim.SetBool("isDepleted", true); //fix animation
        this.enabled = false;
    }

    private void SpawnPickup()
    {
        
    }
}
