using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI Elements")]
    public Image singleHealthImage; 

    public Sprite[] healthSprites;

    public float invincibilityTime = 1f;
    private float invincibilityTimer;
    private bool isInvincible = false;

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

            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        isInvincible = true;
        invincibilityTimer = invincibilityTime;

        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    void UpdateUI()
    {
        if (singleHealthImage == null || healthSprites == null || healthSprites.Length == 0) return;

        float healthPercent = (float)currentHealth / maxHealth;

        int index = Mathf.FloorToInt(healthPercent * (healthSprites.Length - 1));

        index = Mathf.Clamp(index, 0, healthSprites.Length - 1);

        singleHealthImage.sprite = healthSprites[index];
    }

    void Die()
    {
        Debug.Log("Гравець помер!");

        PlayerInput input = GetComponent<PlayerInput>();
        if (input != null)
        {
            input.enabled = false;
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;

            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        GetComponent<Collider2D>().enabled = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEndScreen();
        }
    }
}