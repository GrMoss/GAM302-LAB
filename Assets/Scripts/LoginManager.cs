using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion;
using Fusion.Sockets;
using System;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private Button manPlayerButton;
    [SerializeField] private Button womanPlayerButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private string sceneName;
    [SerializeField] private TMP_Text debugText;
    private string playerName;
    public static int indexPlayer;
    public static bool isStart = false;
    private bool CanPlay = false;
    private void Awake()
    {
        manPlayerButton.onClick.AddListener(OnManPlayerButtonClicked);
        womanPlayerButton.onClick.AddListener(OnWomanPlayerButtonClicked);
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void Start()
    {
        playerNameInputField.onValueChanged.AddListener(OnPlayerNameChanged);
        playerNameInputField.text = "Player" + UnityEngine.Random.Range(1, 1000);
        isStart = false;
        CanPlay = false;
    }
    private void OnPlayerNameChanged(string name)
    {
        playerName = name;
        CanPlay = true;
        Debug.Log("Player name changed to: " + playerName);
    }
    private void OnManPlayerButtonClicked()
    {
        indexPlayer = 0;
        CanPlay = true;
        Debug.Log("Nam player selected");
    }
    private void OnWomanPlayerButtonClicked()
    {
        indexPlayer = 1;
        CanPlay = true;
        Debug.Log("Nữ player selected");
    }
    private void OnStartButtonClicked()
    {
        if (!CanPlay)
        {
            debugText.text = "Vui lòng chọn giới tính nhân vật!";
            Invoke("ResetDebug", 2f);
            return;
        }
        isStart = true;
        SceneManager.LoadScene(sceneName);
        Debug.Log("Game started");
    }

    public void ResetDebug()
    {
        debugText.text = "";
    }
}
