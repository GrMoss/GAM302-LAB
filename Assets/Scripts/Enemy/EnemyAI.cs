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
    [SerializeField] private float wanderSpeed = 1.5f;
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private int scoreReward = 10;
    [SerializeField] private int goldReward = 5;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D _rigidbody2D;
    private Collider2D[] colliders;
    private Transform target;
    private Vector2 wanderTarget;
    private float lastTargetCheckTime;
    private float lastWanderTime;
    private PlayerController[] players;
    private float deathTimer;

    private enum State { Wandering, Chasing, Attacking }
    [Networked] private State CurrentState { get; set; } = State.Wandering;

    [Networked] public float Health { get; set; }
    [Networked] private bool IsFacingRight { get; set; }
    [Networked] private bool IsHit { get; set; }
    [Networked] private bool IsDead { get; set; }

    private AudioManager audioManager;
    private void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public override void Spawned()
    {
        Health = maxHealth;
        IsFacingRight = true;
        IsHit = false;
        IsDead = false;
        deathTimer = 0f;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();

        if (spriteRenderer == null) Debug.LogError("Missing SpriteRenderer on EnemyAI!", this);
        if (animator == null) Debug.LogError("Missing Animator on EnemyAI!", this);
        if (_rigidbody2D == null) Debug.LogError("Missing Rigidbody2D on EnemyAI!", this);
        if (colliders == null || colliders.Length == 0) Debug.LogError("Missing Collider2D on EnemyAI!", this);
        if (healthSlider == null) Debug.LogError("Missing Health Slider on EnemyAI!", this);

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = Health;
            healthSlider.gameObject.SetActive(true);
        }

        UpdatePlayerList();
        lastTargetCheckTime = Runner.SimulationTime;
        lastWanderTime = Runner.SimulationTime;

        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }
        _rigidbody2D.simulated = true;

        wanderTarget = GetRandomWanderPoint();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (Runner.SimulationTime - lastTargetCheckTime >= targetCheckInterval)
        {
            UpdatePlayerList(); // Cập nhật danh sách người chơi
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

    private void UpdatePlayerList()
    {
        players = FindObjectsOfType<PlayerController>();
        // Debug.Log($"[EnemyAI] Updated player list, found {players.Length} players.");
    }

    private void UpdateState()
    {
        if (IsDead) return;

        if (target != null)
        {
            PlayerHealth targetHealth = target.GetComponent<PlayerHealth>();
            if (targetHealth != null && (targetHealth.IsDead || targetHealth.IsRespawning))
            {
                target = null;
            }
        }

        if (target != null && target.gameObject.activeInHierarchy)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget <= stoppingDistance)
            {
                CurrentState = State.Attacking;
            }
            else
            {
                CurrentState = State.Chasing;
            }
        }
        else
        {
            CurrentState = State.Wandering;
            target = null;
        }
    }

    private void HandleState()
    {
        switch (CurrentState)
        {
            case State.Wandering:
                Wander();
                break;
            case State.Chasing:
                ChaseTarget();
                break;
            case State.Attacking:
                _rigidbody2D.velocity = Vector2.zero;
                break;
        }
    }

    private void Wander()
    {
        if (Runner.SimulationTime - lastWanderTime >= 2f)
        {
            wanderTarget = GetRandomWanderPoint();
            lastWanderTime = Runner.SimulationTime;
        }

        Vector2 dirVec = wanderTarget - (Vector2)transform.position;
        if (dirVec.magnitude > 0.1f)
        {
            Vector2 nextVec = dirVec.normalized * wanderSpeed * Runner.DeltaTime;
            _rigidbody2D.MovePosition(_rigidbody2D.position + nextVec);
            _rigidbody2D.velocity = Vector2.zero;
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
            Vector2 dirVec = (Vector2)target.position - _rigidbody2D.position;
            float distanceToTarget = dirVec.magnitude;

            if (distanceToTarget > stoppingDistance)
            {
                Vector2 nextVec = dirVec.normalized * speed * Runner.DeltaTime;
                _rigidbody2D.MovePosition(_rigidbody2D.position + nextVec);
                _rigidbody2D.velocity = Vector2.zero;
                IsFacingRight = target.position.x > _rigidbody2D.position.x;
            }
        }
    }

    private void FindTarget()
    {
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;

        foreach (PlayerController player in players)
        {
            if (player == null || !player.Object.IsValid || !player.gameObject.activeInHierarchy) continue;

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null || playerHealth.IsDead || playerHealth.IsRespawning) continue;

            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closestDistance && distance <= detectionRange)
            {
                closestDistance = distance;
                closestTarget = player.transform;
            }
        }
        target = closestTarget;
    }

    public void TakeDamage(float damage, NetworkObject attacker)
    {
        if (IsDead) return;

        RPC_ApplyDamage(damage, transform.position, attacker != null ? attacker.transform.position : transform.position, attacker);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ApplyDamage(float damage, Vector3 enemyPos, Vector3 targetPos, NetworkObject attacker)
    {
        if (IsDead) return;

        Health = Mathf.Max(Health - damage, 0);
        if (Health > 0)
        {
            IsHit = true;
            audioManager.Play("EnemyHit");
            RPC_KnockBack(enemyPos, targetPos);
        }
        else
        {
            RPC_Die(attacker);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Die(NetworkObject attacker)
    {
        IsDead = true;
        target = null;

        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        _rigidbody2D.simulated = false;

        if (HasStateAuthority)
        {
            Debug.Log($"Enemy {Object.Id} died!");
            if (attacker != null)
            {
                var playerManager = attacker.GetComponent<PlayerManager>();
                if (playerManager != null)
                {
                    audioManager.Play("EnemyDie");
                    playerManager.RPC_AddScore(scoreReward);
                    playerManager.RPC_AddGold(goldReward);
                    Debug.Log($"[EnemyAI] Awarded {scoreReward} score and {goldReward} gold to player {playerManager.PlayerName}");
                }
                else
                {
                    Debug.LogWarning("[EnemyAI] PlayerManager not found on attacker!");
                }
            }
            else
            {
                Debug.LogWarning("[EnemyAI] Attacker is null!");
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_KnockBack(Vector3 enemyPos, Vector3 targetPos)
    {
        if (_rigidbody2D != null && !IsDead)
        {
            Vector2 dirVec = (Vector2)enemyPos - (Vector2)targetPos;
            _rigidbody2D.AddForce(dirVec.normalized * 3f, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }
    }

    internal void TakeDamage(float damage, PlayerRef inputAuthority)
    {
        throw new System.NotImplementedException();
    }

    internal void TakeDamage(float damage)
    {
        throw new System.NotImplementedException();
    }
}