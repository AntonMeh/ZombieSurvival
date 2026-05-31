using Unity.Netcode;
using UnityEngine;
using TMPro;

public class WaveManager : NetworkBehaviour
{
	public static WaveManager Instance { get; private set; }

	[Header("Wave Configuration")]
	public WaveData[] waves;
	public Transform[] spawnPoints;

	[Header("UI")]
	public TMP_Text waveText;        
	public TMP_Text enemiesText;     
	public GameObject intermissionPanel;
	public TMP_Text intermissionTimerText;

	private int currentWaveIndex = 0;
	private int enemiesSpawned;
	private int enemiesKilled;
	private bool bossSpawned = false;   
	private float spawnTimer;
	private float intermissionTimer;

	public enum WaveState
	{
		Intermission,
		Spawning,
		WaitingForClear,
		LevelComplete
	}

	public WaveState CurrentState { get; private set; } = WaveState.Intermission;

	// Network Variables to sync wave state to clients
	private NetworkVariable<int> _currentWaveIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	private NetworkVariable<int> _enemiesKilled = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	private NetworkVariable<int> _enemiesSpawned = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	private NetworkVariable<float> _intermissionTimer = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	private NetworkVariable<int> _currentState = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // Cast to int
	private NetworkVariable<bool> _bossSpawned = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

		bool isMultiplayer = NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);
		if (!isMultiplayer || IsServer)
		{
			StartIntermission();
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (IsServer)
		{
			_currentWaveIndex.Value = currentWaveIndex;
			_enemiesKilled.Value = enemiesKilled;
			_enemiesSpawned.Value = enemiesSpawned;
			_intermissionTimer.Value = intermissionTimer;
			_currentState.Value = (int)CurrentState;
			_bossSpawned.Value = bossSpawned;
		}
	}

	void Update()
	{
		bool isMultiplayer = NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);

		if (isMultiplayer)
		{
			if (IsServer)
			{
				ServerUpdate();

				// Synchronize state to clients
				_currentWaveIndex.Value = currentWaveIndex;
				_enemiesKilled.Value = enemiesKilled;
				_enemiesSpawned.Value = enemiesSpawned;
				_intermissionTimer.Value = intermissionTimer;
				_currentState.Value = (int)CurrentState;
				_bossSpawned.Value = bossSpawned;
			}
			else
			{
				// Read state on clients
				currentWaveIndex = _currentWaveIndex.Value;
				enemiesKilled = _enemiesKilled.Value;
				enemiesSpawned = _enemiesSpawned.Value;
				intermissionTimer = _intermissionTimer.Value;
				CurrentState = (WaveState)_currentState.Value;
				bossSpawned = _bossSpawned.Value;

				// Update client UI
				if (intermissionPanel != null)
				{
					intermissionPanel.SetActive(CurrentState == WaveState.Intermission);
				}
				if (intermissionTimerText != null && CurrentState == WaveState.Intermission)
				{
					intermissionTimerText.text = $"{Mathf.CeilToInt(intermissionTimer)}";
				}
				UpdateUI();
			}
		}
		else
		{
			ServerUpdate();
		}
	}

	void ServerUpdate()
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

	void StartIntermission()
	{
		CurrentState = WaveState.Intermission;
		intermissionTimer = waves[currentWaveIndex].intermissionDuration;

		if (intermissionPanel != null)
		{
			intermissionPanel.SetActive(true);
		}

		UpdateUI();
	}

	void UpdateIntermission()
	{
		intermissionTimer -= Time.deltaTime;

		if (intermissionTimerText != null)
		{
			intermissionTimerText.text = $"{Mathf.CeilToInt(intermissionTimer)}";
		}

		if (intermissionTimer <= 0f)
		{
			StartWave();
		}
	}

	void StartWave()
	{
		CurrentState = WaveState.Spawning;
		enemiesSpawned = 0;
		enemiesKilled = 0;
		bossSpawned = false;
		spawnTimer = 0f; 

		if (intermissionPanel != null)
		{
			intermissionPanel.SetActive(false);
		}

		WaveData wave = waves[currentWaveIndex];
		if (wave.isBossWave && wave.bossPrefab != null)
		{
			Vector3 spawnPos = spawnPoints[0].position; 
			GameObject obj = EnemyPool.Instance.Get(wave.bossPrefab, spawnPos);

			bool isMultiplayer = NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);
			if (isMultiplayer && NetworkManager.Singleton.IsServer)
			{
				NetworkObject netObj = obj.GetComponent<NetworkObject>();
				if (netObj != null)
				{
					netObj.Spawn(true);
				}
			}

			bossSpawned = true;
		}

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
			CurrentState = WaveState.WaitingForClear;
		}
	}

	void SpawnEnemy()
	{
		WaveData wave = waves[currentWaveIndex];

		if (wave.enemies == null || wave.enemies.Length == 0)
		{
			Debug.LogWarning("WaveManager: у хвилі немає жодного ворога!");
			enemiesSpawned++;
			return;
		}

		Vector3 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

		GameObject prefab = GetWeightedRandomEnemy(wave.enemies);

		if (prefab != null && EnemyPool.Instance != null)
		{
			GameObject obj = EnemyPool.Instance.Get(prefab, spawnPos);

			bool isMultiplayer = NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);
			if (isMultiplayer && NetworkManager.Singleton.IsServer)
			{
				NetworkObject netObj = obj.GetComponent<NetworkObject>();
				if (netObj != null)
				{
					netObj.Spawn(true);
				}
			}

			enemiesSpawned++;
			UpdateUI();
		}
	}

	GameObject GetWeightedRandomEnemy(EnemySpawnEntry[] entries)
	{
		int totalWeight = 0;
		foreach (var entry in entries)
		{
			totalWeight += entry.spawnWeight;
		}

		int roll = Random.Range(0, totalWeight);
		int cumulative = 0;

		foreach (var entry in entries)
		{
			cumulative += entry.spawnWeight;
			if (roll < cumulative)
			{
				return entry.prefab;
			}
		}

		return entries[0].prefab; 
	}

	public void OnEnemyKilled()
	{
		bool isMultiplayer = NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);
		if (isMultiplayer && !NetworkManager.Singleton.IsServer)
		{
			return;
		}

		enemiesKilled++;
		UpdateUI();

		WaveData wave = waves[currentWaveIndex];

		int totalCount = wave.enemyCount + (wave.isBossWave && bossSpawned ? 1 : 0);
		bool allSpawned = enemiesSpawned >= wave.enemyCount; 
		bool allKilled  = enemiesKilled >= totalCount;       

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
			{
				UIManager.Instance.ShowVictoryScreen();
			}
		}
		else
		{
			StartIntermission();
		}
	}

	void UpdateUI()
	{
		if (currentWaveIndex >= waves.Length) return;

		if (waveText != null)
		{
			waveText.text = $"Wave: {currentWaveIndex + 1}/{waves.Length}";
		}

		if (enemiesText != null)
		{
			WaveData wave = waves[currentWaveIndex];

			if (CurrentState == WaveState.Intermission)
			{
				int totalCount = wave.enemyCount + (wave.isBossWave ? 1 : 0);
				enemiesText.text = $"Enemies: {totalCount}";
			}
			else
			{
				int totalCount = wave.enemyCount + (wave.isBossWave && bossSpawned ? 1 : 0);
				int remaining = Mathf.Max(0, totalCount - enemiesKilled);
				enemiesText.text = $"Enemies: {remaining}";
			}
		}
	}

	public int GetCurrentWaveNumber() => Mathf.Min(currentWaveIndex + 1, waves.Length);
	public int GetTotalWaves() => waves.Length;
}
