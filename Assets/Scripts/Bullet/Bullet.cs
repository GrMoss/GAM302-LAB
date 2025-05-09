using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [Networked]
    private Vector2 Direction { get; set; }
    [Networked]
    private float Speed { get; set; }
    [Networked]
    private float SpawnTime { get; set; }
    [Networked]
    private NetworkObject Shooter { get; set; } // Lưu người bắn

    [SerializeField]
    private float lifetime = 5f;
    [SerializeField]
    private float damage = 20f;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            SpawnTime = Runner.SimulationTime;
        }
    }

    public void Initialize(Vector2 direction, float bulletSpeed, NetworkObject shooter)
    {
        Direction = direction;
        Speed = bulletSpeed;
        Shooter = shooter;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += (Vector3)(Direction * Speed * Runner.DeltaTime);
        if (HasStateAuthority)
        {
            float timeSinceSpawn = Runner.SimulationTime - SpawnTime;
            if (timeSinceSpawn >= lifetime)
            {
                Runner.Despawn(Object);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasStateAuthority) return;
        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, Shooter);
            }
            Runner.Despawn(Object);
        }
    }
}