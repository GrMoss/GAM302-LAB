using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))] // Thêm NetworkTransform để nội suy vị trí
public class Bullet : NetworkBehaviour
{
    [Networked] 
    private Vector2 Direction { get; set; }
    [Networked] 
    private float Speed { get; set; }

    public void Initialize(Vector2 direction, float bulletSpeed)
    {
        Direction = direction;
        Speed = bulletSpeed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public override void FixedUpdateNetwork()
    {
        // Di chuyển đạn trong FixedUpdateNetwork để đồng bộ với tick mạng
        transform.position += (Vector3)(Direction * Speed * Runner.DeltaTime);
    }

    public override void Render()
    {
        // Để trống Render, NetworkTransform sẽ xử lý nội suy hiển thị
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Object.HasStateAuthority) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(20f);
            Runner.Despawn(Object);
        }
    }
}