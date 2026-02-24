using UnityEngine;

public class StateManager : MonoBehaviour
{
    public enum GameState
    {
        StartScreen,
        Play,
        PauseScreen,

        Invinsible,

        FireFlower,
        Dead,
        Won,
    }

    public static GameState CurrentState;

    void Start()
    {
        CurrentState = GameState.StartScreen;
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
    }

    public static void SetStartState()
    {
        if (CurrentState == GameState.StartScreen)
            return;
        CurrentState = GameState.StartScreen;
    }

    public static void SetPauseState()
    {
        if (CurrentState == GameState.PauseScreen)
            return;
        CurrentState = GameState.PauseScreen;
    }

    public static void SetInvincibleState()
    {
        if (CurrentState == GameState.Invinsible)
            return;
        CurrentState = GameState.Invinsible;
    }

    public static void SetFireFlowerState()
    {
        if (CurrentState == GameState.FireFlower)
            return;
        CurrentState = GameState.FireFlower;
    }

    public static void SetDeadState()
    {
        if (CurrentState == GameState.Dead)
            return;
        CurrentState = GameState.Dead;
    }

    public static void SetWinState()
    {
        if (CurrentState == GameState.Won)
            return;
        CurrentState = GameState.Won;
    }
}
