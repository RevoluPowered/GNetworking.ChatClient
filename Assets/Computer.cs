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

/// <summary>
/// Computer 
/// Revision: 1 
/// </summary>
public class Computer : MonoBehaviour
{
    /// <summary>
    /// Unity Screen
    /// </summary>
    private TextMeshProUGUI screen;

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

    protected ChatClient _chatClient;
    

    /// <summary>
    /// Unity Start
    /// </summary>
    void Start()
    {
        screen = GetComponent<TextMeshProUGUI>();
        screen.text = "";
    }

    /// <summary>
    /// Backspace available
    /// </summary>
    private int _backspaceAvailable = 0;
    private string _inputString = "";
    // Update is called once per frame
    void Update()
    {
        foreach (var c in Input.inputString)
        {
            // has backspace/delete been pressed?
            if (c == '\b')
            {
                if (screen.text.Length == 0 || _backspaceAvailable <= 0) continue;

                screen.text = screen.text.Substring(0, screen.text.Length - 1);
                _inputString = _inputString.Substring(0, _inputString.Length - 1);
                _backspaceAvailable -= 1;
            }
            else if (c == '\n' || c == '\r') // enter/return
            {
                //Print("\n"+ _inputString);
                Print("");
                TextEntered(_inputString);
                
                _backspaceAvailable = 0;
                _inputString = "";

            }
            else
            {
                _inputString += c;
                screen.text += c;
                _backspaceAvailable += 1;
            }
        }
    }

    private void TextEntered(string message)
    {
        ChatClient.OnUserSubmit(message);
    }

    private string[] GetLines()
    {
        return screen.text.Split('\n');
    }

    public void Print(string text)
    {
        var lines = GetLines();
        var length = lines.Length;

        if (length >= 13)
        {
            // take line on top of screen out
            var newText = string.Join("\n", lines.Skip(1));
            screen.text = newText;
        }


        Debug.Log("line count: " + length);
        screen.text += text + "\n";
    }

    public void Clear()
    {
        // clear the console
        screen.text = "";
    }
}
