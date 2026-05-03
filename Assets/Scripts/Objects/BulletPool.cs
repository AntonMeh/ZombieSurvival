using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

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

            GameObject newBullet = Instantiate(bulletPrefab);
            bulletScript = newBullet.GetComponent<Bullet>();

            bulletScript.sourcePrefab = bulletPrefab;
        }

        bulletScript.transform.position = position;
        bulletScript.transform.rotation = rotation;
        bulletScript.gameObject.SetActive(true);

        return bulletScript;
    }

    public void ReturnBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);

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

            Destroy(bullet.gameObject);
        }
    }
}
