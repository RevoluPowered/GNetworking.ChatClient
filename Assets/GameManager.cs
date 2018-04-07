// LICENSE
// GNetworking, SimpleUnityClient and GameServer are property of Gordon Alexander MacPherson
// No warranty is provided with this code, and no liability shall be granted under any circumstances.
// All rights reserved GORDONITE LTD 2018 (c) Gordon Alexander MacPherson.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GNetworking;
using Core.Service;
using GNetworking.Data;
using GNetworking.Managers;
using GNetworking.Messages;
using Lidgren.Network;
using Serilog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    /// <summary>
    /// Game Manager using services
    /// - Makes a game very easy to test and to compartmentalise.
    /// - Uses service locator pattern GetService<T>
    /// - Easy to use
    /// - Compartmentalised game state
    /// - Lighter than using a gameobject
    /// - Doesn't require DoNotDestroy on load for each management service
    /// - Simplifies game framework
    /// - No required singleton uses to allow your services or objects to communicate.
    /// - Helps minimize round referencing 
    /// - Services are only Start() able and Stop() able, nothing complex.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public GameObject ChannelList;
        public GameObject ChannelPrefab;
        public TextMeshProUGUI UserList;

        /// <summary>
        /// Unity Awake
        /// </summary>
        void Awake()
        {
        }    

        public void JoinLocalGame()
        {
            GameServiceManager.RegisterService(new ConfigHandler());
            GameServiceManager.RegisterService(new NetworkClient());
            GameServiceManager.RegisterService(new ChatClient(this, ChannelList, ChannelPrefab, UserList));
            GameServiceManager.StartServices();
        }

        public void HostGame()
        {
            try
            {
                GameServiceManager.RegisterService(new ConfigHandler());
                // create server socket handler
                GameServiceManager.RegisterService(new NetworkServer(27015, 20));
                // create chat system handler
                GameServiceManager.RegisterService(new ServerChatManager());

                GameServiceManager.RegisterService(new NetworkClient());
                GameServiceManager.RegisterService(new ChatClient(this, ChannelList, ChannelPrefab, UserList));
                GameServiceManager.StartServices();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }

        }
        
        
        /// <summary>
        /// Unity Fixed Update
        /// </summary>
        void FixedUpdate()
        {
            // Update all game services
            GameServiceManager.UpdateServices();
        }

        /// <summary>
        /// Unity Application Quit
        /// </summary>
        void OnApplicationQuit()
        {
            // Stop all game services and shutdown gracefully.
            GameServiceManager.StopServices();
        }

        /// <summary>
        /// Unity UI quit button
        /// </summary>
        public void QuitButton()
        {
            Application.Quit();
        }
    }
}