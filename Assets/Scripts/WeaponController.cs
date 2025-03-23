using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float bulletDamage = 20f;
    private float nextFireTime;

    private void Update()
    {
        if (!HasStateAuthority) return;
        HandleShooting();
    }

    public void HandleShooting()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
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
        bulletScript.Initialize(direction, bulletSpeed);
    }
}