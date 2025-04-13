using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class ShopItem : MonoBehaviour
{
    [Header("Shop Item")]
    [SerializeField] private int id;
    [SerializeField] private string itemName;
    [SerializeField] private int price;

    [Header("UI")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private PlayerHealth playerHealth;
    private int tempQuantity = 0;

    private void Start()
    {
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager chưa được gán trong Inspector!");
            enabled = false;
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth chưa được gán trong Inspector!");
            enabled = false;
            return;
        }

        if (buyButton == null || upButton == null || downButton == null || priceText == null || quantityText == null)
        {
            Debug.LogError("Missing UI references in ShopItem.");
            enabled = false;
            return;
        }

        buyButton.onClick.AddListener(OnBuyButtonClicked);
        upButton.onClick.AddListener(OnUpButtonClicked);
        downButton.onClick.AddListener(OnDownButtonClicked);

        priceText.text = price.ToString();
        UpdateUI();

        // Đăng ký sự kiện cập nhật UI khi vàng thay đổi
        playerManager.OnGoldChanged += (_) => UpdateUI();
    }


    private void OnBuyButtonClicked()
    {
        int totalCost = tempQuantity * price;
        int playerGold = playerManager.GetPlayerGold();

        if (tempQuantity > 0 && totalCost <= playerGold)
        {
            playerManager.RPC_ReduceGold(totalCost);
            Buy();
            Debug.Log($"[Shop] Bought {tempQuantity} x {itemName} for {totalCost} gold.");
            // TODO: Add item to inventory here
            tempQuantity = 0;
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("[Shop] Cannot buy: Not enough gold or quantity is zero.");
        }
    }

    private void OnUpButtonClicked()
    {
        int playerGold = playerManager.GetPlayerGold();
        int nextTotalCost = (tempQuantity + 1) * price;

        if (nextTotalCost <= playerGold)
        {
            tempQuantity++;
            UpdateUI();
        }
        else
        {
            Debug.Log("[Shop] Not enough gold to increase quantity.");
        }
    }

    private void OnDownButtonClicked()
    {
        if (tempQuantity > 0)
        {
            tempQuantity--;
            UpdateUI();
        }
        else
        {
            Debug.Log("[Shop] Quantity cannot be less than 0.");
        }
    }

    private void UpdateUI()
    {
        int playerGold = playerManager.GetPlayerGold();
        int totalCost = tempQuantity * price;
        int nextTotalCost = (tempQuantity + 1) * price;

        quantityText.text = tempQuantity.ToString();

        buyButton.interactable = tempQuantity > 0 && totalCost <= playerGold;
        upButton.interactable = nextTotalCost <= playerGold;
        downButton.interactable = tempQuantity > 0;
    }

    private void Buy()
    {
        switch (id)
        {
            case 0:
                AddLivePlayer();
                break;
            case 1:
                break;
            case 2:
                break;
        }
    }

    public void AddLivePlayer()
    {
        playerHealth.AddLocalLives(tempQuantity);
    }
}