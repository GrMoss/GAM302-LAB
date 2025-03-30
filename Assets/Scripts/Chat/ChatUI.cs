using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform chatContainer;           // Container for messages
    [SerializeField] private GameObject messagePrefab;         // Prefab chung cho tin nhắn
    [SerializeField] private int maxMessages = 50;             // Giới hạn tin nhắn

    private IChatService chatService;
    private readonly List<GameObject> messageList = new List<GameObject>();

    private void Awake()
    {
        chatService = ChatManager.Instance;
        if (messagePrefab == null)
        {
            Debug.LogError("messagePrefab chưa được gán trong Inspector!");
        }
    }

    private void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(Send);
        }
        else
        {
            Debug.LogError("sendButton chưa được gán trong Inspector!");
        }
    }

    private void Update()
    {
        if (inputField != null && Input.GetKeyDown(KeyCode.Return))
        {
            Send();
        }
    }

    private void Send()
    {
        if (inputField == null)
        {
            Debug.LogError("inputField chưa được gán trong Inspector!");
            return;
        }

        string content = inputField.text.Trim();
        Debug.Log("Nội dung người dùng nhập: " + content);
        if (!string.IsNullOrEmpty(content))
        {
            chatService.SendMessage(content);
            inputField.text = "";
        }
    }

    public void AddMessage(string formattedMessage)
    {
        if (chatContainer == null || messagePrefab == null)
        {
            Debug.LogError("chatContainer hoặc messagePrefab chưa được gán!");
            return;
        }

        Debug.Log("Tin nhắn nhận được: " + formattedMessage);

        // Spawn prefab chung cho toàn bộ tin nhắn
        GameObject newMessage = Instantiate(messagePrefab, chatContainer);
        TMP_Text textComponent = newMessage.GetComponentInChildren<TMP_Text>();
        
        if (textComponent != null)
        {
            textComponent.richText = true;           // Bật rich text để hỗ trợ thẻ màu
            textComponent.enableVertexGradient = false; // Tắt gradient để tránh xung đột
            textComponent.color = Color.white;       // Màu mặc định trắng, để thẻ <color> quyết định
            textComponent.text = formattedMessage;   // Gán chuỗi đã định dạng
            Debug.Log("Đã gán nội dung cho TMP_Text: " + textComponent.text);
        }
        else
        {
            Debug.LogError("Không tìm thấy TMP_Text trong messagePrefab hoặc các thành phần con!");
        }

        messageList.Add(newMessage);

        // Xóa tin nhắn cũ nếu vượt quá giới hạn
        if (messageList.Count > maxMessages)
        {
            GameObject oldMessage = messageList[0];
            messageList.RemoveAt(0);
            if (oldMessage != null)
            {
                Destroy(oldMessage);
            }
        }
    }
}