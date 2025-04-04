using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class WeaponController : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float bulletDamage = 20f;

    private float nextFireTime;
    private PlayerController playerController; // Tham chiếu đến PlayerController

    public override void Spawned()
    {
        playerController = GetComponent<PlayerController>(); // Lấy PlayerController khi spawn
        if (playerController == null)
        {
            Debug.LogError($"WeaponController trên {Object.Id} không tìm thấy PlayerController!");
        }
    }

    private void Update()
    {
        if (!HasStateAuthority) return;
        HandleShooting();
    }

    public void HandleShooting()
    {
        // Kiểm tra nếu người chơi không còn sống thì không bắn
        if (playerController != null && !playerController.IsAlive) return;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            nextFireTime = Time.time + 1f / fireRate;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 direction = (worldMousePos - (Vector2)bulletSpawnPoint.position).normalized;
            SpawnBullet(direction);
        }
    }

    private void SpawnBullet(Vector2 direction)
    {
        NetworkObject bullet = Runner.Spawn(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity, Object.InputAuthority);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction, bulletSpeed);
        }
        else
        {
            Debug.LogError("Bullet prefab thiếu script Bullet!");
        }
    }
}