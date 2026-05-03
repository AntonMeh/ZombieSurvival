using UnityEngine;

public class GolemAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 1.5f;

    [Header("Attack Settings")]
    public int attackDamage = 25;
    public float attackRate = 2f;
    private float nextAttackTime;

    private Transform target;
    private Rigidbody2D rb;
    private Animator anim;

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

        if (anim != null)
        {
            anim.SetFloat("MoveX", moveX);
        }
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

        rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
        else
        {

            DealDamage();
        }
    }

    public void DealDamage()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= 2f) 
        {
            PlayerHealth ph = target.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(attackDamage);
            }
        }
    }
}
