using System.Collections.Generic;
using Unity.Netcode;
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

	void Start()
	{
		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			if (WaveManager.Instance != null && WaveManager.Instance.waves != null)
			{
				HashSet<GameObject> uniquePrefabs = new HashSet<GameObject>();
				foreach (WaveData wave in WaveManager.Instance.waves)
				{
					if (wave.bossPrefab != null)
					{
						uniquePrefabs.Add(wave.bossPrefab);
					}

					if (wave.enemies != null)
					{
						foreach (var entry in wave.enemies)
						{
							if (entry.prefab != null)
							{
								uniquePrefabs.Add(entry.prefab);
							}
						}
					}
				}

				foreach (GameObject prefab in uniquePrefabs)
				{
					if (!NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(prefab))
					{
						NetworkManager.Singleton.AddNetworkPrefab(prefab);
					}

					NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new EnemyNetworkPoolHandler(prefab));
					Debug.Log($"[EnemyPool] Registered NetworkPrefabInstanceHandler for {prefab.name}");
				}
			}
		}
	}

	public void Preload(GameObject prefab, int count)
	{
		if (!pools.ContainsKey(prefab))
		{
			pools[prefab] = new Queue<GameObject>();
		}

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
		{
			pools[prefab] = new Queue<GameObject>();
		}

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
		{
			eh.sourcePrefab = prefab;
		}

		return obj;
	}

	public void Return(GameObject prefab, GameObject obj)
	{
		obj.SetActive(false);

		if (!pools.ContainsKey(prefab))
		{
			pools[prefab] = new Queue<GameObject>();
		}

		pools[prefab].Enqueue(obj);
	}
}

public class EnemyNetworkPoolHandler : INetworkPrefabInstanceHandler
{
	private GameObject prefab;

	public EnemyNetworkPoolHandler(GameObject prefab)
	{
		this.prefab = prefab;
	}

	public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
	{
		GameObject obj = EnemyPool.Instance.Get(prefab, position);
		obj.transform.rotation = rotation;
		
		NetworkObject netObj = obj.GetComponent<NetworkObject>();
		return netObj;
	}

	public void Destroy(NetworkObject networkObject)
	{
		if (EnemyPool.Instance != null && prefab != null && networkObject != null)
		{
			EnemyPool.Instance.Return(prefab, networkObject.gameObject);
		}
		else if (networkObject != null)
		{
			networkObject.gameObject.SetActive(false);
		}
	}
}
