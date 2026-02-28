using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FlagpoleInteraction : MonoBehaviour
{
    [SerializeField]
    private Transform ClimbPoint;

    [SerializeField]
    private Transform CastlePoint;

    [SerializeField]
    private Transform GroundPoint;

    [SerializeField]
    Collider2D player;

    public float flagX;

    private bool flagDropped;
    private bool timeScoreTotaled;

    [SerializeField]
    private Transform flag;

    [SerializeField]
    private Transform castleFlag;

    [SerializeField]
    private Vector3 flagBasePos = new Vector3();

    [SerializeField]
    private Vector3 cFlagEndPos = new Vector3();

    [SerializeField]
    private AudioManager am;

    [SerializeField]
    private List<Animator> fireworks = new List<Animator>();

    [SerializeField]
    private List<GameObject> pointDividers = new List<GameObject>();

    [SerializeField]
    private UIManager uiMan;

    private int fireworksCount = 0;
    private Rigidbody2D playerRb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StateManager.EnteredWon ??= new UnityEvent();
        StateManager.EnteredWon.AddListener(MoveFlagToBase);
    }

    // Update is called once per frame
    void Update() { }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // sorry plugging this in as this is the end of the play cycle
        AudioManager.Instance.ResetBGMSpeed();
        if (collision == player)
        {
            GameManager.Instance.hasHitFlag = true;
            Debug.Log("Hit Pole");
            am.StopBGM();
            am.Play("flagslide");
            StateManager.SetWinState();
            int timerLastDigit = GameManager.Timer % 10;
            if (new[] { 1, 2, 6 }.Contains(timerLastDigit))
            {
                fireworksCount = timerLastDigit;
            }

            int pointsAwarded = 0;
            Transform t = collision.gameObject.GetComponent<Transform>();
            if (t.position.y <= pointDividers[0].transform.position.y)
            {
                pointsAwarded = 100;
            }
            else if (
                t.position.y > pointDividers[0].transform.position.y
                && t.position.y <= pointDividers[1].transform.position.y
            )
            {
                pointsAwarded = 400;
            }
            else if (
                t.position.y > pointDividers[1].transform.position.y
                && t.position.y <= pointDividers[2].transform.position.y
            )
            {
                pointsAwarded = 800;
            }
            else if (
                t.position.y > pointDividers[2].transform.position.y
                && t.position.y <= pointDividers[3].transform.position.y
            )
            {
                pointsAwarded = 2000;
            }
            else if (t.position.y > pointDividers[3].transform.position.y)
            {
                pointsAwarded = 5000;
            }

            Debug.Log($"Points Awarded: {pointsAwarded}");
            ScoreManager.ModifyScore(pointsAwarded);
            uiMan.SpawnPopup(pointsAwarded, t.position);

            if (collision.gameObject.GetComponent<Rigidbody2D>() != null)
            {
                playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                playerRb.bodyType = RigidbodyType2D.Static;
            }
            t.position = new Vector3(flagX, t.position.y, 0);
            Animator marioA = new Animator();
            if (collision.GetComponent<Animator>() != null)
            {
                marioA = collision.GetComponent<Animator>();
            }
            // marioA.SetLayerWeight(0, 1);
            marioA.SetBool("isJumping", false);
            marioA.SetBool("isMoving", false);
            // marioA.SetLayerWeight(0, 1);
            marioA.speed = 1f;
            marioA.SetBool("isPoleSliding", true);
            StartCoroutine(MoveToCastle(t, marioA));
        }
    }

    IEnumerator MoveToCastle(Transform t, Animator a)
    {
        Debug.Log("Moving Mario");
        float newY = 0;
        float newX = 0;

        while (Mathf.Abs(t.position.y - ClimbPoint.position.y) > 0.01f)
        {
            newY = Mathf.MoveTowards(t.position.y, ClimbPoint.position.y, 6.2f * Time.deltaTime);
            t.position = new Vector3(t.position.x, newY, 0);
            yield return null;
        }
        a.SetBool("isPoleSliding", false);
        a.speed = 0;
        yield return new WaitUntil(() => flagDropped);
        a.gameObject.transform.Rotate(new Vector3(0, 180, 0));
        a.gameObject.transform.position += new Vector3(0.38f, 0, 0);
        yield return new WaitForSeconds(0.5f);
        am.Play("stageclear");
        a.ResetControllerState();
        a.speed = 1;
        a.SetBool("isJumping", true);
        a.gameObject.transform.Rotate(new Vector3(0, -180, 0));

        while (
            Mathf.Abs(t.position.y - GroundPoint.transform.position.y) > 0.01f
            || Mathf.Abs(t.position.x - GroundPoint.transform.position.x) > 0.01f
        )
        {
            newY = Mathf.MoveTowards(t.position.y, GroundPoint.position.y, 3f * Time.deltaTime);
            newX = Mathf.MoveTowards(t.position.x, GroundPoint.position.x, 2f * Time.deltaTime);
            t.position = new Vector3(newX, newY, 0);
            yield return null;
        }

        a.SetBool("isJumping", false);
        a.SetFloat("MoveSpeed", 2);
        a.SetBool("isMoving", true);
        while (Mathf.Abs(t.position.x - CastlePoint.transform.position.x) > 0.01f)
        {
            newX = Mathf.MoveTowards(t.position.x, CastlePoint.position.x, 2f * Time.deltaTime);
            t.position = new Vector3(newX, t.position.y, 0);
            yield return null;
        }
        //Calculate Time Score
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(TabulateTimeScore());
        yield return new WaitUntil(() => timeScoreTotaled);
        MoveFlagOutOfCastle();
    }

    IEnumerator TabulateTimeScore()
    {
        am.StopBGM();
        am.PlayBGMCoinRing();
        while (GameManager.Timer >= 0)
        {
            GameManager.Timer--;
            yield return new WaitForSeconds(0.01f);
            ScoreManager.ModifyScore(50);
            yield return null;
        }
        timeScoreTotaled = true;
        am.StopBGM();
    }

    private void MoveFlagToBase()
    {
        StartCoroutine(MoveFlagToBaseEn());
    }

    IEnumerator MoveFlagToBaseEn()
    {
        Debug.Log("MovingFlag");
        float newY = 0;
        while (Mathf.Abs(flag.position.y - flagBasePos.y) > 0.005f)
        {
            newY = Mathf.MoveTowards(flag.position.y, flagBasePos.y, 6f * Time.deltaTime);
            flag.position = new Vector3(flag.position.x, newY, 0);
            yield return null;
        }
        flagDropped = true;
    }

    private void MoveFlagOutOfCastle()
    {
        StartCoroutine(MoveFlagOutOfCastleEn());
    }

    IEnumerator MoveFlagOutOfCastleEn()
    {
        float newY = 0;
        while (Mathf.Abs(castleFlag.position.y - cFlagEndPos.y) > 0.01f)
        {
            newY = Mathf.MoveTowards(castleFlag.position.y, cFlagEndPos.y, 2f * Time.deltaTime);
            castleFlag.position = new Vector3(castleFlag.position.x, newY, 0);
            yield return null;
        }
        StartCoroutine(PlayFireworks(fireworksCount));
    }

    IEnumerator PlayFireworks(int worksToPlay)
    {
        for (int i = 0; i < worksToPlay; i++)
        {
            // needs this guard since only 3 animators
            int index = i % fireworks.Count;

            fireworks[index].SetTrigger("Explode");

            yield return new WaitForSeconds(
                fireworks[index].GetCurrentAnimatorStateInfo(0).length / 2
            );

            ScoreManager.ModifyScore(500);
            am.Play("firework");

            yield return new WaitForSeconds(
                fireworks[index].GetCurrentAnimatorStateInfo(0).length / 2
            );

            fireworks[index].ResetControllerState();
        }

        GameManager.Instance.hasWon = true;

        yield return new WaitForSeconds(3f);
        playerRb.bodyType = RigidbodyType2D.Dynamic;
        StartCoroutine(GameManager.Instance.RestartGame());
    }
}
