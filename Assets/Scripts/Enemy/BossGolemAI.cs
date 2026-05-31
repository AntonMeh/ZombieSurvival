using UnityEngine;

public class BossGolemAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 1f;

    [Header("Attack Settings")]
    public int attackDamage = 40;
    public float attackRate = 2.5f;
    private float nextAttackTime;

    [Header("Boss Settings")]
    [Tooltip("Назва боса (відображається над полоскою HP)")]
    public string bossName = "Ancient Golem";

    private Transform target;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        target = PlayerController.GetNearestPlayer(transform.position);
    }

    void OnEnable()
    {
        target = PlayerController.GetNearestPlayer(transform.position);

        if (BossHealthBar.Instance != null)
        {
            EnemyHealth eh = GetComponent<EnemyHealth>();
            if (eh != null)
                BossHealthBar.Instance.ShowBoss(bossName, eh.health);
        }
    }

    void OnDisable()
    {

        if (BossHealthBar.Instance != null)
            BossHealthBar.Instance.HideBoss();
    }

    void Update()
    {
        bool isMultiplayer = Unity.Netcode.NetworkManager.Singleton != null && (Unity.Netcode.NetworkManager.Singleton.IsServer || Unity.Netcode.NetworkManager.Singleton.IsClient);
        if (isMultiplayer && !Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        target = PlayerController.GetNearestPlayer(transform.position);

        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        UpdateMovementAndFacing();
    }

    void UpdateMovementAndFacing()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;

        float moveX = Mathf.Abs(direction.x) > 0.1f ? (direction.x > 0 ? 1f : -1f) : 0f;

        if (anim != null)
            anim.SetFloat("MoveX", moveX);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= nextAttackTime)
            PerformAttack();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= nextAttackTime)
            PerformAttack();
    }

    void PerformAttack()
    {
        nextAttackTime = Time.time + attackRate;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Attack");
        else
            DealDamage();
    }

    public void DealDamage()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= 2.5f)
        {
            PlayerHealth ph = target.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(attackDamage);
        }
    }
}
