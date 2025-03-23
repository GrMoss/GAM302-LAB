using Fusion;
using UnityEngine;

public class EnemyAI : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float detectionRange = 10f; // Phạm vi phát hiện người chơi
    [SerializeField] private float moveSpeed = 3f; // Tốc độ di chuyển
    [SerializeField] private float stoppingDistance = 2f; // Khoảng cách dừng lại trước người chơi

    private SpriteRenderer spriteRenderer;
    private Transform target; // Mục tiêu là người chơi gần nhất

    [Networked] 
    public float Health { get; set; }
    [Networked] 
    private bool IsFacingRight { get; set; }
    [Networked] 
    private Vector2 MoveDirection { get; set; } // Hướng di chuyển đồng bộ qua mạng

    public override void Spawned()
    {
        Health = maxHealth;
        IsFacingRight = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            FindTarget();
            MoveToTarget();
        }

        // Di chuyển kẻ địch dựa trên MoveDirection
        transform.position += (Vector3)(MoveDirection * moveSpeed * Runner.DeltaTime);
    }

    public override void Render()
    {
        spriteRenderer.flipX = !IsFacingRight; // Đồng bộ lật sprite trên tất cả client
    }

    private void FindTarget()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;

        foreach (PlayerController player in players)
        {
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
        if (target != null)
        {
            Vector2 targetPos = target.position;
            float distanceToTarget = Vector2.Distance(transform.position, targetPos);

            // Nếu chưa đến khoảng cách dừng, di chuyển về phía người chơi
            if (distanceToTarget > stoppingDistance)
            {
                MoveDirection = (targetPos - (Vector2)transform.position).normalized;
            }
            else
            {
                MoveDirection = Vector2.zero; // Dừng lại khi đủ gần
            }

            // Cập nhật hướng lật dựa trên hướng di chuyển
            if (MoveDirection.x > 0 && !IsFacingRight)
            {
                IsFacingRight = true;
            }
            else if (MoveDirection.x < 0 && IsFacingRight)
            {
                IsFacingRight = false;
            }
        }
        else
        {
            MoveDirection = Vector2.zero; // Không có mục tiêu thì đứng yên
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