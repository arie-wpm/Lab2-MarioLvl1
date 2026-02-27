using UnityEngine;

public class InvisColliderHider : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(true);
    }
    void Update()
    {
        PlayerStats playerStats = GameManager.Instance.player.GetComponent<PlayerStats>();
        if (playerStats.powerState == MarioPowerState.Small) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
