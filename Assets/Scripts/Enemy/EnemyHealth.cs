using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    private int initialHealth; // зберігаємо значення з Inspector
    public Animator animator;
    private Rigidbody2D rb;
    private EnemyAI enemyAI;
    private BatAI batAI;

    [Header("Loot Settings")]
    public GameObject coinPrefab;
    [Range(0, 100)] public float dropChance = 50f;

    [Header("Score Settings")]
    public int pointsValue = 100;

    void Awake()
    {
        initialHealth = health; // запам'ятовуємо стартове HP з Inspector
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
        batAI = GetComponent<BatAI>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        animator.SetTrigger("Hurt");
        // Використовуємо Singleton замість FindWithTag — O(1) замість O(n)
        Vector2 knockback = (transform.position - PlayerController.Instance.transform.position).normalized;
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

        // Сповіщуємо WaveManager про знищення ворога (тільки якщо він є на сцені)
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyKilled();
        }

        TryDropLoot();

        // Вимикаємо AI (зомбі або кажан)
        if (enemyAI != null) enemyAI.enabled = false;
        if (batAI != null) batAI.enabled = false;
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
        if (enemyAI != null)
        {
            // Повертаємо в пул, якщо це зомбі
            ZombiePool.Instance.ReturnZombie(enemyAI);
        }
        else
        {
            // Якщо це інший ворог (кажан), просто вимикаємо
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        health = initialHealth; // скидаємо до значення з Inspector, а не хардкоду
        if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = true;
        if (enemyAI != null) enemyAI.enabled = true;
        if (batAI != null) batAI.enabled = true;
    }
}