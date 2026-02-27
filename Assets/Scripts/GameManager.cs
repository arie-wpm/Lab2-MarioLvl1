using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public ColorChanger colorChanger;

    [Header("Player Scripts")]
    public GameObject player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        colorChanger.ChangeToDefault(player.GetComponentsInChildren<SpriteRenderer>());
    }

    void Start()
    {
        StartCoroutine(RestartGame());
    }

    public static IEnumerator RestartGame()
    {
        StateManager.SetStartState();
        yield return new WaitForSeconds(5f);
        StateManager.SetPlayState();
    }
}
