using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;
using System;
using UnityEngine.UI;

public class FirebaseWebGL : MonoBehaviour
{
    public static FirebaseWebGL Instance;
    [SerializeField] private string databaseURL = "https://gam302-lab-default-rtdb.asia-southeast1.firebasedatabase.app/"; 
    [SerializeField] private TMP_Text top5Text;

    [Header("Player Info")]
    private string playerName;
    private int playerScore;

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
        UpdatePlayerInfo();
        LoadScores(); 
    }

    private void UpdatePlayerInfo()
    {
        if (LoginManager.Instance != null)
        {
            playerName = LoginManager.Instance.GetPlayerName();
            playerScore = LoginManager.Instance.GetPlayerScore();
            Debug.Log($"FirebaseWebGL → Tên: {playerName}, điểm: {playerScore}");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy LoginManager.Instance");
        }
    }

    public void SaveScore()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            UpdatePlayerInfo();
        }

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogError("FirebaseWebGL → playerName bị null hoặc rỗng khi lưu điểm!");
            return;
        }

        PlayerData data = new PlayerData(playerName, playerScore);
        string jsonData = JsonUtility.ToJson(data);

        StartCoroutine(PostData($"leaderboard/{playerName}.json", jsonData));
    }

    public void LoadScores()
    {
        StartCoroutine(GetDataFromFirebase("leaderboard.json"));
    }

    IEnumerator PostData(string path, string jsonData)
    {
        UnityWebRequest request = UnityWebRequest.Put(databaseURL + path, jsonData);
        request.method = UnityWebRequest.kHttpVerbPUT;
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("FirebaseWebGL → Dữ liệu đã được gửi thành công!");
        }
        else
        {
            Debug.LogError("FirebaseWebGL → Lỗi khi gửi dữ liệu: " + request.error);
        }
    }

    IEnumerator GetDataFromFirebase(string path)
    {
        UnityWebRequest request = UnityWebRequest.Get(databaseURL + path);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("FirebaseWebGL → Dữ liệu nhận được: " + request.downloadHandler.text);

            string json = request.downloadHandler.text;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    Dictionary<string, PlayerData> leaderboard = JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(json);
                    List<PlayerData> playerList = new List<PlayerData>(leaderboard.Values);

                    // Sắp xếp giảm dần theo điểm
                    playerList.Sort((a, b) => b.playerScore.CompareTo(a.playerScore));

                    string leaderboardText = "___TOP 5 NGƯỜI CHƠI___\n\n";
                    int count = Mathf.Min(5, playerList.Count);
                    for (int i = 0; i < count; i++)
                    {
                        leaderboardText += $"{i + 1}. {playerList[i].playerName}: {playerList[i].playerScore} điểm\n";
                    }

                    if (top5Text != null)
                    {
                        top5Text.text = leaderboardText;
                    }
                    else
                    {
                        Debug.LogWarning("FirebaseWebGL → Không tìm thấy TMP_Text để hiển thị bảng xếp hạng.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("FirebaseWebGL → Lỗi giải mã JSON: " + ex.Message);
                }
            }
            else
            {
                Debug.LogWarning("FirebaseWebGL → Dữ liệu JSON rỗng.");
            }
        }
        else
        {
            Debug.LogError("FirebaseWebGL → Lỗi khi tải dữ liệu: " + request.error);
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int playerScore;

    public PlayerData(string name, int score)
    {
        playerName = name;
        playerScore = score;
    }
}