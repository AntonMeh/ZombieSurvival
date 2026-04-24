using System.Collections.Generic;
using UnityEngine;

public class ZombiePool : MonoBehaviour
{
    public static ZombiePool Instance;

    // Тепер префаб — це посилання на компонент EnemyAI, а не на GameObject.
    // В Inspector перетягуємо той самий префаб — Unity автоматично
    // покаже поле для компонента EnemyAI, що є на цьому префабі.
    [SerializeField] private EnemyAI zombiePrefab;
    [SerializeField] private int poolSize = 20;

    // Черга зберігає типізовані посилання — немає потреби в GetComponent
    private Queue<EnemyAI> pooledZombies = new Queue<EnemyAI>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            // Instantiate<T> повертає EnemyAI напряму.
            // Unity створює GameObject з усіма компонентами префаба
            // і повертає посилання на потрібний компонент — без GetComponent.
            EnemyAI zombie = Instantiate(zombiePrefab);
            zombie.gameObject.SetActive(false);
            pooledZombies.Enqueue(zombie);
        }
    }

    /// <summary>
    /// Повертає готовий компонент EnemyAI з пулу.
    /// Викликач одразу має доступ до всіх полів скрипта без GetComponent.
    /// </summary>
    public EnemyAI GetZombie()
    {
        if (pooledZombies.Count > 0)
        {
            EnemyAI zombie = pooledZombies.Dequeue();
            zombie.gameObject.SetActive(true);
            return zombie;
        }

        return null;
    }

    /// <summary>
    /// Повертає зомбі назад у пул. Приймає EnemyAI напряму.
    /// </summary>
    public void ReturnZombie(EnemyAI zombie)
    {
        zombie.gameObject.SetActive(false);
        pooledZombies.Enqueue(zombie);
    }
}