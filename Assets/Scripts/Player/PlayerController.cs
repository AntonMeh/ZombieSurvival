using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public static PlayerController Instance { get; private set; }

    public PlayerInventory Inventory { get; private set; }

    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    public Animator animator;

    void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Inventory = GetComponent<PlayerInventory>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {

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
            if (Keyboard.current.wKey != null && Keyboard.current.wKey.isPressed) moveY = 1;
            if (Keyboard.current.sKey != null && Keyboard.current.sKey.isPressed) moveY = -1;
            if (Keyboard.current.aKey != null && Keyboard.current.aKey.isPressed) moveX = -1;
            if (Keyboard.current.dKey != null && Keyboard.current.dKey.isPressed) moveX = 1;

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

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed;
    }
}