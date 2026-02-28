using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PipeTeleport : MonoBehaviour
{
    [SerializeField] private Transform _uEntryPoint;
    [SerializeField] private Transform _oExitPoint;
    [SerializeField] private Transform _oExitFinalPoint;
    [SerializeField] private Transform _uExitCamPos;
    [SerializeField] private float _transitionDuration = 1.5f;
    [SerializeField] private float _blackScreenDuration = 0.25f;

    [SerializeField] private Camera _mainCam;
    [SerializeField] private Camera _undergroundCam;
    [SerializeField] private CameraFollow _camFollowScript;

    [SerializeField] Canvas _uiPanel;
    [SerializeField] private GameObject _blackPanel;
    [SerializeField] private GameObject _topUI;

    private GameObject _player;
    private Coroutine _uEntry;
    private Coroutine _oEntry;
    private Coroutine _oExit;

    private Rigidbody2D _playerRb;
    private BoxCollider2D _playerCol;
    private Animator _playerAnimator;
    private PlayerController _playerController;
    private SpriteRenderer[] _playerSpriteRenderer;
    private int _spriteSortingOrder;

    private InputAction _crouchAction;
    private bool _playerInPipe = false;

    private void Start() {
        _player = GameManager.Instance.player;
        _crouchAction = InputSystem.actions.FindAction("Crouch");;

        _playerRb = _player.GetComponent<Rigidbody2D>();
        _playerCol = _player.GetComponent<BoxCollider2D>();
        _playerAnimator = _player.GetComponent<Animator>();
        _playerController = _player.GetComponent<PlayerController>();
        _playerSpriteRenderer = _player.GetComponentsInChildren<SpriteRenderer>();
        _spriteSortingOrder = _playerSpriteRenderer[0].sortingOrder;

        // damn button press timing
        _crouchAction.performed += ctx => {
            if (_playerInPipe && _playerController.grounded) _uEntry = StartCoroutine(UEntry());
        };
    }

    void OnDestroy() {
        if (_crouchAction != null) {
            _crouchAction.performed -= ctx => {
                if (_playerInPipe && _playerController.grounded) _uEntry = StartCoroutine(UEntry());
                };
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player" && gameObject.name == "PipeEnter") _playerInPipe = true;
    }

    void OnTriggerExit2D(Collider2D other) { 
        if (other.gameObject.tag == "Player" && gameObject.name == "PipeEnter") _playerInPipe = false;
    }

    void OnTriggerStay2D(Collider2D other) {
        // this pipe requires no input
        if (other.gameObject.tag == "Player" && gameObject.name == "UndergroundPipeEnter") {
            _oEntry = StartCoroutine(OEntry());    
            return;
        }
    }

    IEnumerator BriefBlackScreen(Transform transform, bool isUnderground) {
        _topUI.SetActive(false);
        _blackPanel.SetActive(true);
        
        AudioManager.Instance.StopBGM();

        SwitchCameras(isUnderground);
        _player.transform.position = transform.position;
        if (isUnderground) _mainCam.transform.position = _uExitCamPos.position;

        yield return new WaitForSeconds(_blackScreenDuration);

        if (isUnderground) AudioManager.Instance.PlayBGMUnderground();
        else AudioManager.Instance.PlayBGM();

        _blackPanel.SetActive(false);
        _topUI.SetActive(true);
    }

    IEnumerator UEntry() {
        AudioManager.Instance.Play("pipe");
        TogglePlayerComponents(false);
        SetAllAnimParamsFalse();
        if (_player.GetComponent<PlayerStats>().powerState != MarioPowerState.Small)
            _playerAnimator.SetBool("isCrouching", true);

        Vector2 startPos = _player.transform.position;
        Vector2 endPos = new Vector2(startPos.x, startPos.y - 2f);

        float duration = _transitionDuration;
        float timer = 0f;

        while (timer < duration) {
            timer += Time.deltaTime;
            _player.transform.position = Vector2.Lerp(startPos, endPos, timer / duration);
            yield return null;
        }
        _player.transform.position = endPos;

        yield return StartCoroutine(BriefBlackScreen(_uEntryPoint, true));

        TogglePlayerComponents(true);
        _playerController.facingDirection = 1;
        _uEntry = null;
    }

    IEnumerator OEntry() {
        AudioManager.Instance.Play("pipe");
        TogglePlayerComponents(false);
        SetAllAnimParamsFalse();
        _playerAnimator.SetBool("isMoving", true);

        Vector2 startPos = _player.transform.position;
        Vector2 endPos = new Vector2(startPos.x + 2f, startPos.y);

        float duration = _transitionDuration;
        float timer = 0f;

        while (timer < duration) {
            timer += Time.deltaTime;
            _player.transform.position = Vector2.Lerp(startPos, endPos, timer / duration);
            yield return null;
        }
        _player.transform.position = endPos;

        yield return StartCoroutine(BriefBlackScreen(_oExitPoint, false));
        yield return StartCoroutine(OutOfPipeAnim());

        TogglePlayerComponents(true);
        _playerController.facingDirection = 1;
        _oEntry = null;
    }

    IEnumerator OutOfPipeAnim() {
        _playerAnimator.SetBool("isMoving", false);
        Vector2 startPos = _player.transform.position;
        Vector2 endPos = _oExitFinalPoint.position;

        float duration = _transitionDuration;
        float timer = 0f;

        while (timer < duration) {
            timer += Time.deltaTime;
            _player.transform.position = Vector2.Lerp(startPos, endPos, timer / duration);
            yield return null;
        }
        _player.transform.position = endPos;
    }

    void SwitchCameras(bool switchCam) {
        _undergroundCam.gameObject.SetActive(switchCam);
        _camFollowScript.enabled = !switchCam;

        if (switchCam) _uiPanel.worldCamera = _undergroundCam;
        else _uiPanel.worldCamera = _mainCam;
    }

    void TogglePlayerComponents(bool toggle) {
        if (toggle) {
            foreach (SpriteRenderer sr in _playerSpriteRenderer) {
                sr.sortingOrder = _spriteSortingOrder;
            }
        } else {
            foreach (SpriteRenderer sr in _playerSpriteRenderer) {
                sr.sortingOrder = -5;
            }
        }
        _playerAnimator.SetBool("isMoving", false);
        _playerAnimator.SetBool("isJumping", false);
        _playerRb.linearVelocity = Vector2.zero;
        _playerRb.simulated = toggle;
        _playerCol.enabled = toggle;
        _playerController.enabled = toggle;
    }

    void SetAllAnimParamsFalse() {
        _playerAnimator.SetBool("isMoving", false);
        _playerAnimator.SetBool("isJumping", false);
        _playerAnimator.SetBool("isSliding", false);
    } 
}
