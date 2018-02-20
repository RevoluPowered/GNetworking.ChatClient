using System;
using System.Collections.Generic;
using System.Linq;
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
    private int _startingCharacter = 0;
    private string _inputString = "";
    private const string StartingString = "<color=green>message: </color> ";
    private Dictionary<string, Func<string[], bool>> _commandAction = new Dictionary<string, Func<string[], bool>>();
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
                Say(_inputString);
                
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

    private void Say(string message)
    {
        var client = GameServiceManager.GetService<NetworkClient>();
        client.MessagePipe.SendReliable("say", new Message
        {
            Text = message
            // user will be ignored by server so you can't fake being another user
        });
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

    private bool HandleCommand(string[] argumentStrings)
    {
        if (argumentStrings.Length <= 0) return false;
        if (_commandAction.ContainsKey(argumentStrings[0]) == false) return false;

        var consoleArguments = argumentStrings.Skip(1).ToArray();



        return _commandAction[argumentStrings[0]](consoleArguments);
    }
}
