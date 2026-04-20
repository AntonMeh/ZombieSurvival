using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    public Animator animator;
    private Rigidbody2D rb;
    private EnemyAI enemyAI; 

    [Header("Loot Settings")]
    public GameObject coinPrefab;
    [Range(0, 100)] public float dropChance = 50f;

    [Header("Score Settings")]
    public int pointsValue = 100;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        animator.SetTrigger("Hurt");
        Vector2 knockback = (transform.position - GameObject.FindWithTag("Player").transform.position).normalized;
        rb.AddForce(knockback * 2f, ForceMode2D.Impulse);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("Die");

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(pointsValue);

        TryDropLoot();

        if (enemyAI != null) enemyAI.enabled = false;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        Invoke("BackToPool", 1f);
    }
    void TryDropLoot()
    {
        float randomValue = Random.Range(0f, 100f);
        if (randomValue <= dropChance)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
        }
    }

    void BackToPool()
    {
        ZombiePool.Instance.ReturnZombie(gameObject);
    }

    void OnEnable()
    {
        health = 3; 
        if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = true;
        if (GetComponent<EnemyAI>()) GetComponent<EnemyAI>().enabled = true;
    }
}