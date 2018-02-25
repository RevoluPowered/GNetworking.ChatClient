// LICENSE
// GNetworking, SimpleUnityClient and GameServer are property of Gordon Alexander MacPherson
// No warranty is provided with this code, and no liability shall be granted under any circumstances.
// All rights reserved GORDONITE LTD 2018 (c) Gordon Alexander MacPherson.

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
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Computer : MonoBehaviour
{
    /// <summary>
    /// The send button
    /// </summary>
    public Button SendButton;

    /// <summary>
    /// The scroll bar rect transform
    /// </summary>
    public RectTransform ScrollTransform;

    /// <summary>
    /// The scroll rect UI element - for moving the chat down vertically to keep the focus on the last message
    /// </summary>
    public ScrollRect ScrollRect;

    /// <summary>
    /// The message input field
    /// </summary>
    public TMP_InputField MessageInputField;

    /// <summary>
    /// Unity Screen
    /// </summary>
    private TextMeshProUGUI screen;

    /// <summary>
    /// The reference to the chat client
    /// </summary>
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
        // make sure the message isn't empty.
        if (MessageInputField.text.Length <= 0) return;

        // submit text to the chat manager to be sent
        TextEntered(MessageInputField.text);

        // clear message input in the text area.
        MessageInputField.text = "";

        // force re-focusing the input box so the user can keep chatting
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(MessageInputField.gameObject);
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

    /// <summary>
    /// Called when the text is submitted to the chat client
    /// </summary>
    /// <param name="message"></param>
    private void TextEntered(string message)
    {
        ChatClient.OnUserSubmit(message);
    }
    
    /// <summary>
    /// Print to the chat console
    /// </summary>
    /// <param name="text"></param>
    public void Print(string text)
    {
        // draw the text on the screen
        screen.text += text + "\n";

        RecalculateContentSize();
    }

    private void RecalculateContentSize()
    {
        // make the scroll rect bigger based on the content anyone adds
        Vector2 size = ScrollTransform.sizeDelta;
        size.y = screen.GetPreferredValues(screen.text).y + 10;
        ScrollTransform.sizeDelta = size;
        ScrollRect.normalizedPosition = new Vector2(0, 0);
    }

    public void Clear()
    {
        // clear the console
        screen.text = "";
        RecalculateContentSize();
        /*for (int x = 0; x < 20; x++)
        {
            screen.text += "number " + x + "<sprite=" + x + ">\n";
        }*/
    }
}
