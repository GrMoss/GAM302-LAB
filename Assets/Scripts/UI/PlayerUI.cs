using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text playerGoldText;

    private PlayerManager playerManager;

    private void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            playerManager.OnScoreChanged += UpdateScoreUI;
            playerManager.OnGoldChanged += UpdateGoldUI;
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("[PlayerUI] Không tìm thấy PlayerManager!");
        }
    }

    private void OnDestroy()
    {
        if (playerManager != null)
        {
            playerManager.OnScoreChanged -= UpdateScoreUI;
            playerManager.OnGoldChanged -= UpdateGoldUI;
        }
    }

    private void UpdateScoreUI(int score)
    {
        playerScoreText.text = score.ToString();
    }

    private void UpdateGoldUI(int gold)
    {
        playerGoldText.text = gold.ToString();
    }

    private void UpdateUI()
    {
        UpdateScoreUI(playerManager.GetPlayerScore());
        UpdateGoldUI(playerManager.GetPlayerGold());
    }
}