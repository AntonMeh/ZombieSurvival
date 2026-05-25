using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackRate = 1.5f;
    private float nextAttackTime;

    public float speed = 2f;
    private Transform target;
    private Rigidbody2D rb;
    public Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (PlayerController.Instance != null)
            target = PlayerController.Instance.transform;
    }

    void OnEnable()
    {
        if (PlayerController.Instance != null)
            target = PlayerController.Instance.transform;
    }

    void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (target == null)
        {
            if (PlayerController.Instance != null)
                target = PlayerController.Instance.transform;
            return;
        }

        UpdateMovementAndFacing();
    }

    void UpdateMovementAndFacing()
    {
        Vector2 direction = (target.position - transform.position).normalized;

        rb.linearVelocity = direction * speed;

        float moveX = Mathf.Abs(direction.x) > 0.1f ? (direction.x > 0 ? 1f : -1f) : 0f;

        anim.SetFloat("MoveX", moveX);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= nextAttackTime)
        {
            PerformAttack();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= nextAttackTime)
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        nextAttackTime = Time.time + attackRate;
        anim.SetTrigger("Attack");

    }

    public void DealDamage()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= 1.8f) // Радіус атаки зомбі
        {
            PlayerHealth ph = target.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(attackDamage);
        }
    }
}
