using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Універсальний Object Pool для будь-яких ворогів.
/// Працює з будь-якими префабами — зомбі, кажани, боси тощо.
/// Просто додай префаб у WaveData і він автоматично потрапить у пул.
/// </summary>
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [Tooltip("Скільки об'єктів кожного типу створити заздалегідь")]
    [SerializeField] private int preloadCount = 5;

    // Словник: ключ = префаб, значення = черга готових об'єктів
    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

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
    /// Заздалегідь створює об'єкти для конкретного префабу.
    /// Викликається з WaveManager на початку рівня.
    /// </summary>
    public void Preload(GameObject prefab, int count)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pools[prefab].Enqueue(obj);
        }
    }

    /// <summary>
    /// Отримує ворога з пулу або створює нового, якщо пул порожній.
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        GameObject obj;

        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
        }
        else
        {
            // Пул порожній — створюємо новий об'єкт
            obj = Instantiate(prefab);
        }

        obj.transform.position = position;
        obj.SetActive(true);

        // Прив'язуємо префаб, щоб EnemyHealth знав куди повертатися
        EnemyHealth eh = obj.GetComponent<EnemyHealth>();
        if (eh != null)
            eh.sourcePrefab = prefab;

        return obj;
    }

    /// <summary>
    /// Повертає ворога назад у пул.
    /// </summary>
    public void Return(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);

        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        pools[prefab].Enqueue(obj);
    }
}
