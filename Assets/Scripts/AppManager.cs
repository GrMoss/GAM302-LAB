using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AppManager : MonoBehaviour
{
    public static AppManager Instance;

    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private TextMeshProUGUI statusText; 

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
        }
    }

    private void Start()
    {
        // Tự động vào phòng khi game khởi động
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

        // Tạo tên phòng ngẫu nhiên hoặc cố định (ở đây dùng "DefaultRoom")
        string roomName = "DefaultRoom"; // Có thể thay bằng RandomRoomName() nếu muốn ngẫu nhiên

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
            UpdateStatus("Connected successfully, loading GameScene...");
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
        var operation = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);
        while (!operation.isDone)
        {
            UpdateStatus($"Loading GameScene: {(operation.progress * 100):F0}%");
            await Task.Yield();
        }
        UpdateStatus("GameScene loaded!");
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

    // Hàm tạo tên phòng ngẫu nhiên (nếu cần)
    private string RandomRoomName()
    {
        return "Room_" + Random.Range(1000, 9999).ToString();
    }
}