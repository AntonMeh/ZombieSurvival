using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Універсальний Object Pool для куль різних видів зброї.
/// Керує словником пулів для кожного типу кулі (префабу).
/// </summary>
public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    // Словник, де ключ — це префаб кулі, а значення — черга об'єктів
    private Dictionary<GameObject, Queue<Bullet>> poolDictionary = new Dictionary<GameObject, Queue<Bullet>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Отримує кулю з пулу або створює нову, якщо пул порожній.
    /// </summary>
    public Bullet GetBullet(GameObject bulletPrefab, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(bulletPrefab))
        {
            poolDictionary.Add(bulletPrefab, new Queue<Bullet>());
        }

        Bullet bulletScript = null;

        if (poolDictionary[bulletPrefab].Count > 0)
        {
            bulletScript = poolDictionary[bulletPrefab].Dequeue();
        }
        else
        {
            // Створюємо нову кулю, якщо пул порожній
            GameObject newBullet = Instantiate(bulletPrefab);
            bulletScript = newBullet.GetComponent<Bullet>();
            
            // Записуємо префаб, з якого вона створена, щоб знати, в яку чергу її повертати
            bulletScript.sourcePrefab = bulletPrefab;
        }

        bulletScript.transform.position = position;
        bulletScript.transform.rotation = rotation;
        bulletScript.gameObject.SetActive(true);

        return bulletScript;
    }

    /// <summary>
    /// Повертає кулю в пул.
    /// </summary>
    public void ReturnBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);

        // Скидаємо швидкість перед поверненням у пул
        if (bullet.rb != null)
        {
            bullet.rb.linearVelocity = Vector2.zero;
            bullet.rb.angularVelocity = 0f;
        }

        if (bullet.sourcePrefab != null && poolDictionary.ContainsKey(bullet.sourcePrefab))
        {
            poolDictionary[bullet.sourcePrefab].Enqueue(bullet);
        }
        else
        {
            // Захист на випадок, якщо щось пішло не так
            Destroy(bullet.gameObject);
        }
    }
}
