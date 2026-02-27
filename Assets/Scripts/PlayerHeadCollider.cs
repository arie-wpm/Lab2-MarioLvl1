using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerHeadCollider : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerStats playerStats;
    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        playerStats = GetComponentInParent<PlayerStats>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 6)
        {
            if ((other.gameObject.CompareTag("Block") && playerStats.powerState == MarioPowerState.Small) || 
                (other.gameObject.CompareTag("QBlock") && other.GetComponent<InteractableBlock>() != null))
            {
                playerController.HeadButt("00"); //Soft
            }
            else
            {
                playerController.HeadButt("01"); //Hard
            }
            
        }
        else
        {
            playerController.HeadButt("01"); //Hard
        }
    }
}
