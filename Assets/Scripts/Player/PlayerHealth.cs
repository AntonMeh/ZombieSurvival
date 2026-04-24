using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Обов'язково для роботи з UI Image

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI Elements")]
    public Image singleHealthImage; // UI Image, який ми створили в Кроці 1

    // Масив спрайтів. Дуже важливо перетягнути їх у цьому порядку:
    //
    public Sprite[] healthSprites;

    // Невразливість (i-frames) після удару
    public float invincibilityTime = 1f;
    private float invincibilityTimer;
    private bool isInvincible = false;

    // Посилання на інші системи
    public GameOverUI gameOverUI; // Знайди GameOverUI об'єкт на сцені
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                // Тут можна додати блимання спрайта, коли i-frames закінчилися
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Мінімальна пауза 0.1с, щоб не вмерти за 1 кадр від багу фізики
        if (isInvincible && invincibilityTimer > (invincibilityTime - 0.1f)) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Візуальна невразливість (для анімації блимання)
        isInvincible = true;
        invincibilityTimer = invincibilityTime;

        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    void UpdateUI()
    {
        if (singleHealthImage == null || healthSprites == null || healthSprites.Length == 0) return;

        // Розраховуємо відсоток здоров'я (0.0f - 1.0f)
        float healthPercent = (float)currentHealth / maxHealth;

        // Перетворюємо відсоток в індекс масиву. Маємо 6 спрайтів (індекси 0-5).
        // Array:
        // Індекси:   0       1       2       3       4       5
        // Життя:   0-16%   16-33%   33-50%   50-66%   66-83%   83-100%

        int index = Mathf.FloorToInt(healthPercent * (healthSprites.Length - 1));

        // Гарантуємо, що індекс у межах масиву
        index = Mathf.Clamp(index, 0, healthSprites.Length - 1);

        // Змінюємо спрайт на UI
        singleHealthImage.sprite = healthSprites[index];
    }

    void Die()
    {
        Debug.Log("Гравець помер!");

        // 1. Вимикаємо Input System, щоб гравець перестав реагувати на кнопки
        PlayerInput input = GetComponent<PlayerInput>();
        if (input != null)
        {
            input.enabled = false;
        }

        // 2. Зупиняємо фізику (щоб він не ковзав по інерції)
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // Заморожуємо позицію, щоб вороги не могли його штовхати після смерті
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // 3. Вимикаємо коллайдер (щоб зомбі більше не кусали "труп")
        GetComponent<Collider2D>().enabled = false;

        // 4. Показуємо вікно програшу
        if (gameOverUI != null)
        {
            UIManager.Instance.ShowEndScreen();
        }
    }
}