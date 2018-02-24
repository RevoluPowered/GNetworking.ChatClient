using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Core.Service;
using TMPro;
using UnityEngine;
using GNetworking;
using GNetworking.Data;
using GNetworking.Messages;
using UnityEngine.UI;

public class Computer : MonoBehaviour
{
    public Button SendButton;
    public TMP_InputField MessageInputField;
    /// <summary>
    /// Unity Screen
    /// </summary>
    private TextMeshProUGUI screen;
    private ChatClient _chatClient;
    /// <summary>
    /// Unity Start
    /// </summary>
    void Start()
    {
        screen = GetComponent<TextMeshProUGUI>();
        screen.text = "";

        SendButton.onClick.AddListener(OnSendPressed);
    }

    /// <summary>
    /// Called when send is pressed
    /// </summary>
    void OnSendPressed()
    {
        if (MessageInputField.text.Length <= 0) return;
        TextEntered(MessageInputField.text);
        MessageInputField.text = "";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnSendPressed();
        }
    }


    /// <summary>
    /// Chat client service
    /// -- lazily loaded when needed
    /// </summary>
    public ChatClient ChatClient
    {
        get
        {
            if (_chatClient == null)
            {
                _chatClient = GameServiceManager.GetService<ChatClient>();
            }
            return _chatClient;
        }
    }

    private void TextEntered(string message)
    {
        ChatClient.OnUserSubmit(message);
    }



    public void Print(string text)
    {
        screen.text += text + "\n";
    }

    public void Clear()
    {
        // clear the console
        screen.text = "";

        /*for (int x = 0; x < 20; x++)
        {
            screen.text += "number " + x + "<sprite=" + x + ">\n";
        }*/
    }
}
