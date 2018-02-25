// LICENSE
// GNetworking, SimpleUnityClient and GameServer are property of Gordon Alexander MacPherson
// No warranty is provided with this code, and no liability shall be granted under any circumstances.
// All rights reserved GORDONITE LTD 2018 (c) Gordon Alexander MacPherson.

using System.Collections;
using System.Collections.Generic;
using Serilog;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Core;
using Core.Service;
using Serilog.Events;



public class SeriLogWindow : EditorWindow
{
	[MenuItem("Window/Debugging Tool")]
	static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(SeriLogWindow));
	}
	//bool show_debugging = false;
	//bool setting = false;
    void OnGUI()
    {
		GUILayout.Label("Serilog Debugging Settings", EditorStyles.boldLabel);

		if(!Application.isPlaying)
			GUILayout.Label("You can't change this unless the game is running!");

		GUI.enabled = Application.isPlaying;
		EditorGUILayout.BeginHorizontal();
        {
            var service = GameServiceManager.GetService<Logging>();
			if( GUILayout.Button("Debug") )
			{
			    service.SetLoggingLevel(LogEventLevel.Debug);
			}
			if( GUILayout.Button("Error") )
			{
			    service.SetLoggingLevel(LogEventLevel.Error);
			}
			if( GUILayout.Button("Fatal") )
			{
			    service.SetLoggingLevel(LogEventLevel.Fatal);
			}
		
			if( GUILayout.Button("Info") )
			{
			    service.SetLoggingLevel(LogEventLevel.Information);
			}

			if(GUILayout.Button("Verbose"))
			{
			    service.SetLoggingLevel(LogEventLevel.Verbose);
			}
		}
		EditorGUILayout.EndHorizontal();
    }
}

#endif // UNITY_EDITOR