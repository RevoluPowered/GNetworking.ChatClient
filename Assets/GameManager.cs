using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GNetworking;
using Core.Service;
using GNetworking.Data;
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

        void Awake()
        {
            GameServiceManager.RegisterService(new NetworkClient());
            GameServiceManager.RegisterService(new ChatClient(ChannelList, ChannelPrefab, UserList));
            GameServiceManager.StartServices();
        }

        void FixedUpdate()
        {
            GameServiceManager.UpdateServices();
        }

        void OnApplicationQuit()
        {
            GameServiceManager.StopServices();
        }
    }
}