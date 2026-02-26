using UnityEngine;
using UnityEngine.Events;

public class StateManager : MonoBehaviour
{
    public enum GameState
    {
        StartScreen,
        Play,
        PauseScreen,
        Invincible,

        FireFlower,
        Dead,
        Won,
    }

    // State Related Events Invoked whenever a state is entered.
    public static GameState CurrentState;

    public static UnityEvent EnteredStartScreen;
    public static UnityEvent EnteredPlay;
    public static UnityEvent EnteredPauseScreen;
    public static UnityEvent EnteredInvincible;
    public static UnityEvent EnteredFireFlower;
    public static UnityEvent EnteredDead;
    public static UnityEvent EnteredWon;

    void OnEnable()
    {
        EnteredStartScreen ??= new UnityEvent();
        EnteredPlay ??= new UnityEvent();
        EnteredPauseScreen ??= new UnityEvent();
        EnteredInvincible ??= new UnityEvent();
        EnteredFireFlower ??= new UnityEvent();
        EnteredDead ??= new UnityEvent();
        EnteredWon ??= new UnityEvent();
    }

    void Start()
    {
        CurrentState = GameState.Play;
        EnteredWon.AddListener(PrintWin);
    }

    public static GameState CurrentGameState()
    {
        return CurrentState;
    }

    public static void SetPlayState()
    {
        if (CurrentState == GameState.Play)
            return;
        CurrentState = GameState.Play;
        EnteredPlay.Invoke();
    }

    public static void SetStartState()
    {
        if (CurrentState == GameState.StartScreen)
            return;
        CurrentState = GameState.StartScreen;
        EnteredStartScreen.Invoke();
    }

    public static void SetPauseState()
    {
        if (CurrentState == GameState.PauseScreen)
            return;
        CurrentState = GameState.PauseScreen;
        EnteredPauseScreen.Invoke();
    }

    public static void SetInvincibleState()
    {
        if (CurrentState == GameState.Invincible)
            return;
        CurrentState = GameState.Invincible;
        EnteredInvincible.Invoke();
    }

    public static void SetFireFlowerState()
    {
        if (CurrentState == GameState.FireFlower)
            return;
        CurrentState = GameState.FireFlower;
        EnteredFireFlower.Invoke();
    }

    public static void SetDeadState()
    {
        if (CurrentState == GameState.Dead)
            return;
        CurrentState = GameState.Dead;
        EnteredDead.Invoke();
    }

    public static void SetWinState()
    {
        if (CurrentState == GameState.Won)
            return;

        CurrentState = GameState.Won;
        EnteredWon.Invoke();
    }

    private void PrintWin()
    {
        Debug.Log("You Win");
    }
}
