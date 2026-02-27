using UnityEngine;

public class RespawnCollider : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            GameManager.Instance._currentRespawnPoint = GameManager.Instance.respawnPoint2;
            GameManager.Instance._currentCamRespawnPoint = GameManager.Instance.CamRespawnPoint2;
        }
    }
}
