using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float respawnDelay = 3f;

    [Networked] public float Health { get; set; }
    [Networked] public bool IsDead { get; set; }
    [Networked] public bool IsRespawning { get; set; }

    [Header("UI")]
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Image playerIcon;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text livesText;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private int localLives;
    private Slider healthSlider;
    private PlayerController playerController;

    public override void Spawned()
    {
        Health = maxHealth;
        IsDead = false;
        IsRespawning = false;
        localLives = maxLives;

        playerController = GetComponent<PlayerController>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"Player {Object.Id}: Không tìm thấy Animator!");
            }
        }

        if (animator != null)
        {
            animator.SetBool("Dead", false);
        }

        if (healthBar == null)
        {
            Debug.LogWarning($"Player {Object.Id}: Không tìm thấy healthBar!");
        }
        else
        {
            healthSlider = healthBar.GetComponentInChildren<Slider>();
            if (healthSlider == null)
            {
                Debug.LogWarning($"Player {Object.Id}: Không tìm thấy Slider trong healthBar!");
            }
            else
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = Health;
            }
            healthBar.SetActive(true);
        }

        if (playerIcon == null)
        {
            playerIcon = GetComponentInChildren<Image>();
            if (playerIcon == null)
            {
                Debug.LogWarning($"Player {Object.Id}: Không tìm thấy Image trong hierarchy!");
            }
        }

        if (playerIcon != null)
        {
            playerIcon.gameObject.SetActive(HasInputAuthority);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (HasInputAuthority && livesText != null)
        {
            livesText.text = localLives.ToString();
            livesText.gameObject.SetActive(true);
        }
        else if (livesText == null)
        {
            Debug.LogWarning($"Player {Object.Id}: Không tìm thấy livesText!");
        }
    }

    public void TakeDamage(float damage)
    {
        if (!HasStateAuthority) return;

        Debug.Log($"Player {Object.Id} took {damage} damage!");
        Health = Mathf.Max(Health - damage, 0);

        if (Health <= 0 && !IsRespawning)
        {
            Rpc_UpdateLifeAndRespawn();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_UpdateLifeAndRespawn()
    {
        localLives--;

        if (HasStateAuthority)
        {
            if (localLives > 0)
            {
                Rpc_StartRespawn();
                Debug.Log($"Player {Object.Id} lost a life, {localLives} lives remaining, respawning...");
            }
            else
            {
                IsDead = true;
                Rpc_NotifyGameOver();
                Invoke(nameof(Die), 0.5f);
                Debug.Log($"Player {Object.Id} has no lives left!");
            }
        }

        if (HasInputAuthority && livesText != null)
        {
            livesText.text = localLives.ToString();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_NotifyGameOver()
    {
        // Logic từ code mẫu
        if (HasInputAuthority)
        {
            if (losePanel != null)
            {
                losePanel.SetActive(true);
                Debug.Log($"Player {Object.Id} lost the game!");
            }
        }
        else
        {
            if (winPanel != null)
            {
                winPanel.SetActive(true);
                Debug.Log($"Player {Object.Id} wins because another player died!");
            }
        }

        if (animator != null)
        {
            animator.SetBool("Dead", true);
        }

        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }

        if (HasInputAuthority && livesText != null)
        {
            livesText.text = localLives.ToString();
        }

        if (HasStateAuthority && playerController != null)
        {
            playerController.RPC_Die();
        }

        // Giữ Rpc_NotifyPlayersOfWin như cũ
        if (HasStateAuthority)
        {
            Rpc_NotifyPlayersOfWin(Object.Id);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_NotifyPlayersOfWin(NetworkId deadPlayerId)
    {
        if (!IsDead && HasInputAuthority)
        {
            Debug.Log($"Player {Object.Id} nhận thông báo: Player {deadPlayerId} đã chết, bạn còn sống!");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_StartRespawn()
    {
        IsRespawning = true;
        Health = 0;

        if (animator != null)
        {
            animator.SetBool("Dead", true);
        }

        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }

        if (HasStateAuthority && playerController != null)
        {
            playerController.RPC_Die();
        }

        if (HasStateAuthority)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
    }

    private void Respawn()
    {
        if (HasStateAuthority)
        {
            Health = maxHealth;
            IsRespawning = false;
            Rpc_CompleteRespawn();
            Debug.Log($"Player {Object.Id} respawned with {Health} health!");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_CompleteRespawn()
    {
        if (animator != null)
        {
            animator.SetBool("Dead", false);
        }

        if (healthBar != null)
        {
            healthBar.SetActive(true);
            if (healthSlider != null)
            {
                healthSlider.value = Health;
            }
        }

        if (HasInputAuthority && livesText != null)
        {
            livesText.text = localLives.ToString();
        }

        if (HasStateAuthority && playerController != null)
        {
            playerController.RPC_Respawn();
        }
    }

    private void Die()
    {
        if (Runner.IsServer)
        {
            Debug.Log($"Player {Object.Id} died and is despawned!");
            Runner.Despawn(Object);
        }
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
        if (healthSlider != null && !IsRespawning && !IsDead)
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