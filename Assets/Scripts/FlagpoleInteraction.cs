using System.Collections;
using UnityEngine;

public class FlagpoleInteraction : MonoBehaviour
{
    public GameObject ClimbPoint;
    public GameObject CastlePoint;

    [SerializeField]
    Animation poleSlide;

    public float flagX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("Hit Pole");
            StateManager.SetWinState();
            collision.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            Transform t = collision.gameObject.GetComponent<Transform>();
            t.position = new Vector3(flagX, t.position.y, 0);
            Animator marioA = collision.GetComponent<Animator>();
            marioA.SetLayerWeight(0, 1);
            marioA.SetBool("isJumping", false);
            marioA.SetBool("isMoving", false);
            marioA.SetLayerWeight(0, 1);
            marioA.SetBool("isPoleSliding", true);
            StartCoroutine(MoveToCastle(t));
        }
    }

    IEnumerator MoveToCastle(Transform t)
    {
        Debug.Log("Moving Mario");
        float newY = 0;

        while (Mathf.Abs(t.position.y - ClimbPoint.transform.position.y) > 0.01f)
        {
            newY = Mathf.MoveTowards(
                t.position.y,
                ClimbPoint.transform.position.y,
                2f * Time.deltaTime
            );
            t.position = new Vector3(t.position.x, newY, 0);
            yield return null;
        }
    }
}
