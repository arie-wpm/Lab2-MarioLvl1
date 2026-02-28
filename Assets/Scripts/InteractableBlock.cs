using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableBlock : MonoBehaviour
{
    private enum BlockState
    {
        QBlock,
        QBlockInvis,
        BrickBreakable,
        BrickUnbreakable
    }

    [SerializeField] private BlockState blockState;
    [SerializeField] private float destroyDelay;
    [SerializeField] private int hp = 1;
    
    [SerializeField] private GameObject heldPickup;
    [SerializeField] private GameObject upgradedPickup;
    [SerializeField] private GameObject brickDebrisPrefab;
    [SerializeField] Sprite qBlockSprite;
    
    private SpriteRenderer rend;
    private BoxCollider2D col;
    private Animator anim;

    private BlockState _initialBlockState;
    private GameObject _initalHeldPickup;
    private GameObject _initalUpgradedPickup;
    private List<GameObject> _spawnedItems = new List<GameObject>();
    private Sprite _initialSprite;
    private int _initalHP;
    private bool _isDisabled = false;
    
    void Start()
    {
        rend = gameObject.GetComponent<SpriteRenderer>();
        col = gameObject.GetComponent<BoxCollider2D>();
        anim = gameObject.GetComponent<Animator>();
        if (blockState == BlockState.QBlockInvis)
        {
            rend.enabled = false;
        }

        // store init state
        _initialBlockState = blockState;
        _initalHeldPickup = heldPickup;
        _initalUpgradedPickup = upgradedPickup;
        _initalHP = hp;
        _initialSprite = rend.sprite;
        _isDisabled = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_isDisabled) return;
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = other.gameObject.GetComponent<PlayerStats>();
            MarioPowerState powerState = playerStats.powerState;

            Bounds playerBounds = other.collider.bounds;
            Bounds blockBounds = col.bounds;
            Bounds checkBounds = blockBounds;
            checkBounds.Expand(new Vector3(0.1f, 0f, 0f));

            float playerCenterX = playerBounds.center.x;
            bool hAligned = playerCenterX > checkBounds.min.x && playerCenterX < checkBounds.max.x;

            float playerTopY = playerBounds.max.y;
            float blockBotY = checkBounds.min.y;

            float tolerance = 0.05f;
            bool bAligned = Mathf.Abs(blockBotY - playerTopY) <= tolerance;

            if (hAligned && bAligned) HitBlock(powerState);
        }
    }

    private void HitBlock(MarioPowerState powerState)
    {
        Bump();
        hp -= 1;
        if (heldPickup) SpawnPickup();

        switch (blockState)
        {
            case BlockState.QBlockInvis:
                rend.enabled = true;
                SetInactive();
                break;
            case BlockState.QBlock:
                SetInactive();
                break;
            case BlockState.BrickBreakable:
                if (powerState == MarioPowerState.Small) StartCoroutine(MoveBlockAnimation());
                else BreakableBlock();
                break;
            case BlockState.BrickUnbreakable:
                if (hp <= 0) rend.sprite = qBlockSprite;
                SetInactive();
                break;
        }
    }

    private void BreakableBlock()
    {
        AudioManager.Instance.Play("brick");
        if (hp > 0) return;
        Instantiate(brickDebrisPrefab, transform.position, Quaternion.identity);
        // Destroy(gameObject, destroyDelay);
        StartCoroutine(SoftDestroy(destroyDelay));
    }

    private void SetInactive()
    {;
        StartCoroutine(MoveBlockAnimation());
        if (hp > 0) return;
        if (anim != null) anim.SetBool("isDepleted", true);
        _isDisabled = true;
    }

    private void Bump()
    {
        AudioManager.Instance.Play("bump");
        Vector2 point = new Vector2(col.bounds.center.x, col.bounds.max.y + 0.1f);
        Vector2 size = new Vector2(col.bounds.size.x * 0.9f, 0.25f);
        
        Collider2D[] hits = Physics2D.OverlapBoxAll(point, size, 0f);
        foreach(Collider2D hit in hits)
        {
            IBumpable bumpable = hit.GetComponent<IBumpable>();
            if (bumpable != null)
            {
                bumpable.OnBump();
            }
        }
    }

    private void SpawnPickup()
    {
        var blockCol = GetComponent<Collider2D>();
        float x = blockCol.bounds.center.x;
        float y = blockCol.bounds.center.y;
        // check Mario size
        PlayerStats playerstats = GameManager.Instance.player.GetComponent<PlayerStats>();
        if (playerstats.powerState != MarioPowerState.Small && upgradedPickup != null) heldPickup = upgradedPickup;
        if (heldPickup != null) {
            GameObject item = Instantiate(heldPickup, new Vector3(x, y, 0f), Quaternion.identity, transform);
            _spawnedItems.Add(item);
            if (hp > 0) return;
            heldPickup = null;
            upgradedPickup = null;
        }
    }
    IEnumerator MoveBlockAnimation()
    {
        Vector2 currentTransform = transform.position;
        Vector2 targetTransform = new Vector2(transform.position.x, transform.position.y + 0.5f);
        float timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            transform.position = Vector2.Lerp(currentTransform, targetTransform, timer / 0.1f);
            yield return null;
        }

        timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            transform.position = Vector2.Lerp(targetTransform, currentTransform, timer / 0.1f);
            yield return null;
        }
    }

    public void Reset()
    {
        foreach (GameObject item in _spawnedItems) Destroy(item);
        _isDisabled = false;
        blockState = _initialBlockState;
        heldPickup = _initalHeldPickup;
        upgradedPickup = _initalUpgradedPickup;
        hp = _initalHP;
        rend.sprite = _initialSprite;

        if (_initialBlockState == BlockState.QBlockInvis || _initialBlockState == BlockState.QBlock)
        {
            anim.ResetControllerState();
            anim.SetBool("isDepleted", false);
        }

        if (_initialBlockState == BlockState.BrickBreakable || _initialBlockState == BlockState.BrickUnbreakable)
        {
            AddInteractions();
        }

        if (blockState == BlockState.QBlockInvis)
        {
            rend.enabled = false;
        }
    }

    IEnumerator SoftDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveInteractions();
    }

    public void RemoveInteractions()
    {
        rend.enabled = false;
        col.enabled = false;
    }

    public void AddInteractions()
    {
        rend.enabled = true;
        col.enabled = true;
    }
}
