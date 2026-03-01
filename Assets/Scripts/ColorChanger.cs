using System.Collections;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    private Color _default1 = new Color32(181, 49, 32, 255);
    private Color _default2 = new Color32(234, 158, 34, 255);
    private Color _default3 = new Color32(107, 109, 0, 255);

    private Color _luigi1 = new Color32(255, 254, 255, 255);
    private Color _luigi2 = new Color32(234, 158, 34, 255);
    private Color _luigi3 = new Color32(56, 135, 0, 255);

    private Color _flower1 = new Color32(247, 216, 165, 255);
    private Color _flower2 = new Color32(234, 158, 34, 255);
    private Color _flower3 = new Color32(181, 49, 32, 255);

    private Color _star1_1 = new Color32(12, 147, 0, 255);
    private Color _star2_1 = new Color32(255, 254, 255, 255);
    private Color _star3_1 = new Color32(234, 158, 34, 255);

    private Color _star1_2 = new Color32(181, 216, 165, 255);
    private Color _star2_2 = new Color32(255, 254, 255, 255);
    private Color _star3_2 = new Color32(234, 158, 34, 255);

    private Color _star1_3 = new Color32(0, 0, 0, 255);
    private Color _star2_3 = new Color32(254, 204, 197, 255);
    private Color _star3_3 = new Color32(153, 78, 0, 255);

    private Color _transparent = new Color32(0, 0, 0, 0);

    private Color[] _currentColors;
    private float _flashInterval = 0.1f;

    public float currentTimeScale = 1f;

    public void ChangeToDefault(SpriteRenderer[] sprites)
    {
        Color[] colors;

        if (GameManager.Instance._character == "mario")
            colors = new Color[] { _default1, _default2, _default3 };
        else
            colors = new Color[] { _luigi1, _luigi2, _luigi3 };

        for (int i = 0; i < sprites.Length; i++)
            sprites[i].color = colors[i];

        _currentColors = colors;
    }

    public void ChangeToFlower(SpriteRenderer[] sprites)
    {
        Color[] colors = { _flower1, _flower2, _flower3 };
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].color = colors[i];
        }
        _currentColors = colors;
    }

    public void ChangeToStar(SpriteRenderer[] sprites, float duration)
    {
        StartCoroutine(StarAnim(sprites, duration));
    }

    public void ChangeToIFrame(SpriteRenderer[] sprites, float duration)
    {
        StartCoroutine(IFrameAnim(sprites, duration));
    }

    public void StartTransformFreeze()
    {
        StartCoroutine(TransformFreeze());
    }

    public void StartTransformOnHit()
    {
        StartCoroutine(TransformOnHit());
    }

    public IEnumerator StarAnim(SpriteRenderer[] sprites, float totalDuration)
    {
        Color[][] flashSets = new Color[][]
        {
            new Color[] { _star1_1, _star2_1, _star3_1 },
            new Color[] { _star1_2, _star2_2, _star3_2 },
            new Color[] { _star1_3, _star2_3, _star3_3 },
        };

        float elapsed = 0f;
        int setIndex = 0;
        int setCount = flashSets.Length;

        while (elapsed < totalDuration)
        {
            Color[] currentSet = flashSets[setIndex];

            for (int i = 0; i < sprites.Length && i < currentSet.Length; i++)
            {
                sprites[i].color = currentSet[i];
            }
            setIndex = (setIndex + 1) % setCount;
            yield return new WaitForSecondsRealtime(_flashInterval);
            elapsed += _flashInterval;
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].color = _currentColors[i];
        }
    }

    public IEnumerator IFrameAnim(SpriteRenderer[] sprites, float totalDuration)
    {
        Color[][] flashSets = new Color[][]
        {
            new Color[] { _transparent, _transparent, _transparent },
            _currentColors,
        };
        Collider2D playerCollider = GameManager.Instance.player.GetComponent<Collider2D>();
        //Get layer Masks for enemy and Nothing.
        LayerMask enemy = LayerMask.GetMask("Enemy");
        LayerMask nothing = LayerMask.GetMask("Nothing");
        //Set exclude enemies.
        playerCollider.excludeLayers = enemy;
        float elapsed = 0f;
        int setIndex = 0;
        int setCount = flashSets.Length;

        while (elapsed < totalDuration)
        {
            Color[] currentSet = flashSets[setIndex];

            for (int i = 0; i < sprites.Length && i < currentSet.Length; i++)
            {
                sprites[i].color = currentSet[i];
            }
            setIndex = (setIndex + 1) % setCount;
            yield return new WaitForSecondsRealtime(_flashInterval);
            elapsed += _flashInterval;
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].color = _currentColors[i];
        }

        PlayerController player = GameManager.Instance.player.GetComponent<PlayerController>();
        //Set exclude nothing.
        playerCollider.excludeLayers = nothing;
        player.canDie = true;
    }

    public IEnumerator TransformFreeze()
    {
        Time.timeScale = 0f;
        currentTimeScale = Time.timeScale;

        Animator marioAnimator = GameManager.Instance.player.GetComponent<Animator>();
        PlayerStats marioStats = GameManager.Instance.player.GetComponent<PlayerStats>();
        SpriteRenderer[] sprites =
            GameManager.Instance.player.GetComponentsInChildren<SpriteRenderer>();

        if (marioStats.powerState == MarioPowerState.Small)
            marioAnimator.SetBool("isTransforming", true);
        else
        {
            marioAnimator.speed = 0f;
            ChangeToStar(sprites, 1f);
        }

        yield return new WaitForSecondsRealtime(1f);

        marioAnimator.speed = 1f;
        marioAnimator.SetBool("isTransforming", false);
        marioAnimator.SetLayerWeight(1, 1f);
        Time.timeScale = 1f;
        currentTimeScale = 1f;
    }

    public IEnumerator TransformOnHit()
    {
        Time.timeScale = 0f;
        currentTimeScale = Time.timeScale;

        Animator marioAnimator = GameManager.Instance.player.GetComponent<Animator>();
        PlayerStats marioStats = GameManager.Instance.player.GetComponent<PlayerStats>();
        SpriteRenderer[] sprites =
            GameManager.Instance.player.GetComponentsInChildren<SpriteRenderer>();

        if (marioStats.powerState != MarioPowerState.Small)
            marioAnimator.SetBool("isTransforming", true);

        yield return new WaitForSecondsRealtime(1f);

        marioAnimator.speed = 1f;
        marioAnimator.SetBool("isTransforming", false);
        marioAnimator.SetLayerWeight(1, 0f);
        Time.timeScale = 1f;
        currentTimeScale = 1f;
        marioStats.SetPowerState(MarioPowerState.Small);
        ChangeToIFrame(sprites, 2f);
    }
}
