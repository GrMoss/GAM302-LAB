using Fusion;
using UnityEngine;

public class EnemyAI : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float targetCheckInterval = 0.5f;
    [SerializeField] private float deathDelay = 1f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private new Rigidbody2D rigidbody2D;
    private new Collider2D collider2D;
    private Transform target;
    private float lastTargetCheckTime;
    private PlayerController[] players;
    private bool isLive;
    private float deathTimer;

    [Networked] public float Health { get; set; }
    [Networked] private bool IsFacingRight { get; set; }
    [Networked] private bool IsHit { get; set; }
    [Networked] private bool IsDead { get; set; }

    public override void Spawned()
    {
        Health = maxHealth;
        IsFacingRight = true;
        isLive = true;
        IsHit = false;
        IsDead = false;
        deathTimer = 0f;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();

        if (spriteRenderer == null) Debug.LogError("Missing SpriteRenderer on EnemyAI!", this);
        if (animator == null) Debug.LogError("Missing Animator on EnemyAI!", this);
        if (rigidbody2D == null) Debug.LogError("Missing Rigidbody2D on EnemyAI!", this);
        if (collider2D == null) Debug.LogError("Missing Collider2D on EnemyAI!", this);

        players = FindObjectsOfType<PlayerController>();
        lastTargetCheckTime = Runner.SimulationTime;

        collider2D.enabled = true;
        rigidbody2D.simulated = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !isLive) return;

        if (Runner.SimulationTime - lastTargetCheckTime >= targetCheckInterval)
        {
            FindTarget();
            lastTargetCheckTime = Runner.SimulationTime;
        }

        MoveToTarget();

        if (IsDead)
        {
            deathTimer += Runner.DeltaTime;
            if (deathTimer >= deathDelay)
            {
                Runner.Despawn(Object);
            }
        }
    }

    public override void Render()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !IsFacingRight;
        }
        if (animator != null)
        {
            animator.SetBool("Dead", IsDead);
            if (IsHit)
            {
                animator.SetTrigger("Hit");
                IsHit = false;
            }
        }
    }

    private void FindTarget()
    {
        if (players == null || players.Length == 0)
        {
            players = FindObjectsOfType<PlayerController>();
        }

        float closestDistance = float.MaxValue;
        Transform closestTarget = null;

        foreach (PlayerController player in players)
        {
            if (player == null || !player.Object.IsValid) continue;
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closestDistance && distance <= detectionRange)
            {
                closestDistance = distance;
                closestTarget = player.transform;
            }
        }
        target = closestTarget;
    }

    private void MoveToTarget()
    {
        if (target != null && target.gameObject.activeInHierarchy && isLive)
        {
            if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Hit")) return;

            Vector2 dirVec = (Vector2)target.position - rigidbody2D.position; // Ép kiểu Vector3 sang Vector2
            float distanceToTarget = dirVec.magnitude;

            if (distanceToTarget > stoppingDistance)
            {
                Vector2 nextVec = dirVec.normalized * speed * Runner.DeltaTime;
                rigidbody2D.MovePosition(rigidbody2D.position + nextVec);
                rigidbody2D.velocity = Vector2.zero;
            }

            IsFacingRight = target.position.x > rigidbody2D.position.x;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!HasStateAuthority || !isLive) return;

        Health = Mathf.Max(Health - damage, 0);
        if (Health > 0)
        {
            IsHit = true;
            RPC_KnockBack(transform.position, target != null ? target.position : transform.position);
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        if (!HasStateAuthority) return;

        isLive = false;
        IsDead = true;
        collider2D.enabled = false;
        rigidbody2D.simulated = false;

        if (Runner.IsServer)
        {
            Debug.Log($"Enemy {Object.Id} died!");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_KnockBack(Vector3 enemyPos, Vector3 targetPos)
    {
        if (rigidbody2D != null)
        {
            Vector2 dirVec = (Vector2)enemyPos - (Vector2)targetPos;
            rigidbody2D.AddForce(dirVec.normalized * 3f, ForceMode2D.Impulse);
        }
    }
}