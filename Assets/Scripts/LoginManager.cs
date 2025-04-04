using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance;

    [SerializeField] private Button manPlayerButton;
    [SerializeField] private Button womanPlayerButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private string gameSceneName = "GameScene"; // Thêm trường để tùy chỉnh tên scene

    private string playerName;
    public static int indexPlayer;
    public static bool isStart = false;
    public static string PlayerNameStatic { get; private set; }
    private bool canPlay = false;
    private NetworkRunner runner;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        manPlayerButton.onClick.AddListener(OnManPlayerButtonClicked);
        womanPlayerButton.onClick.AddListener(OnWomanPlayerButtonClicked);
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void Start()
    {
        playerNameInputField.onValueChanged.AddListener(OnPlayerNameChanged);
        playerNameInputField.text = "Player" + UnityEngine.Random.Range(1, 1000);
        isStart = false;
        canPlay = false;
    }

    private void OnPlayerNameChanged(string name)
    {
        playerName = name;
        PlayerNameStatic = name;
        canPlay = true;
        Debug.Log("Player name changed to: " + playerName);
    }

    private void OnManPlayerButtonClicked()
    {
        indexPlayer = 0;
        canPlay = true;
        Debug.Log("Nam player selected");
    }

    private void OnWomanPlayerButtonClicked()
    {
        indexPlayer = 1;
        canPlay = true;
        Debug.Log("Nữ player selected");
    }

    private void OnStartButtonClicked()
    {
        if (!canPlay)
        {
            debugText.text = "Vui lòng chọn giới tính nhân vật!";
            Invoke("ResetDebug", 2f);
            return;
        }

        isStart = true;
        StartGame(GameMode.Shared);
    }

    private async void StartGame(GameMode mode)
    {
        if (runner != null && runner.IsRunning)
        {
            UpdateStatus("Already connected to a session!");
            return;
        }

        runner = Instantiate(networkRunnerPrefab);
        DontDestroyOnLoad(runner.gameObject);

        string roomName = "DefaultRoom";

        var sessionProps = new Dictionary<string, SessionProperty>
        {
            { "isPublic", 1 }
        };

        var args = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            Scene = null,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            SessionProperties = sessionProps
        };

        UpdateStatus($"Starting {mode} mode, Room: {roomName}");

        var result = await runner.StartGame(args);
        if (result.Ok)
        {
            UpdateStatus($"Connected successfully, loading {gameSceneName}...");
            await LoadGameSceneAsync();
        }
        else
        {
            UpdateStatus($"Failed to start game: {result.ErrorMessage}");
            CleanupRunner();
        }
    }

    private async Task LoadGameSceneAsync()
    {
        var operation = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
        while (!operation.isDone)
        {
            UpdateStatus($"Loading {gameSceneName}: {(operation.progress * 100):F0}%");
            await Task.Yield();
        }
        UpdateStatus($"{gameSceneName} loaded!");
    }

    private void UpdateStatus(string message)
    {
        Debug.Log(message);
        if (statusText != null) statusText.text = message;
    }

    private void CleanupRunner()
    {
        if (runner != null)
        {
            Destroy(runner.gameObject);
            runner = null;
        }
    }

    public void Disconnect()
    {
        if (runner != null)
        {
            runner.Shutdown();
            CleanupRunner();
            UpdateStatus("Disconnected");
        }
    }

    private string RandomRoomName()
    {
        return "Room_" + UnityEngine.Random.Range(1000, 9999).ToString();
    }

    public void ResetDebug()
    {
        debugText.text = "";
    }

    public string GetPlayerName()
    {
        return playerName;
    }
}