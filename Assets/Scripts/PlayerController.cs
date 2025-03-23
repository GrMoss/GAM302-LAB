using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private InputActionReference moveActionToUse;
    [SerializeField] private float speed = 5f;

    private Rigidbody2D myRigidbody2D;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Networked] private Vector2 MoveDirection { get; set; }
    [Networked] private bool IsFacingRight { get; set; }
    [Networked] public NetworkBool IsAlive { get; set; } = true; // Thay đổi thành public

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
        IsAlive = true;

        if (myRigidbody2D != null)
        {
            myRigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
            myRigidbody2D.simulated = true;
        }
    }

    private void OnEnable()
    {
        if (moveActionToUse != null && moveActionToUse.action != null)
        {
            moveActionToUse.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveActionToUse != null && moveActionToUse.action != null)
        {
            moveActionToUse.action.Disable();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority && IsAlive)
        {
            // Client sở hữu gửi input
            Vector2 newMoveDirection = moveActionToUse.action.ReadValue<Vector2>();
            MoveDirection = newMoveDirection;
            Flip();
        }

        if (HasStateAuthority)
        {
            // Server cập nhật vị trí
            if (IsAlive)
            {
                myRigidbody2D.velocity = MoveDirection * speed;
            }
            else
            {
                myRigidbody2D.velocity = Vector2.zero;
                myRigidbody2D.simulated = false;
            }
        }
    }

    public override void Render()
    {
        if (!IsAlive)
        {
            spriteRenderer.enabled = false; // Ẩn sprite khi chết
            return;
        }

        spriteRenderer.enabled = true;
        spriteRenderer.flipX = !IsFacingRight;
        animator.SetFloat("Speed", MoveDirection.magnitude);
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Die()
    {
        IsAlive = false;
        myRigidbody2D.simulated = false;
        myRigidbody2D.velocity = Vector2.zero;

        if (HasStateAuthority)
        {
            Debug.Log($"Player {Object.Id} died!");
        }
    }
}