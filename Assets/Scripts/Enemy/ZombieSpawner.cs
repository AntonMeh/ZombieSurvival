using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public Transform[] spawnPoints; 
    public float spawnInterval = 3f;
    private float nextSpawnTime;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnZombie();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnZombie()
    {
        GameObject zombie = ZombiePool.Instance.GetZombie();
        if (zombie != null)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            zombie.transform.position = spawnPoints[randomIndex].position;
        }
    }
}