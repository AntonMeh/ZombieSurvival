using System.Collections.Generic;
using UnityEngine;

public class ZombiePool : MonoBehaviour
{
    public static ZombiePool Instance;

    // Тепер префаб — це посилання на компонент ZombieAI, а не на GameObject.
    // В Inspector перетягуємо той самий префаб — Unity автоматично
    // покаже поле для компонента ZombieAI, що є на цьому префабі.
    [SerializeField] private ZombieAI zombiePrefab;
    [SerializeField] private int poolSize = 20;

    // Черга зберігає типізовані посилання — немає потреби в GetComponent
    private Queue<ZombieAI> pooledZombies = new Queue<ZombieAI>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            // Instantiate<T> повертає ZombieAI напряму.
            // Unity створює GameObject з усіма компонентами префаба
            // і повертає посилання на потрібний компонент — без GetComponent.
            ZombieAI zombie = Instantiate(zombiePrefab);
            zombie.gameObject.SetActive(false);
            pooledZombies.Enqueue(zombie);
        }
    }

    /// <summary>
    /// Повертає готовий компонент ZombieAI з пулу.
    /// Викликач одразу має доступ до всіх полів скрипта без GetComponent.
    /// </summary>
    public ZombieAI GetZombie()
    {
        if (pooledZombies.Count > 0)
        {
            ZombieAI zombie = pooledZombies.Dequeue();
            zombie.gameObject.SetActive(true);
            return zombie;
        }

        return null;
    }

    /// <summary>
    /// Повертає зомбі назад у пул. Приймає ZombieAI напряму.
    /// </summary>
    public void ReturnZombie(ZombieAI zombie)
    {
        if (zombie == null)
        {
            // Якщо це не зомбі (наприклад, кажан без ZombieAI), 
            // просто деактивуємо об'єкт. 
            // В ідеалі для кожного типу ворога має бути свій пул.
            return;
        }

        zombie.gameObject.SetActive(false);
        pooledZombies.Enqueue(zombie);
    }
}