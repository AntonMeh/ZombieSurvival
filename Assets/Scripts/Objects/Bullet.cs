using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;

    [HideInInspector] public GameObject sourcePrefab; 
    [HideInInspector] public Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    private float lifetime = 2f;

    public void SetLifetime(float time)
    {
        lifetime = time;
    }

    void OnEnable()
    {
        Invoke("DisableBullet", lifetime);
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
            gameObject.SetActive(false); 
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