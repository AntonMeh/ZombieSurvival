using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
	#region Public Fields

	public static PlayerController Instance { get; private set; }

	public PlayerInventory Inventory { get; private set; }

	public float speed = 5f;

	public Animator animator;

	#endregion

	#region Private Fields

	private Rigidbody2D rb;
	private Vector2 moveInput;

	#endregion

	#region Unity Lifecycle

	private void Awake()
	{
		Inventory = GetComponent<PlayerInventory>();

		// If we are in single-player mode, assign the static Instance immediately
		if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient))
		{
			Instance = this;
		}
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		// If we are in multiplayer mode and this is a pre-placed scene object, destroy/despawn it!
		if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
		{
			bool isSceneObj = false;
			NetworkObject netObj = null;
			if (TryGetComponent<NetworkObject>(out netObj))
			{
				if (netObj.IsSceneObject == true || !netObj.IsSpawned)
				{
					isSceneObj = true;
				}
			}
			else
			{
				isSceneObj = true;
			}

			if (isSceneObj)
			{
				Debug.Log($"[PlayerController] Identified pre-placed scene player {gameObject.name}. Cleaning up...");
				if (NetworkManager.Singleton.IsServer)
				{
					if (netObj != null && netObj.IsSpawned)
					{
						netObj.Despawn(true);
					}
					else
					{
						Destroy(gameObject);
					}
				}
				else
				{
					gameObject.SetActive(false);
				}
				return;
			}
		}

		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
		{
			foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
			{
				spriteRenderer.enabled = false;
			}

			foreach (var col in GetComponentsInChildren<Collider2D>())
			{
				col.enabled = false;
			}

			if (TryGetComponent(out Rigidbody2D body))
			{
				body.simulated = false;
			}

			var shooting = GetComponent<PlayerShooting>();
			if (shooting != null)
			{
				shooting.enabled = false;
			}

			this.enabled = false;
		}
	}

	private void Update()
	{
		if (!CanControl())
		{
			return;
		}

		if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
		{
			moveInput = Vector2.zero;
			animator.SetFloat("Speed", 0f);
			return;
		}

		float moveX = 0;
		float moveY = 0;
		moveInput = Vector2.zero; // Скидаємо рух кожен кадр

		if (!Application.isMobilePlatform && Keyboard.current != null)
		{
			if (Keyboard.current.wKey != null && Keyboard.current.wKey.isPressed)
			{
				moveY = 1;
			}
			if (Keyboard.current.sKey != null && Keyboard.current.sKey.isPressed)
			{
				moveY = -1;
			}
			if (Keyboard.current.aKey != null && Keyboard.current.aKey.isPressed)
			{
				moveX = -1;
			}
			if (Keyboard.current.dKey != null && Keyboard.current.dKey.isPressed)
			{
				moveX = 1;
			}

			moveInput = new Vector2(moveX, moveY).normalized;
		}

		if (SimpleJoystick.MoveJoy != null && SimpleJoystick.MoveJoy.InputVector.magnitude > 0.1f)
		{
			moveInput = SimpleJoystick.MoveJoy.InputVector.normalized;
		}

		if (moveInput.magnitude > 0)
		{
			animator.SetFloat("Horizontal", moveInput.x);
			animator.SetFloat("Vertical", moveInput.y);

			animator.SetFloat("LastHorizontal", moveInput.x);
			animator.SetFloat("LastVertical", moveInput.y);
		}

		animator.SetFloat("Speed", moveInput.magnitude);
	}

	private void FixedUpdate()
	{
		if (!CanControl())
		{
			return;
		}

		rb.linearVelocity = moveInput * speed;
	}

	#endregion

	#region Network Lifecycle

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (IsOwner)
		{
			Instance = this;

			// Hook up Cinemachine Camera to follow this local player
			var cinemachineCam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
			if (cinemachineCam != null)
			{
				cinemachineCam.Follow = transform;
				Debug.Log("[PlayerController] Cinemachine Camera follow target set to local player.");
			}
		}
		else
		{
			Rigidbody2D body = GetComponent<Rigidbody2D>();
			if (body != null)
			{
				body.bodyType = RigidbodyType2D.Kinematic;
				body.linearVelocity = Vector2.zero;
				body.angularVelocity = 0f;
			}
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		if (IsOwner && Instance == this)
		{
			Instance = null;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (Instance == this)
		{
			Instance = null;
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

	public static Transform GetNearestPlayer(Vector3 position)
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		Transform nearest = null;
		float minDistance = float.MaxValue;

		foreach (GameObject player in players)
		{
			PlayerHealth health = player.GetComponent<PlayerHealth>();
			if (health != null && health.GetHealth() > 0)
			{
				float dist = Vector2.Distance(position, player.transform.position);
				if (dist < minDistance)
				{
					minDistance = dist;
					nearest = player.transform;
				}
			}
		}

		return nearest;
	}

	#endregion
}