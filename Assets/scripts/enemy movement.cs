using UnityEngine;

public class enemymovement : MonoBehaviour
{
    [SerializeField] private float slowSpeed = 1f;
    [SerializeField] private float fastSpeed = 4f;
    [SerializeField] private float speedChangeInterval = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private SpriteRenderer enemySprite;

    private Vector3 leftPoint;
    private Vector3 rightPoint;
    private bool movingRight = true;
    private bool movingFast;
    private float speedTimer;

    private void Awake()
    {
        if (enemySprite == null)
        {
            enemySprite = GetComponentInChildren<SpriteRenderer>();
        }

        leftPoint = transform.position - Vector3.right * patrolDistance;
        rightPoint = transform.position + Vector3.right * patrolDistance;
    }

    private void Update()
    {
        speedTimer += Time.deltaTime;
        if (speedTimer >= speedChangeInterval)
        {
            speedTimer = 0f;
            movingFast = !movingFast;
        }

        Vector3 target = movingRight ? rightPoint : leftPoint;
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            (movingFast ? fastSpeed : slowSpeed) * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.001f)
        {
            movingRight = !movingRight;
        }

        if (enemySprite != null)
        {
            enemySprite.flipX = !movingRight;
        }
    }
}
