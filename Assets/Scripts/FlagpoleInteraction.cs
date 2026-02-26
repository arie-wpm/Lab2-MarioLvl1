using System.Collections;
using UnityEngine;

public class FlagpoleInteraction : MonoBehaviour
{
    [SerializeField]
    private Transform ClimbPoint;

    [SerializeField]
    private Transform CastlePoint;

    [SerializeField]
    private Transform GroundPoint;

    [SerializeField]
    Animation poleSlide;

    public float flagX;

    private bool flagDropped;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StateManager.EnteredWon.AddListener(MoveFlagToBase);
    }

    // Update is called once per frame
    void Update() { }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("Hit Pole");
            am.StopBGM();
            am.Play("flagslide");

            StateManager.SetWinState();
            if (collision.gameObject.GetComponent<Rigidbody2D>() != null)
            {
                collision.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            }
            Transform t = collision.gameObject.GetComponent<Transform>();
            t.position = new Vector3(flagX, t.position.y, 0);
            Animator marioA = collision.GetComponent<Animator>();
            marioA.SetLayerWeight(0, 1);
            marioA.SetBool("isJumping", false);
            marioA.SetBool("isMoving", false);
            marioA.SetLayerWeight(0, 1);
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
        MoveFlagOutOfCastle();
    }

    private void MoveFlagToBase()
    {
        StartCoroutine(MoveFlagToBaseEn());
    }

    IEnumerator MoveFlagToBaseEn()
    {
        Debug.Log("MovingFlag");
        float newY = 0;
        while (Mathf.Abs(flag.position.y - flagBasePos.y) > 0.01f)
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
    }
}
