using UnityEngine;
using UnityEngine.UI;

public class UISpriteSwitcher : MonoBehaviour
{
    [Header("Numer Sprites")]
    [SerializeField]
    private Sprite[] _numbers;

    [Header("Timer Sprite Ref")]
    [SerializeField]
    private Image _digit1;

    [SerializeField]
    private Image _digit2;

    [SerializeField]
    private Image _digit3;

    [Header("Coins Sprite Ref ")]
    [SerializeField]
    private Image _coin1;

    [SerializeField]
    private Image _coin2;

    [Header("Score Sprite Ref ")]
    [SerializeField]
    private Image _score1;

    [SerializeField]
    private Image _score2;

    [SerializeField]
    private Image _score3;

    [SerializeField]
    private Image _score4;

    [SerializeField]
    private Image _score5;

    [SerializeField]
    private Image _score6;

    [Header("Lives Sprite Ref ")]
    [SerializeField]
    private Image _lives1;

    [Header("Top Score Sprite Ref ")]
    [SerializeField]
    private SpriteRenderer _topScore1;

    [SerializeField]
    private SpriteRenderer _topScore2;

    [SerializeField]
    private SpriteRenderer _topScore3;

    [SerializeField]
    private SpriteRenderer _topScore4;

    [SerializeField]
    private SpriteRenderer _topScore5;

    [SerializeField]
    private SpriteRenderer _topScore6;

    private int _time;
    private int _coins;
    private int _score;
    private int _lives;

    private PlayerStats playerStats;

    private void Start()
    {
        // hook up stuff
        playerStats = GameManager.Instance.player.GetComponent<PlayerStats>();
        _time = 200; // random test
        _coins = 0;
        _score = 6969; // random test
        _lives = 0;

        ScoreManager.ScoreChanged.AddListener(UpdateScore);
        UpdateTopScore();
    }

    void OnDisable()
    {
        ScoreManager.ScoreChanged.RemoveListener(UpdateScore);
    }

    void Update()
    {
        if (StateManager.CurrentGameState() == StateManager.GameState.NULL)
        {
            UpdateCoins();
            return;
        }
        UpdateCoins();
        UpdateTime();
        UpdateScore();
        UpdateLives();
        UpdateTopScore();
    }

    void UpdateCoins()
    {
        _coins = playerStats.coins;
        _coins = Mathf.Clamp(_coins, 0, 99);
        _coin1.sprite = _numbers[_coins / 10];
        _coin2.sprite = _numbers[_coins % 10];
    }

    void UpdateTime()
    {
        _time = GameManager.Timer;
        if (_time < 0)
            return;
        int digit1 = _time / 100;
        int digit2 = (_time / 10) % 10;
        int digit3 = _time % 10;

        _digit1.sprite = _numbers[digit1];
        _digit2.sprite = _numbers[digit2];
        _digit3.sprite = _numbers[digit3];
    }

    public void UpdateScore()
    {
        _score = ScoreManager.GetScore();
        _score1.sprite = _numbers[(_score / 100000) % 10];
        _score2.sprite = _numbers[(_score / 10000) % 10];
        _score3.sprite = _numbers[(_score / 1000) % 10];
        _score4.sprite = _numbers[(_score / 100) % 10];
        _score5.sprite = _numbers[(_score / 10) % 10];
        _score6.sprite = _numbers[_score % 10];
    }

    void UpdateLives()
    {
        _lives = playerStats.lives;
        _lives = Mathf.Clamp(_lives, 0, _numbers.Length - 1);
        _lives1.sprite = _numbers[_lives];
    }

    void UpdateTopScore()
    {
        int highscore = PlayerPrefs.GetInt("HighScore", 0);
        _topScore1.sprite = _numbers[(highscore / 100000) % 10];
        _topScore2.sprite = _numbers[(highscore / 10000) % 10];
        _topScore3.sprite = _numbers[(highscore / 1000) % 10];
        _topScore4.sprite = _numbers[(highscore / 100) % 10];
        _topScore5.sprite = _numbers[(highscore / 10) % 10];
        _topScore6.sprite = _numbers[highscore % 10];
    }
}
