using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    void Start()
    {
        Destroy(gameObject, 2f);
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
            Destroy(gameObject);
        }
    }
}