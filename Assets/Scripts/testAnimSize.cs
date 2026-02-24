using UnityEngine;

public class testAnimSize : MonoBehaviour
{
    public Animator animator;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            float weight = animator.GetLayerWeight(1);
            if (weight == 1f) animator.SetLayerWeight(1, 0f);
            else animator.SetLayerWeight(1, 1f);
            AudioManager.instance.Play("bump");
        }
    }

    void Start()
    {
        AudioManager.instance.PlayBGM();
    } 
}
