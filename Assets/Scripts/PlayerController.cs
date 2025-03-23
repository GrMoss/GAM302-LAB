using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private InputActionReference moveActionToUse;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float maxHealth = 100f;

    private Rigidbody2D myRigidbody2D;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Networked] 
    private Vector2 MoveDirection { get; set; }
    [Networked] 
    private bool IsFacingRight { get; set; }
    [Networked] 
    public float Health { get; set; }

    private void Awake()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void Spawned()
    {
        IsFacingRight = true;
        MoveDirection = Vector2.zero;
        Health = maxHealth;
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
        if (!HasStateAuthority) return;
        Move();
    }

    public override void FixedUpdateNetwork()
    {
        myRigidbody2D.velocity = MoveDirection * speed;
    }

    public override void Render()
    {
        spriteRenderer.flipX = !IsFacingRight;
        animator.SetFloat("Speed", MoveDirection.magnitude);
    }

    private void Move()
    {
        Vector2 newMoveDirection = moveActionToUse.action.ReadValue<Vector2>();
        MoveDirection = newMoveDirection;
        Flip();
    }

    private void Flip()
    {
        if (MoveDirection.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (MoveDirection.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!HasStateAuthority) return;
        Health = Mathf.Max(Health - damage, 0);
        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Runner.Despawn(Object);
    }
}