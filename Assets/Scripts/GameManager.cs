using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
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

    [Header("Respawn Locations")]
    public Camera mainCamera;
    public Camera underGroundCamera;
    public Transform respawnPoint1;
    public Transform respawnPoint2;
    public Transform CamRespawnPoint1, CamRespawnPoint2;
    public BoxCollider2D respawnTriggerBox;

    public Transform _currentRespawnPoint;
    public Transform _currentCamRespawnPoint;

    [Header("Blocks")]
    public GameObject interactables;

    [Header("SpawnedObjects")]
    public GameObject spawnedGrouping;

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
                AudioManager.Instance.PlayBGM();
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

    void SetMarioPosition() {
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
        yield return new WaitForSeconds(5f);
        StateManager.SetPlayState();
    }

    public IEnumerator RestartGame()
    {
        StateManager.SetStartState();
        yield return new WaitForSeconds(5f);
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ResetSceneObjects();
    }

    public void ResetSceneObjects() {

        // reset position
        _currentRespawnPoint = respawnPoint1;
        _currentCamRespawnPoint = CamRespawnPoint1;
        player.transform.position = _currentRespawnPoint.transform.position;
        Camera.main.transform.position = _currentCamRespawnPoint.transform.position;

        // resetColor and State
        colorChanger.ChangeToDefault(player.GetComponentsInChildren<SpriteRenderer>());
        player.GetComponent<PlayerStats>().Reset();
        StateManager.CurrentState = StateManager.GameState.NULL;
        // ScoreManager score reset

        //reset blocks
        InteractableBlock[] blocks = interactables.GetComponentsInChildren<InteractableBlock>();
        foreach (InteractableBlock block in blocks) block.Reset();
    }
}
