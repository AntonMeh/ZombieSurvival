using UnityEngine;

/// <summary>
/// AI кажана: летить до гравця, зупиняється на відстані атаки, атакує на місці.
/// HP/Hurt/Die обробляє EnemyHealth на тому ж GameObject.
/// 
/// Animator параметри:
///   float "MoveX"    — напрямок (-1 / 1)
///   float "Speed"    — швидкість (0 = idle, >0 = рух)
///   trigger "Attack" — анімація атаки
/// </summary>
public class BatAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;

    [Header("Combat")]
    public int attackDamage = 15;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform target;
    private float nextAttackTime;

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
            StopMovement();
            return;
        }

        if (target == null)
        {
            if (PlayerController.Instance != null)
                target = PlayerController.Instance.transform;
            return;
        }

        UpdateFacing();

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > attackRange)
        {
            ChaseTarget();
        }
        else
        {
            HoverAndAttack();
        }
    }

    /// <summary>
    /// Оновлює напрямок спрайта (ліво/право) залежно від позиції гравця.
    /// </summary>
    void UpdateFacing()
    {
        float dirX = target.position.x - transform.position.x;
        anim.SetFloat("MoveX", dirX > 0 ? 1f : -1f);
    }

    /// <summary>
    /// Летить до гравця, поки не досягне attackRange.
    /// </summary>
    void ChaseTarget()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    /// <summary>
    /// Зависає на місці біля гравця та атакує з кулдауном.
    /// </summary>
    void HoverAndAttack()
    {
        StopMovement();

        if (Time.time >= nextAttackTime)
        {
            PerformAttack();
        }
    }

    /// <summary>
    /// Виконує атаку: тригерить анімацію та наносить шкоду гравцю.
    /// </summary>
    void PerformAttack()
    {
        nextAttackTime = Time.time + attackCooldown;
        anim.SetTrigger("Attack");
        // Шкода наноситься через Animation Event → DealDamage()
    }

    /// <summary>
    /// Викликається з Animation Event на кадрі удару в анімації атаки.
    /// </summary>
    public void DealDamage()
    {
        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= attackRange * 1.5f)
        {
            PlayerHealth ph = target.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// Повністю зупиняє рух кажана.
    /// </summary>
    void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }
}