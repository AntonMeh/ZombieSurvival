using UnityEngine;

/// <summary>
/// AI зомбі: переслідує гравця у 2D просторі (MoveX, MoveY).
/// </summary>
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

        // Рух
        rb.linearVelocity = direction * speed;

        // Зміна спрайтів у 2вимірному просторі (як у кажана, але і для Y)
        // Використовуємо -1, 0, або 1 для чіткого перемикання анімацій (Blend Tree)
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
        // Шкода наноситься через Animation Event → DealDamage()
    }

    /// <summary>
    /// Викликається з Animation Event на кадрі удару в анімації атаки.
    /// </summary>
    public void DealDamage()
    {
        if (target == null) return;

        PlayerHealth ph = target.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(attackDamage);
    }
}
