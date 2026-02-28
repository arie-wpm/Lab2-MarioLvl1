using System.Collections;
using UnityEngine;

public class BlockItem : MonoBehaviour
{
    private Rigidbody2D rb;
    private CircleCollider2D physicsCollider;
    private BoxCollider2D triggerCollider;
    private SpriteRenderer spriteRenderer;
    private EntityMovement movement;
    private Pickup pickup;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        physicsCollider = GetComponent<CircleCollider2D>();
        triggerCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<EntityMovement>();
        pickup = GetComponent<Pickup>();
    }

    private void Start()
    {
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        if (rb == null || physicsCollider == null || triggerCollider == null || spriteRenderer == null) yield break;
        // Audio addition
        if (pickup.type == PickupType.SuperMushroom || pickup.type == PickupType.FireFlower ||
            pickup.type == PickupType.OneUp || pickup.type == PickupType.Star) {
            AudioManager.Instance.Play("item");
        } 
        else if (pickup.type == PickupType.Coin) AudioManager.Instance.Play("coin");
        
        Transform block = transform.parent;
        Collider2D blockCol = block != null ? block.GetComponent<Collider2D>() : null;

        var blockSR = block != null ? block.GetComponent<SpriteRenderer>() : null;
        if (blockSR != null)
        {
            spriteRenderer.sortingLayerID = blockSR.sortingLayerID;
            spriteRenderer.sortingOrder = blockSR.sortingOrder - 1;
        }

        transform.SetParent(null, true);

        if (movement != null) movement.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        physicsCollider.enabled = false;
        triggerCollider.enabled = false;
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(0.25f);

        spriteRenderer.enabled = true;

        if (blockCol == null)
            yield break;

        if (pickup.type == PickupType.Coin)
        {
            // transform.localScale *= 1.1f;
            if (movement != null) movement.enabled = false;

            rb.simulated = false;
            physicsCollider.enabled = false;
            triggerCollider.enabled = false;

            Vector3 coinStartPos = transform.position;
            float coinElapsed = 0f;
            Vector3 peakPos = coinStartPos + Vector3.up * 3f;
            float upDuration = 0.25f;
            float downDuration = 0.25f;

            // Move up
            while (coinElapsed < upDuration)
            {
                float t = coinElapsed / upDuration;
                transform.position = Vector3.Lerp(coinStartPos, peakPos, t);
                coinElapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = peakPos;

            coinElapsed = 0f;

            // Move down
            while (coinElapsed < downDuration)
            {
                float t = coinElapsed / downDuration;
                transform.position = Vector3.Lerp(peakPos, coinStartPos, t);
                coinElapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = coinStartPos;

            pickup.ApplyPickup();
            Destroy(gameObject);
        }
        else
        {
            
            Bounds bb = blockCol.bounds;

            Vector2 startCenter = bb.center;
            Vector2 endCenter = new Vector2(bb.center.x, bb.max.y + physicsCollider.bounds.extents.y + 0.02f);

            Vector2 deltaToStart = startCenter - (Vector2)physicsCollider.bounds.center;
            transform.position += (Vector3)deltaToStart;

            Vector2 startPos = transform.position;

            Vector2 deltaToEnd = endCenter - (Vector2)physicsCollider.bounds.center;
            Vector2 endPos = (Vector2)transform.position + deltaToEnd;

            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.position = Vector2.Lerp(startPos, endPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;

            rb.simulated = true;
            rb.position = endPos;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            physicsCollider.enabled = true;
            triggerCollider.enabled = true;

            Physics2D.IgnoreCollision(physicsCollider, blockCol, true);

            yield return new WaitForFixedUpdate();

            rb.position = endPos;
            rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(0.1f);

            Physics2D.IgnoreCollision(physicsCollider, blockCol, false);

            if (movement != null)
            {
                movement.direction = Vector2.right;
                movement.enabled = true;
            }

        }
    }
}