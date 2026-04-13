using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Ми прибрали speed, бо силу дає пістолет
    public int damage = 1;

    void Start()
    {
        // Куля просто живе 2 секунди
        Destroy(gameObject, 2f);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Якщо це не гравець, то знищуємо кулю (можна додати перевірку на ворога)
        if (!hitInfo.CompareTag("Player"))
        {
            Debug.Log("Влучання в: " + hitInfo.name);
            Destroy(gameObject);
        }
    }
}