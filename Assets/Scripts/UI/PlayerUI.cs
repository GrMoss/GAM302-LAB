using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text playerGoldText;

    [SerializeField] private GameObject shopPanel;

    [SerializeField] private PlayerManager playerManager;

    private void Start()
    {
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
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("shopUI chưa được gán trong Inspector!");
        }
    }

    private void FixedUpdate()
    {
           if (Input.GetKeyDown(KeyCode.E))
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(!shopPanel.activeSelf);
            }
            else
            {
                Debug.LogError("shopUI chưa được gán trong Inspector!");
            }
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

    public void OnBoxShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }
}