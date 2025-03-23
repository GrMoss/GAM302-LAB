using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private InputActionReference moveActionToUse;

    [SerializeField] private float speed = 5f;

    private Rigidbody2D myRigidbody2D;
    private Vector2 moveDirection;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isRight = true;

    private void Awake()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Lấy SpriteRenderer
    }

    private void OnEnable()
    {
        moveActionToUse.action.Enable();
    }

    private void OnDisable()
    {
        moveActionToUse.action.Disable();
    }

    private void Update()
    {
        Move();
    }

    private void FixedUpdate()
    {
        myRigidbody2D.velocity = moveDirection * speed;
    }

    private void Move()
    {
        moveDirection = moveActionToUse.action.ReadValue<Vector2>();
        Anim();
    }

    private void Anim()
    {
        animator.SetFloat("Speed", moveDirection.magnitude); 
        Flip();
    }

    private void Flip()
    {
        // Lật nhân vật dựa trên hướng di chuyển
        if (moveDirection.x > 0 && !isRight)
        {
            isRight = true;
            spriteRenderer.flipX = false; 
        }
        else if (moveDirection.x < 0 && isRight)
        {
            isRight = false;
            spriteRenderer.flipX = true; 
        }
    }
}