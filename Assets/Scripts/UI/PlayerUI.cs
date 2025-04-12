using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text playerGoldText;

    private int playerScore = LoginManager.Instance.GetPlayerScore();
    private int playerGold = LoginManager.Instance.GetPlayerGold();
    private void Start()
    {
        UpdateUI();
    }
    private void Update()
    {
        if (LoginManager.Instance.GetPlayerScore() != playerScore || LoginManager.Instance.GetPlayerGold() != playerGold)
        {
            playerScore = LoginManager.Instance.GetPlayerScore();
            playerGold = LoginManager.Instance.GetPlayerGold();
            UpdateUI();
        }
    }
    private void UpdateUI()
    {
        playerScoreText.text = "Score: " + playerScore;
        playerGoldText.text = "Gold: " + playerGold;
    }
}
