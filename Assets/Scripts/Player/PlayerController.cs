using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float moveX = 0;
        float moveY = 0;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveY = 1;
            if (Keyboard.current.sKey.isPressed) moveY = -1;
            if (Keyboard.current.aKey.isPressed) moveX = -1;
            if (Keyboard.current.dKey.isPressed) moveX = 1;

            moveInput = new Vector2(moveX, moveY).normalized;
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