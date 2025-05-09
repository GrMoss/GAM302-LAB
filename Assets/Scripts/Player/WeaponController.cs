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
    private AudioManager audioManager;
    private void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("AudioManager không tìm thấy trong scene!");
        }
    }

    private float nextFireTime;
    private PlayerController playerController;

    public override void Spawned()
    {
        playerController = GetComponent<PlayerController>();
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
        audioManager.Play("PlayerAttack");
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction, bulletSpeed, GetComponent<NetworkObject>()); 
        }
        else
        {
            Debug.LogError("Bullet prefab thiếu script Bullet!");
        }
    }
}