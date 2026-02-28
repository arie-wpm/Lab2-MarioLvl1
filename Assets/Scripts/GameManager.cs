using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static int Timer = 400;

    private float timeUnitIncrementSize = 0.4f;
    private float currentTimeUnitPos = 0.4f;
    public ColorChanger colorChanger;

    public GameObject player;
    public StateManager.GameState currentGameState;

    [SerializeField]
    private GameObject StartScreenObj;

    [Header("TitleScreenObj")]
    [SerializeField]
    private GameObject player1GameObj;

    [SerializeField]
    private GameObject player2GameObj;

    [SerializeField]
    private GameObject selectorObj;

    [SerializeField]
    private float _blackScreenDuration = 2.5f;

    [Header("Respawn Locations")]
    public Camera mainCamera;
    public Camera underGroundCamera;
    public Transform respawnPoint1;
    public Transform respawnPoint2;
    public Transform CamRespawnPoint1,
        CamRespawnPoint2;
    public BoxCollider2D respawnTriggerBox;

    public Transform _currentRespawnPoint;
    public Transform _currentCamRespawnPoint;

    [Header("Blocks")]
    public GameObject interactables;

    [Header("SpawnedObjects")]
    public GameObject spawnedGrouping;
    public GameObject allEnemies;
    public GameObject goombaPrefab;
    public GameObject koopaPrefab;

    private List<Vector2> _enemyTransform = new();
    private List<int> _enemyType = new();

    private bool isGameOver = false;
    private bool isFirstStart = true;
    public bool hasTimeRunOut = false;

    public GameObject blackPanel;
    public GameObject gameOverPanel;
    public GameObject timeUpPanel;

    // using Attack as Start for now since default is Enter
    private InputAction _moveAction => InputSystem.actions.FindAction("Move");
    private InputAction _startAction => InputSystem.actions.FindAction("Next");

    // private InputAction _pauseAction => InputSystem.actions.FindAction("Pause");
    private bool _selectTrack = true;

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
        StartScreenObj.SetActive(true);
        _currentRespawnPoint = respawnPoint1;
        _currentCamRespawnPoint = CamRespawnPoint1;

        // store all enemy positions and type
        Transform[] enemies = allEnemies.GetComponentsInChildren<Transform>().Skip(1).ToArray();

        foreach (Transform enemy in enemies)
        {
            _enemyTransform.Add(enemy.transform.position);
            if (enemy.name.Contains("Goomba"))
                _enemyType.Add(0);
            else
                _enemyType.Add(1);
        }
    }

    void Update()
    {
        currentGameState = StateManager.CurrentGameState();

        switch (StateManager.CurrentGameState())
        {
            case StateManager.GameState.NULL:
                UIManager ui = GetComponent<UIManager>();
                ui.DisableLivesScreen();
                UpdateInStartScreen();
                break;
            case StateManager.GameState.StartScreen:
                StartScreenObj.SetActive(false);
                SetMarioPosition();
                break;
            case StateManager.GameState.Play:
                HandlePlayBGMOnce();
                UpdateInPlayMode();
                break;
            case StateManager.GameState.PauseScreen:
                UpdateInPauseScreen();
                break;
            case StateManager.GameState.Dead:
                UpdateInDeadMode();
                break;
        }
    }

    void HandlePlayBGMOnce()
    {
        if (isFirstStart)
        {
            AudioManager.Instance.PlayBGM();
            isFirstStart = false;
        }
    }

    void SetMarioPosition()
    {
        CameraFollow camFollow = mainCamera.GetComponent<CameraFollow>();
        camFollow.SetCamX(_currentCamRespawnPoint.transform.position.x);
        camFollow.SetLeftEdge(_currentCamRespawnPoint.transform.position.x - 7.5f);
        player.transform.position = _currentRespawnPoint.transform.position;
        Camera.main.transform.position = _currentCamRespawnPoint.transform.position;
    }

    void UpdateInStartScreen()
    {
        StartScreenObj.SetActive(true);
        // replicating select button here
        Vector2 move = _moveAction.ReadValue<Vector2>();
        if (Mathf.Abs(move.x) > 0f)
            return;

        if (_moveAction.WasPressedThisFrame())
        {
            _selectTrack = !_selectTrack;
            Vector3 selectorPos = selectorObj.transform.position;
            if (_selectTrack)
            {
                selectorPos.y = player1GameObj.transform.position.y;
            }
            else
            {
                selectorPos.y = player2GameObj.transform.position.y;
            }
            selectorObj.transform.position = selectorPos;
        }

        if (_startAction.WasPressedThisFrame())
        {
            // no logic here yet but we can add luigi colors if time permits
            // note: need to add a coroutine here to switch to BlackScreen
            StartCoroutine(RestartLevel()); //
        }
    }

    void UpdateInPlayMode()
    {
        // hacking in time out for now
        if (Timer <= 0f)
        {
            hasTimeRunOut = true;
            RunTimeOut();
        }

        StartScreenObj.SetActive(false);
        Time.timeScale = GameManager.Instance.colorChanger.currentTimeScale;
        if (StateManager.CurrentGameState() != StateManager.GameState.Play)
            return;

        // check for Input Pause
        if (_startAction.WasPressedThisFrame())
        {
            AudioManager.Instance.Play("pause");
            AudioManager.Instance.PauseBGM();
            StateManager.SetPauseState();
            player.GetComponent<Animator>().speed = 0f;
        }

        //Checks if mario time unit has passed (0.4f) then decrements visual timer if it has.
        if (currentTimeUnitPos >= timeUnitIncrementSize)
        {
            Timer--;
            currentTimeUnitPos = 0;
        }
        else
        {
            currentTimeUnitPos += Time.deltaTime;
        }
    }

    void RunTimeOut()
    {
        StateManager.SetDeadState();
        PlayerStats pStats = player.GetComponent<PlayerStats>();
        PlayerController pController = player.GetComponent<PlayerController>();
        Animator animator = player.GetComponent<Animator>();
        BoxCollider2D mainCol = player.GetComponent<BoxCollider2D>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        pStats.lives--;
        animator.SetBool("isDead", true);
        mainCol.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        StartCoroutine(pController.MoveMarioDead());
    }

    void UpdateInPauseScreen()
    {
        StartScreenObj.SetActive(false);
        Time.timeScale = 0f;
        if (StateManager.CurrentGameState() != StateManager.GameState.PauseScreen)
            return;

        if (_startAction.WasPressedThisFrame())
        {
            AudioManager.Instance.Play("pause");
            AudioManager.Instance.ResumeBGM();
            StateManager.SetPlayState();
            player.GetComponent<Animator>().speed = 1f;
        }
    }

    void UpdateInDeadMode()
    {
        StartScreenObj.SetActive(false);
        if (StateManager.CurrentGameState() != StateManager.GameState.Dead)
            return;
        AudioManager.Instance.Play("death");
        AudioManager.Instance.StopBGM();
    }

    public IEnumerator RestartLevel()
    {
        StateManager.SetStartState();
        ResetSceneObjects();
        yield return new WaitForSeconds(_blackScreenDuration);
        StateManager.SetPlayState();
    }

    public IEnumerator TimeRanOut()
    {
        yield return new WaitForSeconds(_blackScreenDuration);
        yield return StartCoroutine(OtherBlackScreen(_blackScreenDuration));
        ResetSceneObjects();
    }

    public IEnumerator RestartGame()
    {
        isGameOver = true;
        yield return StartCoroutine(OtherBlackScreen(_blackScreenDuration));
        ResetSceneObjects();
    }

    public void ResetSceneObjects()
    {
        // reset position
        if (isGameOver)
        {
            _currentRespawnPoint = respawnPoint1;
            _currentCamRespawnPoint = CamRespawnPoint1;
            player.transform.position = _currentRespawnPoint.transform.position;
            Camera.main.transform.position = _currentCamRespawnPoint.transform.position;
            // resetColor and State
            colorChanger.ChangeToDefault(player.GetComponentsInChildren<SpriteRenderer>());
            player.GetComponent<PlayerStats>().Reset();
            StateManager.CurrentState = StateManager.GameState.NULL;
            // ScoreManager score reset
            isGameOver = false;
        }

        //reset blocks
        InteractableBlock[] blocks = interactables.GetComponentsInChildren<InteractableBlock>();
        foreach (InteractableBlock block in blocks)
            block.Reset();

        // reset enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
            Destroy(enemy);

        for (int i = 0; i < _enemyTransform.Count; i++)
        {
            if (_enemyType[i] == 0)
                Instantiate(goombaPrefab, _enemyTransform[i], Quaternion.identity);
            else
                Instantiate(koopaPrefab, _enemyTransform[i], Quaternion.identity);
        }

        isFirstStart = true;
        hasTimeRunOut = false;
        Timer = 400;
    }

    IEnumerator OtherBlackScreen(float duration)
    {
        if (isGameOver)
        {
            blackPanel.SetActive(true);
            gameOverPanel.SetActive(true);
            timeUpPanel.SetActive(false);
        }
        else if (hasTimeRunOut)
        {
            blackPanel.SetActive(true);
            timeUpPanel.SetActive(true);
            gameOverPanel.SetActive(false);
        }
        yield return new WaitForSeconds(duration);
        blackPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        timeUpPanel.SetActive(false);
    }
}
