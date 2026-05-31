using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
	[Header("Health Settings")]
	public int maxHealth = 100;
	private int currentHealth;

	// Синхронізована змінна: сервер записує, всі клієнти читають
	private NetworkVariable<int> _currentHealth = new NetworkVariable<int>(
		100, 
		NetworkVariableReadPermission.Everyone, 
		NetworkVariableWritePermission.Server
	);

	[Header("UI Elements")]
	public Image singleHealthImage; 
	public TMPro.TMP_Text healthText;

	public float invincibilityTime = 1f;
	private float invincibilityTimer;
	private bool isInvincible = false;

	private Rigidbody2D rb;
	private bool _isDead = false;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			_currentHealth.OnValueChanged += OnHealthChanged;
			
			if (IsServer)
			{
				_currentHealth.Value = maxHealth;
			}
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			_currentHealth.OnValueChanged -= OnHealthChanged;
		}
	}

	void Start()
	{
		if (CanControl())
		{
			// 1. Спробуємо отримати посилання з UIManager.Instance
			if (UIManager.Instance != null)
			{
				if (singleHealthImage == null)
				{
					singleHealthImage = UIManager.Instance.healthBarFill;
				}
				if (healthText == null)
				{
					healthText = UIManager.Instance.healthText;
				}
			}

			// 2. Якщо не знайдено, спробуємо знайти за назвою (fallback)
			if (singleHealthImage == null)
			{
				GameObject healthBarObj = GameObject.Find("HealthBarFill");
				if (healthBarObj != null)
				{
					singleHealthImage = healthBarObj.GetComponent<Image>();
					Debug.Log("[PlayerHealth] Found HealthBarFill in scene via GameObject.Find.");
				}
			}

			if (healthText == null)
			{
				GameObject textObj = GameObject.Find("HealthText");
				if (textObj != null)
				{
					healthText = textObj.GetComponent<TMPro.TMP_Text>();
					Debug.Log("[PlayerHealth] Found HealthText in scene via GameObject.Find.");
				}
			}

			// Логування та налаштування
			if (singleHealthImage == null)
			{
				Debug.LogError("[PlayerHealth] HealthBarFill Image is NOT found in scene or UIManager! Please assign it in UIManager or verify its name.");
			}
			else
			{
				singleHealthImage.type = Image.Type.Filled;
				singleHealthImage.fillMethod = Image.FillMethod.Horizontal;
				singleHealthImage.fillOrigin = (int)Image.OriginHorizontal.Left;
			}

			if (healthText == null)
			{
				Debug.LogWarning("[PlayerHealth] HealthText is NOT found in scene or UIManager!");
			}
		}

		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			if (IsServer)
			{
				_currentHealth.Value = maxHealth;
			}
		}
		else
		{
			currentHealth = maxHealth;
		}

		UpdateUI();
	}

	void Update()
	{
		if (isInvincible)
		{
			invincibilityTimer -= Time.deltaTime;
			if (invincibilityTimer <= 0f)
			{
				isInvincible = false;
			}
		}
	}

	public void TakeDamage(int damage)
	{
		if (isInvincible) return;

		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			// Клієнтська оптимізація: вмикаємо невразливість локально відразу, щоб не спамити RPC
			if (!IsServer)
			{
				isInvincible = true;
				invincibilityTimer = invincibilityTime;
			}

			TakeDamageServerRpc(damage);
		}
		else
		{
			ApplyDamage(damage);
		}
	}

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	private void TakeDamageServerRpc(int damage)
	{
		ApplyDamage(damage);
	}

	private void ApplyDamage(int damage)
	{
		// На сервері також перевіряємо невразливість
		if (IsServer && isInvincible) return;

		int newHealth = GetHealth() - damage;
		newHealth = Mathf.Clamp(newHealth, 0, maxHealth);

		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			_currentHealth.Value = newHealth;
		}
		else
		{
			currentHealth = newHealth;
		}

		isInvincible = true;
		invincibilityTimer = invincibilityTime;

		UpdateUI();

		if (GetHealth() <= 0)
		{
			Die();
		}
	}

	private void OnHealthChanged(int previousValue, int newValue)
	{
		UpdateUI();
		if (newValue <= 0)
		{
			Die();
		}
	}

	public int GetHealth()
	{
		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			return _currentHealth.Value;
		}
		return currentHealth;
	}

	void UpdateUI()
	{
		if (singleHealthImage == null) return;

		float healthPercent = (float)GetHealth() / maxHealth;
		singleHealthImage.fillAmount = Mathf.Clamp01(healthPercent);

		if (healthText != null)
		{
			healthText.text = $"{Mathf.Max(0, GetHealth())} / {maxHealth}";
		}
	}

	void Die()
	{
		if (_isDead) return;
		_isDead = true;

		Debug.Log("Гравець помер!");

		PlayerInput input = GetComponent<PlayerInput>();
		if (input != null)
		{
			input.enabled = false;
		}

		rb = GetComponent<Rigidbody2D>();
		if (rb != null)
		{
			rb.linearVelocity = Vector2.zero;
			rb.constraints = RigidbodyConstraints2D.FreezeAll;
		}

		GetComponent<Collider2D>().enabled = false;

		if (CanControl() && UIManager.Instance != null)
		{
			UIManager.Instance.ShowEndScreen();
		}

		// Знищення об'єкта гравця при смерті
		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			if (IsServer)
			{
				// Delay despawning to allow the NetworkVariable health update (0 HP) 
				// to replicate to clients, so they can trigger their local death UI.
				Invoke(nameof(DespawnPlayer), 0.2f);
			}
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void DespawnPlayer()
	{
		NetworkObject netObj = GetComponent<NetworkObject>();
		if (netObj != null && netObj.IsSpawned)
		{
			netObj.Despawn(true);
		}
	}

	private bool CanControl()
	{
		if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient))
		{
			return true;
		}
		return IsOwner;
	}
}