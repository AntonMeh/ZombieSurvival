using System.Collections.Generic;
using UnityEngine;

public class ZombiePool : MonoBehaviour
{
    public static ZombiePool Instance; 

    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<GameObject> pooledZombies = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject zombie = Instantiate(zombiePrefab);
            zombie.SetActive(false); 
            pooledZombies.Enqueue(zombie);
        }
    }

    public GameObject GetZombie()
    {
        if (pooledZombies.Count > 0)
        {
            GameObject zombie = pooledZombies.Dequeue();
            zombie.SetActive(true);
            return zombie;
        }

        return null;
    }

    public void ReturnZombie(GameObject zombie)
    {
        zombie.SetActive(false);
        pooledZombies.Enqueue(zombie);
    }
}