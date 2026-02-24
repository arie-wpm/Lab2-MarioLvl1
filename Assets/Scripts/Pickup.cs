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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp) return;
        if (!other.CompareTag("Player")) return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null)
        {
            Debug.LogWarning("PlayerStats script missing");
            return;
        }

        pickedUp = true;

        Debug.Log($"Picked up: {type} | Object: {gameObject.name}");

        ApplyPickup(stats);

        Debug.Log($"Stats -> Coins: {stats.coins} | Lives: {stats.lives} | PowerState: {stats.powerState}");

        Destroy(gameObject);
    }

    private void ApplyPickup(PlayerStats stats)
    {
        switch (type)
        {
            case PickupType.Coin:
                stats.AddCoins(amount);
                break;

            case PickupType.OneUp:
                stats.AddLife(1);
                break;

            case PickupType.SuperMushroom:
                stats.SetPowerState(MarioPowerState.Super);
                break;

            case PickupType.FireFlower:
                if (stats.powerState == MarioPowerState.Small)
                {
                    stats.SetPowerState(MarioPowerState.Super);
                }
                else if (stats.powerState == MarioPowerState.Super)
                {
                    stats.SetPowerState(MarioPowerState.Fire);
                }
                break;

            case PickupType.Star:
                stats.ActivateStar(starDuration);
                break;
        }
    }
}