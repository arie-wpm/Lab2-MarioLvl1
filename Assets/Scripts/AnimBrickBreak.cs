using UnityEngine;

public class AnimBrickBreak : MonoBehaviour
{
    [SerializeField] private Transform brick1;
    [SerializeField] private Transform brick2;
    [SerializeField] private Transform brick3;
    [SerializeField] private Transform brick4;

    private Vector2 v1, v2, v3, v4;

    [SerializeField] private float gravity = -20f;


    void Start() => BreakBrick();

    void Update()
    {
        float dt = Time.deltaTime;

        v1.y += gravity * dt;
        v2.y += gravity * dt;
        v3.y += gravity * dt;
        v4.y += gravity * dt;

        brick1.position += (Vector3)(v1 * dt);
        brick2.position += (Vector3)(v2 * dt);
        brick3.position += (Vector3)(v3 * dt);
        brick4.position += (Vector3)(v4 * dt);
    }

    void BreakBrick() {
        v1 = new Vector2(-2f, 12f);
        v2 = new Vector2(2f, 12f);
        v3 = new Vector2(-2f, 9f);
        v4 = new Vector2(2f, 9f);
        // Destroy(gameObject, 1.2f);
    }
}