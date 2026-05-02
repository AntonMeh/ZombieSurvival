using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    private int initialHealth;
    public Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;

    [Header("Pool")]
    [Tooltip("Префаб, з якого створений цей ворог. Заповнюється автоматично пулом.")]
    [HideInInspector] public GameObject sourcePrefab;

    [Header("Loot Settings")]
    public GameObject coinPrefab;
    [Range(0, 100)] public float dropChance = 50f;

    [Header("Score Settings")]
    public int pointsValue = 100;

    void Awake()
    {
        initialHealth = health;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        animator.SetTrigger("Hurt");
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

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyKilled();
        }

        TryDropLoot();

        // Вимикаємо всі AI-компоненти на цьому об'єкті
        foreach (var ai in GetComponents<MonoBehaviour>())
        {
            if (ai != this && ai is not EnemyHealth)
                ai.enabled = false;
        }

        rb.linearVelocity = Vector2.zero;
        if (col != null) col.enabled = false;

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
        if (EnemyPool.Instance != null && sourcePrefab != null)
        {
            EnemyPool.Instance.Return(sourcePrefab, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        health = initialHealth;
        if (col != null) col.enabled = true;

        // Вмикаємо всі AI-компоненти назад
        foreach (var ai in GetComponents<MonoBehaviour>())
        {
            if (ai != this)
                ai.enabled = true;
        }
    }
}