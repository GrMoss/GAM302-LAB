using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D myRigidbody2D;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;

    private Vector2 moveInput;

    [Networked] private bool IsFacingRight { get; set; }
    [Networked] public NetworkBool IsAlive { get; set; } = true;

    private void Awake()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    public override void Spawned()
    {
        IsFacingRight = true;
        moveInput = Vector2.zero;
        IsAlive = true;

        if (myRigidbody2D != null)
        {
            myRigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
            myRigidbody2D.simulated = true;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!HasStateAuthority) return;
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (!HasStateAuthority || !IsAlive)
        {
            myRigidbody2D.velocity = Vector2.zero; // Dừng di chuyển khi không còn sống
            return;
        }

        myRigidbody2D.velocity = moveInput * speed;
        Flip();
    }

    public override void Render()
    {
        // Không ẩn spriteRenderer khi chết, chỉ cập nhật animation và hướng
        spriteRenderer.flipX = !IsFacingRight;
        animator.SetFloat("Speed", IsAlive ? moveInput.magnitude : 0f); // Dừng animation di chuyển khi chết
    }

    private void Flip()
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Die()
    {
        IsAlive = false;
        myRigidbody2D.simulated = false; // Vô hiệu hóa di chuyển
        myRigidbody2D.velocity = Vector2.zero;

        if (HasStateAuthority)
        {
            Debug.Log($"Player {Object.Id} died!");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Respawn()
    {
        IsAlive = true;
        myRigidbody2D.simulated = true; // Kích hoạt lại di chuyển
        myRigidbody2D.velocity = Vector2.zero;

        if (HasStateAuthority)
        {
            Debug.Log($"Player {Object.Id} respawned and can move again!");
        }
    }
}