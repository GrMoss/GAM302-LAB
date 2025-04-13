using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;

public class FirebaseWebGL : MonoBehaviour
{
    public static FirebaseWebGL Instance;
    [SerializeField] private string databaseURL = "https://gam302-lab-default-rtdb.asia-southeast1.firebasedatabase.app/";
    [SerializeField] private TMP_Text top5Text;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[FirebaseWebGL] Đã tồn tại một FirebaseWebGL khác! Hủy instance này.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[FirebaseWebGL] Awake: FirebaseWebGL được khởi tạo");
    }

    private void Start()
    {
        LoadScores();
        Debug.Log("[FirebaseWebGL] Start: Gọi LoadScores");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Debug.Log("[FirebaseWebGL] OnDestroy: FirebaseWebGL bị hủy");
    }

    public void SaveScore(PlayerManager playerManager)
    {
        Debug.Log($"[FirebaseWebGL] SaveScore được gọi, playerManager={(playerManager != null ? "OK" : "null")}");

        if (playerManager == null)
        {
            Debug.LogError("[FirebaseWebGL] SaveScore thất bại: PlayerManager là null!");
            return;
        }

        string playerName = playerManager.PlayerName;
        int playerScore = playerManager.GetPlayerScore();

        Debug.Log($"[FirebaseWebGL] Chuẩn bị lưu điểm: playerName={playerName}, playerScore={playerScore}");

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogError("[FirebaseWebGL] SaveScore thất bại: playerName rỗng!");
            return;
        }

        PlayerData data = new PlayerData(playerName, playerScore);
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"[FirebaseWebGL] JSON dữ liệu: {jsonData}");
        StartCoroutine(PostData($"leaderboard/{playerName}.json", jsonData));
    }

    public void LoadScores()
    {
        StartCoroutine(GetDataFromFirebase("leaderboard.json"));
    }

    IEnumerator PostData(string path, string jsonData)
    {
        Debug.Log($"[FirebaseWebGL] PostData: Gửi yêu cầu PUT tới {databaseURL + path}");
        UnityWebRequest request = UnityWebRequest.Put(databaseURL + path, jsonData);
        request.method = UnityWebRequest.kHttpVerbPUT;
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[FirebaseWebGL] Dữ liệu đã được gửi thành công!");
        }
        else
        {
            Debug.LogError($"[FirebaseWebGL] Lỗi khi gửi dữ liệu: {request.error}, ResponseCode={request.responseCode}, URL={databaseURL + path}");
        }
    }

    IEnumerator GetDataFromFirebase(string path)
    {
        Debug.Log($"[FirebaseWebGL] GetDataFromFirebase: Gửi yêu cầu GET tới {databaseURL + path}");
        UnityWebRequest request = UnityWebRequest.Get(databaseURL + path);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[FirebaseWebGL] Dữ liệu nhận được: {request.downloadHandler.text}");

            string json = request.downloadHandler.text;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    Dictionary<string, PlayerData> leaderboard = JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(json);
                    List<PlayerData> playerList = new List<PlayerData>(leaderboard.Values);

                    playerList.Sort((a, b) => b.playerScore.CompareTo(a.playerScore));

                    string leaderboardText = "___TOP 10 NGƯỜI CHƠI___\n\n";
                    int count = Mathf.Min(10, playerList.Count);
                    for (int i = 0; i < count; i++)
                    {
                        leaderboardText += $"{i + 1}. {playerList[i].playerName}: {playerList[i].playerScore} điểm\n";
                    }

                    if (top5Text != null)
                    {
                        top5Text.text = leaderboardText;
                        Debug.Log("[FirebaseWebGL] Cập nhật top5Text với bảng xếp hạng");
                    }
                    else
                    {
                        Debug.LogWarning("[FirebaseWebGL] Không tìm thấy TMP_Text để hiển thị bảng xếp hạng.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FirebaseWebGL] Lỗi giải mã JSON: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[FirebaseWebGL] Dữ liệu JSON rỗng.");
            }
        }
        else
        {
            Debug.LogError($"[FirebaseWebGL] Lỗi khi tải dữ liệu: {request.error}, ResponseCode={request.responseCode}");
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