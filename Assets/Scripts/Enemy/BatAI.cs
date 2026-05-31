using UnityEngine;

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

        target = PlayerController.GetNearestPlayer(transform.position);
    }

    void OnEnable()
    {
        target = PlayerController.GetNearestPlayer(transform.position);
    }

    void Update()
    {
        bool isMultiplayer = Unity.Netcode.NetworkManager.Singleton != null && (Unity.Netcode.NetworkManager.Singleton.IsServer || Unity.Netcode.NetworkManager.Singleton.IsClient);
        if (isMultiplayer && !Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            StopMovement();
            return;
        }

        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
        {
            StopMovement();
            return;
        }

        target = PlayerController.GetNearestPlayer(transform.position);

        if (target == null)
        {
            StopMovement();
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

    void UpdateFacing()
    {
        float dirX = target.position.x - transform.position.x;
        anim.SetFloat("MoveX", dirX > 0 ? 1f : -1f);
    }

    void ChaseTarget()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void HoverAndAttack()
    {
        StopMovement();

        if (Time.time >= nextAttackTime)
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        nextAttackTime = Time.time + attackCooldown;
        anim.SetTrigger("Attack");

    }

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

    void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }
}