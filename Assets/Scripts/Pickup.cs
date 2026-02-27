using System.Collections.Generic;
using UnityEngine;

public enum PickupType
{
    Coin,
    SuperMushroom,
    FireFlower,
    Star,
    OneUp
}

public class Pickup : MonoBehaviour
{
    public PickupType type;
    public int amount = 1;
    public float starDuration = 6f;

    private bool pickedUp;
    private List<GameObject> _mushroomToFlower = new List<GameObject>();
    private PlayerStats _playerStats;

    void Start()
    {
        _playerStats = GameManager.Instance.player.GetComponent<PlayerStats>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (pickedUp) return;
        if (!other.gameObject.CompareTag("Player")) return;

        if (_playerStats == null)
        {
            Debug.LogWarning("PlayerStats script missing");
            return;
        }

        pickedUp = true;

        Debug.Log($"Picked up: {type} | Object: {gameObject.name}");

        ApplyPickup(_playerStats);

        Debug.Log($"Stats -> Coins: {_playerStats.coins} | Lives: {_playerStats.lives} | PowerState: {_playerStats.powerState}");

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp) return;
        if (!other.gameObject.CompareTag("Player")) return;

        if (_playerStats == null)
        {
            Debug.LogWarning("PlayerStats script missing");
            return;
        }

        pickedUp = true;

        Debug.Log($"Picked up: {type} | Object: {gameObject.name}");

        ApplyPickup(_playerStats);

        Debug.Log($"Stats -> Coins: {_playerStats.coins} | Lives: {_playerStats.lives} | PowerState: {_playerStats.powerState}");

        Destroy(gameObject);
    }

    public void ApplyPickup()
    {
        ApplyPickup(_playerStats);
    }

    private void ApplyPickup(PlayerStats stats)
    {
        switch (type)
        {
            case PickupType.Coin:
                stats.AddCoins(amount);
                break;

            case PickupType.OneUp:
                AudioManager.Instance.Play("1up");
                stats.AddLife(1);
                break;

            case PickupType.SuperMushroom:
                AudioManager.Instance.Play("powerup");
                GameManager.Instance.colorChanger.StartTransformFreeze();
                stats.SetPowerState(MarioPowerState.Super);
                break;

            case PickupType.FireFlower:
                AudioManager.Instance.Play("powerup");
                GameManager.Instance.colorChanger.StartTransformFreeze();
                stats.SetPowerState(MarioPowerState.Fire);
                break;

            case PickupType.Star:
                stats.ActivateStar(starDuration);
                break;
        }
    }
}