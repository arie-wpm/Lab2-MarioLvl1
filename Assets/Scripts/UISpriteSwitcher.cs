using UnityEngine;
using UnityEngine.UI;

public class UISpriteSwitcher : MonoBehaviour {

    [Header("Numer Sprites")] 
    [SerializeField] private Sprite[] _numbers;

    [Header("Timer Sprite Ref")] 
    [SerializeField] private Image _digit1;
    [SerializeField] private Image _digit2;
    [SerializeField] private Image _digit3;

    [Header("Coins Sprite Ref ")]
    [SerializeField] private Image _coin1;
    [SerializeField] private Image _coin2;

    [Header("Score Sprite Ref ")]
    [SerializeField] private Image _score1;
    [SerializeField] private Image _score2;
    [SerializeField] private Image _score3;
    [SerializeField] private Image _score4;
    [SerializeField] private Image _score5;
    [SerializeField] private Image _score6;

    [Header("Lives Sprite Ref ")]
    [SerializeField] private Image _lives1;

    private int _time;
    private int _coins;
    private int _score;
    private int _lives;

    private PlayerStats playerStats;

    private void Start() {

        // hook up stuff
        playerStats = GameManager.Instance.player.GetComponent<PlayerStats>();
        _time = 0;
        _coins = 0;
        _score = 0;
        _lives = 0;
    }

    void Update()
    {
        if (StateManager.CurrentGameState() == StateManager.GameState.NULL) return;
        UpdateTime();
        UpdateCoins();
        UpdateScore();
        UpdateLives();
    }

    void UpdateTime() {
        if (_time < 0) {
            _time = Mathf.Max(0, _time);
            return;
        }
        _time = 100; // implement
        int digit1 = _time / 100;
        int digit2 = (_time / 10) % 10;
        int digit3 = _time % 10;

        _digit1.sprite = _numbers[digit1];
        _digit2.sprite = _numbers[digit2];
        _digit3.sprite = _numbers[digit3];
    }

    void UpdateCoins() {
        if (_coins < 0) {
            _coins = Mathf.Max(0, _coins);
            return;
        }
        _coins = playerStats.coins;
        _coin1.sprite = _numbers[_coins / 10];
        _coin2.sprite = _numbers[_coins % 10];
    }

    void UpdateScore() {
        if (_score < 0) {
            _score = Mathf.Max(0, _score);
            return;
        }
        _score = 1234;
        _score1.sprite = _numbers[_score / 100000];
        _score2.sprite = _numbers[(_score / 10000) % 10];
        _score3.sprite = _numbers[(_score / 1000) % 10];
        _score4.sprite = _numbers[(_score / 100) % 10];
        _score5.sprite = _numbers[(_score / 10) % 10];
        _score6.sprite = _numbers[_score % 10];
    }

    void UpdateLives() {
        if (_lives < 0) {
            _lives = Mathf.Max(0, _lives);
            return;
        }
        _lives = playerStats.lives;
        _lives1.sprite = _numbers[_lives];
    }
}
