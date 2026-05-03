using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

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

            obj = Instantiate(prefab);
        }

        obj.transform.position = position;
        obj.SetActive(true);

        EnemyHealth eh = obj.GetComponent<EnemyHealth>();
        if (eh != null)
            eh.sourcePrefab = prefab;

        return obj;
    }

    public void Return(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);

        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        pools[prefab].Enqueue(obj);
    }
}
