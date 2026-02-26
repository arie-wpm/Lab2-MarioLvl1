using System;
using UnityEngine;

public class PlayerFootCollider : MonoBehaviour
{
    private PlayerController playerController;
    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy") && playerController.rb.linearVelocityY < 0)
        {
            playerController.Stomp();
        }
    }
    
}
