using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    [Networked]
    public float Health { get; set; }

    [Header("UI")]
    [SerializeField] private Slider healthSlider; // Slider đồng bộ cho tất cả client
    [SerializeField] private Image playerIcon;    // Image chỉ hiển thị cho người chơi sở hữu

    public override void Spawned()
    {
        Health = maxHealth;

        // Tìm Slider và Image nếu chưa gán trong Inspector
        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
            if (healthSlider == null)
            {
                Debug.LogWarning($"Player {Object.Id}: Không tìm thấy Slider trong hierarchy!");
            }
        }

        if (playerIcon == null)
        {
            playerIcon = GetComponentInChildren<Image>();
            if (playerIcon == null)
            {
                Debug.LogWarning($"Player {Object.Id}: Không tìm thấy Image trong hierarchy!");
            }
        }

        // Thiết lập Slider (hiển thị trên tất cả client)
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = Health;
            healthSlider.gameObject.SetActive(true); // Luôn hiển thị Slider
        }

        // Thiết lập Image (chỉ hiển thị cho người chơi sở hữu)
        if (playerIcon != null)
        {
            playerIcon.gameObject.SetActive(HasInputAuthority);
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Player {Object.Id} took {damage} damage!");
        if (!HasStateAuthority) return;
        Health = Mathf.Max(Health - damage, 0);
        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (Runner.IsServer)
        {
            Debug.Log($"Player {Object.Id} died!");
        }
        Runner.Despawn(Object);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasStateAuthority) return;
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(10f);
        }
    }

    public override void Render()
    {
        // Cập nhật Slider (đồng bộ cho tất cả client)
        if (healthSlider != null)
        {
            healthSlider.value = Health;
        }

        // Cập nhật Image (chỉ cho người chơi sở hữu)
        if (playerIcon != null && HasInputAuthority)
        {
            if (Health <= maxHealth * 0.3f) // Dưới 30% máu thì đổi màu đỏ
            {
                playerIcon.color = Color.red;
            }
            else
            {
                playerIcon.color = Color.white; // Màu mặc định
            }
        }
    }
}