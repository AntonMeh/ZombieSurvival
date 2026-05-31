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
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

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
		if (!IsOwner)
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
		if (!IsOwner)
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

	#endregion
}