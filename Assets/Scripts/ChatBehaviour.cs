using Mirror;
using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChatBehaviour : NetworkBehaviour
{
    [SerializeField] private InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Text chatContentText;
    [SyncVar(hook = nameof(OnChatMessageReceived))]
    private string chatMessage;

    public override void OnStartClient()
    {
        base.OnStartClient();
        sendButton.onClick.AddListener(OnSendButtonClicked);
        chatInputField.onEndEdit.AddListener(delegate { OnEnterPressed(); }); // Додаємо слухача до події onEndEdit
    }

    private void OnEnterPressed()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSendButtonClicked();
        }
    }

    public void OnSendButtonClicked()
    {
        if (!string.IsNullOrWhiteSpace(chatInputField.text))
        {
            CmdSendChatMessage(chatInputField.text);
            chatInputField.text = string.Empty;
            chatInputField.ActivateInputField();
        }
    }

    [ClientRpc]
    public void RpcReceiveChatMessage(ChatMessage chatMessage)
    {
        string formattedMessage = $"Player {chatMessage.SenderId}: {chatMessage.Text}";
        chatContentText.text += "\n" + formattedMessage; // Додаємо нове повідомлення до текстового поля
    }

    public struct ChatMessage
    {
        public string SenderId;
        public string Text;

        public ChatMessage(string senderId, string text)
        {
            SenderId = senderId;
            Text = text;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSendChatMessage(string message)
    {
        var senderId = this.netId;
        
        ChatMessage chatMessage = new ChatMessage(senderId.ToString(), message);
        
        RpcReceiveChatMessage(chatMessage);
    }

    public void OnChatMessageReceived(string oldMessage, string newMessage)
    {
        Debug.Log($"Chat message received: {newMessage}");
    }
}