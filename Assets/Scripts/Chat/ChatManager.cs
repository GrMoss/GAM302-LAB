using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class ChatManager : NetworkBehaviour, IChatService
{
    public static ChatManager Instance { get; private set; }
    
    private readonly List<string> messages = new List<string>();

    [SerializeField] private ChatUI chatUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcReceiveMessage(string playerName, string content)
    {
        // Nối playerName và content với thẻ màu ngay tại đây
        string formattedMessage = $"<color=#73CFF5>{playerName}</color>: <color=#FFFFFF>{content}</color>";
        messages.Add(formattedMessage);
        chatUI?.AddMessage(formattedMessage);
    }

    public new void SendMessage(string content)
    {
        string playerName = Runner.LocalPlayer.PlayerId.ToString();
        RpcReceiveMessage(playerName, content);
    }
}

public interface IChatService
{
    void SendMessage(string content);
}