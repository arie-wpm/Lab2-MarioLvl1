using UnityEngine;

public class InvisBlockHack : MonoBehaviour
{
    private PlatformEffector2D platformEffector2D;
    private BoxCollider2D boxCollider2D;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        platformEffector2D = GetComponent<PlatformEffector2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!spriteRenderer.enabled)
        {
            platformEffector2D.enabled = true;
            boxCollider2D.usedByEffector = true;
        }
        else
        {
            platformEffector2D.enabled = false;
            boxCollider2D.usedByEffector = false;
        }
    }
}
