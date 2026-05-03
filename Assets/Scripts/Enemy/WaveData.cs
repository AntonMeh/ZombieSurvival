using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "ZombieSurvival/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Enemies")]
    [Tooltip("Кількість звичайних ворогів у хвилі (бос не рахується тут)")]
    public int enemyCount = 10;

    [Tooltip("Інтервал між спавнами (секунди)")]
    public float spawnInterval = 2f;

    [Header("Enemy Types")]
    [Tooltip("Перетягни сюди всі префаби ворогів, які можуть з'являтися у цій хвилі")]
    public EnemySpawnEntry[] enemies;

    [Header("Boss Wave")]
    [Tooltip("Якщо увімкнено — на початку хвилі з'явиться один бос")]
    public bool isBossWave = false;

    [Tooltip("Префаб боса (BossGolem або будь-який інший)")]
    public GameObject bossPrefab;

    [Header("Timing")]
    [Tooltip("Пауза перед початком цієї хвилі")]
    public float intermissionDuration = 5f;
}

[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("Префаб ворога (будь-який: зомбі, кажан, бос...)")]
    public GameObject prefab;

    [Tooltip("Вага спавну. Якщо зомбі=70, кажан=30, то зомбі з'являтиметься ~70% часу")]
    public int spawnWeight = 1;
}
