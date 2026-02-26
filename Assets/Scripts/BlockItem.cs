using System.Collections;
using UnityEngine;

public class BlockItem : MonoBehaviour
{
    private Rigidbody2D rb;
    private CircleCollider2D physicsCollider;
    private BoxCollider2D triggerCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        physicsCollider = GetComponent<CircleCollider2D>();
        triggerCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (rb == null || physicsCollider == null || triggerCollider == null || spriteRenderer == null) return;
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        Transform block = transform.parent;
        Collider2D blockCol = block != null ? block.GetComponent<Collider2D>() : null;

        var blockSR = block != null ? block.GetComponent<SpriteRenderer>() : null;
        if (blockSR != null)
        {
            spriteRenderer.sortingLayerID = blockSR.sortingLayerID;
            spriteRenderer.sortingOrder = blockSR.sortingOrder - 1;
        }

        transform.SetParent(null, true);

        var movement = GetComponent<EntityMovement>();
        if (movement != null) movement.enabled = false;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        physicsCollider.enabled = false;
        triggerCollider.enabled = false;
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(0.3f);

        Bounds blockB = blockCol != null ? blockCol.bounds : new Bounds(rb.position, Vector3.one);

        float xOffset = -0.5f;

        Vector2 offsetWorld = Vector2.Scale(physicsCollider.offset, transform.lossyScale);
        float itemHalfH = physicsCollider.radius * transform.lossyScale.y;

        Vector2 startCenter = new Vector2(blockB.center.x + xOffset, blockB.center.y);
        Vector2 endCenter = new Vector2(blockB.center.x + xOffset, blockB.max.y + itemHalfH + 0.01f);

        Vector2 startPos = startCenter - offsetWorld;
        Vector2 endPos = endCenter - offsetWorld;

        rb.position = startPos;

        spriteRenderer.enabled = true;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            rb.position = Vector2.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.position = endPos;
        rb.linearVelocity = Vector2.zero;

        rb.bodyType = RigidbodyType2D.Dynamic;

        physicsCollider.enabled = true;
        triggerCollider.enabled = true;

        yield return new WaitForFixedUpdate();

        rb.linearVelocity = Vector2.zero;

        if (movement != null) movement.enabled = true;
    }
}