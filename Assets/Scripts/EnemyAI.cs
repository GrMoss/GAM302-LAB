using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float targetCheckInterval = 0.5f;
    [SerializeField] private float deathDelay = 1f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float wanderSpeed = 1.5f; // Tốc độ đi vòng vòng
    [SerializeField] private float wanderRadius = 5f;  // Bán kính đi vòng vòng
    [SerializeField] private Slider healthSlider;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private new Rigidbody2D rigidbody2D;
    private new Collider2D collider2D;
    private Transform target;
    private Vector2 wanderTarget;
    private float lastTargetCheckTime;
    private float lastWanderTime;
    private PlayerController[] players;
    private bool isLive;
    private float deathTimer;

    private enum State { Wandering, Chasing, Attacking }
    private State currentState = State.Wandering;

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
        if (healthSlider == null) Debug.LogError("Missing Health Slider on EnemyAI!", this);

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = Health;
            healthSlider.gameObject.SetActive(true);
        }

        players = FindObjectsOfType<PlayerController>();
        lastTargetCheckTime = Runner.SimulationTime;
        lastWanderTime = Runner.SimulationTime;

        collider2D.enabled = true;
        rigidbody2D.simulated = true;

        // Khởi tạo vị trí đi vòng vòng ban đầu
        wanderTarget = GetRandomWanderPoint();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !isLive) return;

        if (Runner.SimulationTime - lastTargetCheckTime >= targetCheckInterval)
        {
            FindTarget();
            lastTargetCheckTime = Runner.SimulationTime;
        }

        UpdateState();
        HandleState();

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
        if (healthSlider != null)
        {
            if (IsDead)
            {
                healthSlider.gameObject.SetActive(false);
            }
            else
            {
                healthSlider.gameObject.SetActive(true);
                healthSlider.value = Health;
            }
        }
    }

    private void UpdateState()
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget <= stoppingDistance)
            {
                currentState = State.Attacking;
            }
            else
            {
                currentState = State.Chasing;
            }
        }
        else
        {
            currentState = State.Wandering;
            target = null; // Reset target nếu không còn mục tiêu
        }
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case State.Wandering:
                Wander();
                break;
            case State.Chasing:
                ChaseTarget();
                break;
            case State.Attacking:
                // Không di chuyển khi ở trạng thái tấn công (gần mục tiêu)
                rigidbody2D.velocity = Vector2.zero;
                break;
        }
    }

    private void Wander()
    {
        if (Runner.SimulationTime - lastWanderTime >= 2f) // Thay đổi điểm đến mỗi 2 giây
        {
            wanderTarget = GetRandomWanderPoint();
            lastWanderTime = Runner.SimulationTime;
        }

        Vector2 dirVec = wanderTarget - (Vector2)transform.position;
        if (dirVec.magnitude > 0.1f)
        {
            Vector2 nextVec = dirVec.normalized * wanderSpeed * Runner.DeltaTime;
            rigidbody2D.MovePosition(rigidbody2D.position + nextVec);
            rigidbody2D.velocity = Vector2.zero;
            IsFacingRight = nextVec.x > 0;
        }
    }

    private Vector2 GetRandomWanderPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        return (Vector2)transform.position + randomOffset;
    }

    private void ChaseTarget()
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            Vector2 dirVec = (Vector2)target.position - rigidbody2D.position;
            float distanceToTarget = dirVec.magnitude;

            if (distanceToTarget > stoppingDistance)
            {
                Vector2 nextVec = dirVec.normalized * speed * Runner.DeltaTime;
                rigidbody2D.MovePosition(rigidbody2D.position + nextVec);
                rigidbody2D.velocity = Vector2.zero;
                IsFacingRight = target.position.x > rigidbody2D.position.x;
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
            if (player == null || !player.Object.IsValid || !player.gameObject.activeInHierarchy) continue;
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closestDistance && distance <= detectionRange)
            {
                closestDistance = distance;
                closestTarget = player.transform;
            }
        }
        target = closestTarget;
    }

    public void TakeDamage(float damage)
    {
        if (!isLive) return;

        RPC_ApplyDamage(damage, transform.position, target != null ? target.position : transform.position);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ApplyDamage(float damage, Vector3 enemyPos, Vector3 targetPos)
    {
        if (!isLive) return;

        Health = Mathf.Max(Health - damage, 0);
        if (Health > 0)
        {
            IsHit = true;
            RPC_KnockBack(enemyPos, targetPos);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLive) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }
    }
}