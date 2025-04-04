using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform chatContainer;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private int maxMessages = 50;

    private IChatService chatService;
    private readonly List<GameObject> messageList = new List<GameObject>();

    private void Awake()
    {
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
        // Cố gắng lấy ChatManager.Instance nếu chưa có
        if (chatService == null)
        {
            chatService = ChatManager.Instance;
            if (chatService == null && Time.time > 1f) // Đợi 1 giây để tránh log liên tục
            {
                Debug.LogWarning("ChatManager.Instance vẫn chưa sẵn sàng!");
            }
        }

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
            if (chatService == null)
            {
                chatService = ChatManager.Instance; // Thử lại lần cuối
                if (chatService == null)
                {
                    Debug.LogError("Không thể gửi tin nhắn: ChatManager.Instance chưa sẵn sàng!");
                    return;
                }
            }
            chatService.SendMessage(content);
            inputField.text = "";
            inputField.ActivateInputField();
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

        GameObject newMessage = Instantiate(messagePrefab, chatContainer);
        TMP_Text textComponent = newMessage.GetComponentInChildren<TMP_Text>();
        
        if (textComponent != null)
        {
            textComponent.richText = true;
            textComponent.enableVertexGradient = false;
            textComponent.color = Color.white;
            textComponent.text = formattedMessage;
            Debug.Log("Đã gán nội dung cho TMP_Text: " + textComponent.text);
        }
        else
        {
            Debug.LogError("Không tìm thấy TMP_Text trong messagePrefab hoặc các thành phần con!");
        }

        messageList.Add(newMessage);

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