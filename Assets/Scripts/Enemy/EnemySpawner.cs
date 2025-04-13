using Fusion;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef enemyPrefab; // Prefab của enemy
    [SerializeField] private float spawnInterval = 2f;    // Thời gian giữa các lần spawn (giây)
    [SerializeField] private float spawnRadius = 5f;      // Bán kính spawn quanh vị trí spawner
    [SerializeField] private Transform spawnCenter;       // Điểm trung tâm để spawn
    [Networked] private float Timer { get; set; }         // Đồng bộ thời gian spawn qua mạng

    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        Timer = 0f;
        if (spawnCenter == null)
        {
            spawnCenter = transform;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        Timer += Runner.DeltaTime;

        if (Timer >= spawnInterval)
        {
            Timer = 0f;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = spawnCenter.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        NetworkObject enemy = Runner.Spawn(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnCenter != null ? spawnCenter.position : transform.position, spawnRadius);
    }
}