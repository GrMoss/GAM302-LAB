using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    [Networked]
    public float Health { get; set; }

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image playerIcon;  

    public override void Spawned()
    {
        Health = maxHealth;
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

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = Health;
            healthSlider.gameObject.SetActive(true); 
        }

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
        if (healthSlider != null)
        {
            healthSlider.value = Health;
        }

        if (playerIcon != null && HasInputAuthority)
        {
            if (Health <= maxHealth * 0.3f) 
            {
                playerIcon.color = Color.red;
            }
            else
            {
                playerIcon.color = Color.white; 
            }
        }
    }
}