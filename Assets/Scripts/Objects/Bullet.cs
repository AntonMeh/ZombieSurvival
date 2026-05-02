using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    
    [HideInInspector] public GameObject sourcePrefab; // Для повернення в правильний пул
    [HideInInspector] public Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    void OnEnable()
    {
        // Повертаємо в пул через 2 секунди, якщо нікуди не влучили
        Invoke("DisableBullet", 2f);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void DisableBullet()
    {
        if (BulletPool.Instance != null)
            BulletPool.Instance.ReturnBullet(this);
        else
            gameObject.SetActive(false); // Fallback
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        EnemyHealth enemy = hitInfo.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if (!hitInfo.CompareTag("Player") && !hitInfo.CompareTag("Bullet"))
        {
            DisableBullet();
        }
    }
}