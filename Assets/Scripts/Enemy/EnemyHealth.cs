using Unity.Netcode;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
	public int health = 3;
	private int initialHealth;
	public Animator animator;
	private Rigidbody2D rb;
	private Collider2D col;

	[Header("Pool")]
	[Tooltip("Префаб, з якого створений цей ворог. Заповнюється автоматично пулом.")]
	[HideInInspector] public GameObject sourcePrefab;

	[Header("Loot Settings")]
	public GameObject coinPrefab;
	[Range(0, 100)] public float dropChance = 50f;

	[Header("Score Settings")]
	public int pointsValue = 100;

	private bool isBoss = false;
	private bool isDead = false;

	void Awake()
	{
		initialHealth = health;
	}

	void Start()
	{
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		col = GetComponent<Collider2D>();
		isBoss = GetComponent<BossGolemAI>() != null;
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			if (!IsServer)
			{
				if (rb == null)
				{
					rb = GetComponent<Rigidbody2D>();
				}
				if (rb != null)
				{
					rb.bodyType = RigidbodyType2D.Kinematic;
					rb.linearVelocity = Vector2.zero;
					rb.angularVelocity = 0f;
				}
			}
		}
	}

	public void TakeDamage(int damage)
	{
		bool isMultiplayer = NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);
		if (isMultiplayer && !NetworkManager.Singleton.IsServer)
		{
			// Clients do not process damage directly; only server does
			return;
		}

		if (isDead) return;

		health -= damage;

		animator.SetTrigger("Hurt");

		Transform nearestPlayer = PlayerController.GetNearestPlayer(transform.position);
		if (nearestPlayer != null)
		{
			Vector2 knockback = (transform.position - nearestPlayer.position).normalized;
			rb.AddForce(knockback * 2f, ForceMode2D.Impulse);
		}

		if (isBoss && BossHealthBar.Instance != null)
		{
			BossHealthBar.Instance.UpdateHealth(Mathf.Max(0, health), initialHealth);
		}

		if (health <= 0)
		{
			isDead = true;
			Die();
		}
	}

	void Die()
	{
		animator.SetTrigger("Die");

		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.AddScore(pointsValue);
		}

		if (WaveManager.Instance != null)
		{
			WaveManager.Instance.OnEnemyKilled();
		}

		TryDropLoot();

		foreach (var ai in GetComponents<MonoBehaviour>())
		{
			if (ai != this && ai is not EnemyHealth)
			{
				ai.enabled = false;
			}
		}

		rb.linearVelocity = Vector2.zero;
		if (col != null)
		{
			col.enabled = false;
		}

		bool isMultiplayer = NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);
		if (isMultiplayer)
		{
			if (NetworkObject != null && NetworkObject.IsSpawned && NetworkManager.Singleton.IsServer)
			{
				// Delay despawning to show the death animation
				Invoke(nameof(DespawnEnemy), 1f);
			}
		}
		else
		{
			Invoke("BackToPool", 1f);
		}
	}

	private void DespawnEnemy()
	{
		if (NetworkObject != null && NetworkObject.IsSpawned)
		{
			NetworkObject.Despawn(true);
		}
	}

	void TryDropLoot()
	{
		float randomValue = Random.Range(0f, 100f);
		if (randomValue <= dropChance)
		{
			Instantiate(coinPrefab, transform.position, Quaternion.identity);
		}
	}

	void BackToPool()
	{
		if (EnemyPool.Instance != null && sourcePrefab != null)
		{
			EnemyPool.Instance.Return(sourcePrefab, gameObject);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	void OnEnable()
	{
		isDead = false;
		health = initialHealth;
		if (col != null)
		{
			col.enabled = true;
		}

		bool isMultiplayer = NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);
		bool shouldEnableAI = !isMultiplayer || NetworkManager.Singleton.IsServer;

		foreach (var ai in GetComponents<MonoBehaviour>())
		{
			if (ai != this)
			{
				if (ai is ZombieAI || ai is GolemAI || ai is BossGolemAI || ai is BatAI)
				{
					ai.enabled = shouldEnableAI;
				}
				else
				{
					ai.enabled = true;
				}
			}
		}
	}
}