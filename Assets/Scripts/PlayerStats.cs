using UnityEngine;
using System.Collections;

public enum MarioPowerState
{
    Small,
    Super,
    Fire
}

public class PlayerStats : MonoBehaviour
{
    public int coins;
    public int lives = 3;
    public MarioPowerState powerState = MarioPowerState.Small;

    [Header("Star")]
    public bool isInvincible;
    private Coroutine starCoroutine;

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log("Coins: " + coins);
    }

    public void AddLife(int amount)
    {
        lives += amount;
        Debug.Log("Lives: " + lives);
    }

    public void SetPowerState(MarioPowerState newState)
    {
        powerState = newState;
        Debug.Log("Power State: " + powerState);
        // change animator
        // change collider size
        // enable fireball ability
    }

    public void ActivateStar(float duration)
    {
        if (starCoroutine != null) StopCoroutine(starCoroutine);
        starCoroutine = StartCoroutine(StarRoutine(duration));
    }

    private IEnumerator StarRoutine(float duration)
    {
        isInvincible = true;
        Debug.Log("Star: invincible ON");

        yield return new WaitForSeconds(duration);

        isInvincible = false;
        Debug.Log("Star: invincible OFF");
        starCoroutine = null;
    }
}