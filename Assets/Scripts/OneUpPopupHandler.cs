using UnityEngine;

public class OneUpPopupHandler : MonoBehaviour
{
    [SerializeField] private float movespeed;
    [SerializeField] private float despawnDelay;
    private float timer;
    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timer < despawnDelay)
        {
            gameObject.transform.Translate(Vector2.up * (movespeed * Time.fixedDeltaTime));
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
