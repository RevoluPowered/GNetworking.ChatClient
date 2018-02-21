using System.Linq;
using System.Net;
using Core.Service;
using GNetworking;
using GNetworking.Data;
using GNetworking.Messages;
using Lidgren.Network;
using Serilog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class ChatClient : GameService
    {
        public ChatChannel CurrentChannel = null;
        private Computer _chat_terminal = null;
        private GameObject _channelList;
        private GameObject _channelPrefab;

        public ChatClient( GameObject channelList, GameObject channelPrefab ) : base("Chat Client")
        {
            this._channelList = channelList;
            this._channelPrefab = channelPrefab;
        }

        public override void Start()
        {
            _chat_terminal = Object.FindObjectOfType<Computer>();
            var client = GameServiceManager.GetService<NetworkClient>();

            client.OnClientConnectionSuccessful += OnConnected;
            client.OnClientDisconnected += OnDisconnected;

            // register network data function for the message type
            client.MessagePipe.On("say", SayMessageReceived);
            client.MessagePipe.On("UserInfo", SetUserInformation);
            // connect locally
            client.Connect(IPAddress.Loopback, 27015);
        }


        public override void Stop()
        {
            _chat_terminal = null;
            CurrentChannel = null;
            _channelPrefab = null;
            _channelList = null;
        }

        

        public bool SayMessageReceived(string name, NetConnection sender, NetPipeMessage msg)
        {
            // Retrieve the message
            var chatMessage = msg.GetMessage<Message>();

            // this can be null if someone is sending data which is erroneous or they're sending the wrong arguments
            if (chatMessage == null) return false;

            // message is for other channel
            if (chatMessage.ChannelName != CurrentChannel.Name) return true;

            _chat_terminal.Print(chatMessage.User.Nickname + ": " + chatMessage.Text);

            Log.Information("Recieved message:" + chatMessage.Text);
            Debug.Log("Nickname said: " + chatMessage.Text);
            return true;
        }

        public bool SetUserInformation(string name, NetConnection sender, NetPipeMessage msg)
        {
            var userInfo = msg.GetMessage<UserInfoMessage>();

            if (userInfo == null) return false;

            _chat_terminal.Print("My nickname is: " + userInfo.UserData.Nickname);

            // clear list of channels
            foreach (var transform in _channelList.GetComponentsInChildren<Transform>(true))
            {
                if (_channelList.transform == transform) continue;
                if (transform.gameObject != null)
                {
                    Object.Destroy(transform);
                }
            }

            // Default first channel as the selected channel.
            CurrentChannel = userInfo.ChatChannels.First();

            // populate channel list
            foreach (var channel in userInfo.ChatChannels)
            {
                var go = Object.Instantiate(_channelPrefab, _channelList.transform, false);
                var button = go.GetComponent<Button>();
                var uiChannelLink = go.AddComponent<ChannelUILink>();
                uiChannelLink.ChatChannel = channel;

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                text.text = channel.Name;

                // event handler assignment -> with data pre-attached on call.
                button.onClick.AddListener(delegate { OnChannelClicked(channel); });
            }
            return true;
        }


        void OnChannelClicked(ChatChannel Channel)
        {
            Debug.Log("Channel Changed");
            CurrentChannel = Channel;

            // Clear Console
            _chat_terminal.Clear();

            // Output all the messages which have been sent to this channel
            foreach (var message in Channel.Messages)
            {
                _chat_terminal.Print(message.User.Nickname + ": " + message.Text);
            }
        }

        void OnConnected()
        {
            Log.Information("Connection started");
            _chat_terminal.Print("Connected to server.");
        }

        void OnDisconnected()
        {
            Log.Information("Connection disconnected");
            _chat_terminal.Print("Disconnected from the server.");
        }

        public override void Update()
        {

        }
    }

}