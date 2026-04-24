using UnityEngine;
using TMPro;

/// <summary>
/// Керує хвилями ворогів на рівні. Замінює ZombieSpawner.
/// Стани: Intermission → Spawning → WaitingForClear → (наступна хвиля або перемога)
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Configuration")]
    public WaveData[] waves;
    public Transform[] spawnPoints;

    [Header("UI")]
    public TMP_Text waveText;        // "Wave: 2/5"
    public TMP_Text enemiesText;     // "Enemies: 7"
    public GameObject intermissionPanel;
    public TMP_Text intermissionTimerText;

    // --- Internal state ---
    private int currentWaveIndex = 0;
    private int enemiesSpawned;
    private int enemiesKilled;
    private float spawnTimer;
    private float intermissionTimer;

    public enum WaveState
    {
        Intermission,
        Spawning,
        WaitingForClear,
        LevelComplete
    }

    public WaveState CurrentState { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("WaveManager: масив waves порожній!");
            return;
        }
        StartIntermission();
    }

    void Update()
    {
        switch (CurrentState)
        {
            case WaveState.Intermission:
                UpdateIntermission();
                break;
            case WaveState.Spawning:
                UpdateSpawning();
                break;
        }
    }

    // ===================== STATE MACHINE =====================

    void StartIntermission()
    {
        CurrentState = WaveState.Intermission;
        intermissionTimer = waves[currentWaveIndex].intermissionDuration;

        if (intermissionPanel != null)
            intermissionPanel.SetActive(true);

        UpdateUI();
    }

    void UpdateIntermission()
    {
        intermissionTimer -= Time.deltaTime;

        if (intermissionTimerText != null)
            intermissionTimerText.text = $"Наступна хвиля через: {Mathf.CeilToInt(intermissionTimer)}";

        if (intermissionTimer <= 0f)
            StartWave();
    }

    void StartWave()
    {
        CurrentState = WaveState.Spawning;
        enemiesSpawned = 0;
        enemiesKilled = 0;
        spawnTimer = 0f; // перший спавн одразу

        if (intermissionPanel != null)
            intermissionPanel.SetActive(false);

        UpdateUI();
    }

    void UpdateSpawning()
    {
        WaveData wave = waves[currentWaveIndex];

        if (enemiesSpawned < wave.enemyCount)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = wave.spawnInterval;
            }
        }
        else
        {
            // Всіх заспавнили — чекаємо поки гравець їх знищить
            CurrentState = WaveState.WaitingForClear;
        }
    }

    void SpawnEnemy()
    {
        EnemyAI zombie = ZombiePool.Instance.GetZombie();
        if (zombie != null)
        {
            int index = Random.Range(0, spawnPoints.Length);
            zombie.transform.position = spawnPoints[index].position;
            enemiesSpawned++;
            UpdateUI();
        }
        // Якщо пул порожній — спробуємо на наступному тіку таймера
    }

    // ===================== CALLED BY EnemyHealth =====================

    /// <summary>
    /// Викликається з EnemyHealth.Die() при смерті ворога.
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        UpdateUI();

        // Хвиля завершена, коли ВСІ вороги заспавнені І вбиті
        bool allSpawned = enemiesSpawned >= waves[currentWaveIndex].enemyCount;
        bool allKilled = enemiesKilled >= waves[currentWaveIndex].enemyCount;

        if (allSpawned && allKilled)
        {
            WaveCompleted();
        }
    }

    void WaveCompleted()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            CurrentState = WaveState.LevelComplete;
            Debug.Log("Усі хвилі пройдено! Перемога!");

            if (UIManager.Instance != null)
                UIManager.Instance.ShowVictoryScreen();
        }
        else
        {
            StartIntermission();
        }
    }

    // ===================== UI =====================

    void UpdateUI()
    {
        if (currentWaveIndex >= waves.Length) return;

        if (waveText != null)
            waveText.text = $"Wave: {currentWaveIndex + 1}/{waves.Length}";

        if (enemiesText != null)
        {
            int remaining = waves[currentWaveIndex].enemyCount - enemiesKilled;
            enemiesText.text = $"Enemies: {remaining}";
        }
    }

    // ===================== PUBLIC API =====================

    public int GetCurrentWaveNumber() => currentWaveIndex + 1;
    public int GetTotalWaves() => waves.Length;
}
