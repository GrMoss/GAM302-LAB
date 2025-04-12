
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
                Debug.LogWarning($"[PlayerHealth] Player {Object.Id}: Không tìm thấy Animator!");
            }
        }

        if (animator != null)
        {
            animator.SetBool("Dead", false);
        }

        if (healthBar == null)
        {
            Debug.LogWarning($"[PlayerHealth] Player {Object.Id}: Không tìm thấy healthBar!");
        }
        else
        {
            healthSlider = healthBar.GetComponentInChildren<Slider>();
            if (healthSlider == null)
            {
                Debug.LogWarning($"[PlayerHealth] Player {Object.Id}: Không tìm thấy Slider trong healthBar!");
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
                Debug.LogWarning($"[PlayerHealth] Player {Object.Id}: Không tìm thấy Image trong hierarchy!");
            }
        }

        if (playerIcon != null)
        {
            playerIcon.gameObject.SetActive(HasInputAuthority);
        }

        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        if (HasInputAuthority && livesText != null)
        {
            livesText.text = localLives.ToString();
            livesText.gameObject.SetActive(true);
        }
        else if (livesText == null)
        {
            Debug.LogWarning($"[PlayerHealth] Player {Object.Id}: Không tìm thấy livesText!");
        }
    }

    public void TakeDamage(float damage)
    {
        if (!HasStateAuthority) return;

        Debug.Log($"[PlayerHealth] Player {Object.Id} nhận {damage} sát thương!");
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
                Debug.Log($"[PlayerHealth] Player {Object.Id} mất một mạng, còn {localLives} mạng, đang hồi sinh...");
            }
            else
            {
                IsDead = true;
                Rpc_NotifyGameOver();
                Invoke(nameof(Die), 0.5f);
                Debug.Log($"[PlayerHealth] Player {Object.Id} hết mạng!");
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
        if (HasInputAuthority)
        {
            if (losePanel != null)
            {
                losePanel.SetActive(true);
                Debug.Log($"[PlayerHealth] Player {Object.Id} thua!");
            }

            if (HasStateAuthority && FirebaseWebGL.Instance != null)
            {
                FirebaseWebGL.Instance.SaveScore();
                Debug.Log($"[PlayerHealth] Player {Object.Id} thua, lưu điểm: {LoginManager.Instance.GetPlayerScore()}");
            }
        }
        else
        {
            if (winPanel != null)
            {
                winPanel.SetActive(true);
                Debug.Log($"[PlayerHealth] Player {Object.Id} thắng vì người chơi khác đã chết!");
            }

            if (HasStateAuthority && FirebaseWebGL.Instance != null)
            {
                FirebaseWebGL.Instance.SaveScore();
                Debug.Log($"[PlayerHealth] Player {Object.Id} thắng, lưu điểm: {LoginManager.Instance.GetPlayerScore()}");
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
            Debug.Log($"[PlayerHealth] Player {Object.Id} nhận thông báo: Player {deadPlayerId} đã chết, bạn còn sống!");
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
            Debug.Log($"[PlayerHealth] Player {Object.Id} hồi sinh với {Health} máu!");
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
            Debug.Log($"[PlayerHealth] Player {Object.Id} đã chết và được despawn!");
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
            playerIcon.color = Health <= maxHealth * 0.3f ? Color.red : Color.white;
        }
    }
}