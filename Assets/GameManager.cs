using System.Collections;
using System.Collections.Generic;
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
    public class GameManager : MonoBehaviour
    {
        private Computer Computer = null;
        private UserInfoMessage UserInfo = null;

        public GameObject ChannelListGameObject;
        public GameObject ChannelListItem;

        void Awake()
        {
            Computer = FindObjectOfType<Computer>();
        }

        public bool SayMessageReceived(string name, NetConnection sender, NetPipeMessage msg)
        {
            // Retrieve the message
            var chatMessage = msg.GetMessage<Message>();
          
            // this can be null if someone is sending data which is erroneous or they're sending the wrong arguments
            if (chatMessage == null) return false;
           
            Computer.Print(chatMessage.User.Nickname + ": " + chatMessage.Text);

            Log.Information("Recieved message:" + chatMessage.Text);
            Debug.Log("Nickname said: " + chatMessage.Text);
            return true;
        }

        public bool SetUserInformation(string name, NetConnection sender, NetPipeMessage msg)
        {
            var userInfo = msg.GetMessage<UserInfoMessage>();

            if (userInfo == null) return false;
            this.UserInfo = userInfo;

            Computer.Print("My nickname is: " + userInfo.UserData.Nickname);

            // clear list of channels
            foreach (var transform in ChannelListGameObject.GetComponentsInChildren<Transform>(true))
            {
                if (ChannelListGameObject.transform == transform) continue;
                if (transform.gameObject != null)
                {
                    Destroy(transform);
                }
            }

            // repopulate channel list
            foreach (var channel in userInfo.AssignedChannels)
            {
                var go = Instantiate(ChannelListItem, ChannelListGameObject.transform, false);
                var button = go.GetComponent<Button>();
                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                text.text = channel.Name;
                button.onClick.AddListener(() =>
                {
                    // get channel
                    Debug.Log("Channel data requested");
                });
            }
            return true;
        }

        void Start()
        {
            var client = GameServiceManager.RegisterService(new NetworkClient());
            GameServiceManager.StartServices();

            client.OnClientConnectionSuccessful += OnConnected;
            client.OnClientDisconnected += OnDisconnected;

            // register network data function for the message type
            client.MessagePipe.On("say", SayMessageReceived);
            client.MessagePipe.On("UserInfo", SetUserInformation);
            // connect locally
            client.Connect(IPAddress.Loopback, 27015);
        }

        void OnConnected()
        {
            Log.Information("Connection started");
            Computer.Print("Connected to server.");
        }

        void OnDisconnected()
        {
            Log.Information("Connection disconnected");
            Computer.Print("Disconnected from the server.");
        }

        void OnApplicationQuit()
        {
            Computer = null;
            GameServiceManager.StopServices();
        }

        void FixedUpdate()
        {
            GameServiceManager.UpdateServices();
        }
    }
}