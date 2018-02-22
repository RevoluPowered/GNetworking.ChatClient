using System.Collections.Generic;
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
        private Computer _chatTerminal = null;
        private NetworkClient _networkClient;
        private GameObject _channelList;
        private GameObject _channelPrefab;
        private List<ChatChannel> _chatChannels = new List<ChatChannel>();

        public ChatClient( GameObject channelList, GameObject channelPrefab ) : base("Chat Client")
        {
            this._channelList = channelList;
            this._channelPrefab = channelPrefab;
        }

        public override void Start()
        {
            _chatTerminal = Object.FindObjectOfType<Computer>();
            _networkClient = GameServiceManager.GetService<NetworkClient>();

            _networkClient.OnClientConnectionSuccessful += OnConnected;
            _networkClient.OnClientDisconnected += OnDisconnected;

            // register network data function for the message type
            _networkClient.MessagePipe.On("say", SayMessageReceived);
            _networkClient.MessagePipe.On("UserInfo", SetUserInformation);
            _networkClient.MessagePipe.On("response-nickname-change", OnNicknameChangedResponse);
            // connect locally
            _networkClient.Connect(IPAddress.Loopback, 27015);
        }


        public override void Stop()
        {
            _chatTerminal = null;
            CurrentChannel = null;
            _channelPrefab = null;
            _channelList = null;
        }


        /// <summary>
        /// Called when the user inputs a string into the chat box
        /// </summary>
        /// <param name="messageSubmitted"></param>
        public void OnUserSubmit(string messageSubmitted)
        {
            bool command = messageSubmitted.StartsWith("/");

            if (command)
            {
                string[] commands = messageSubmitted.Split(' ');

                if (commands.Length >= 2)
                {
                    var parameters = string.Join(" ", commands.Skip(1));
                    if (commands[0] == "/nickname")
                    {
                        _chatTerminal.Print("New nickname: " + parameters);
                        _networkClient.MessagePipe.SendReliable("request-nickname-change", new User(parameters));
                    }
                }
            }
            else
            {
                _networkClient.MessagePipe.SendReliable("say", new Message
                {
                    Text = messageSubmitted,
                    ChannelName = CurrentChannel.Name
                    // user field will be ignored by server so you can't fake being another user
                });
            }
        }

        public bool OnNicknameChangedResponse(string name, NetConnection sender, NetPipeMessage msg)
        {
            var message = msg.GetMessage<User>();

            if (message != null)
            {
                _chatTerminal.Print("My nickname is now: " + message.Nickname);
            }

            return true;
        }


        /// <summary>
        /// Get channel by name
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public ChatChannel GetChannelByName( string channelName )
        {
            return _chatChannels.Find(c => c.Name == channelName);
        }

        /// <summary>
        /// Say Message Handler - When server tells us someone has sent a message.
        /// </summary>
        /// <param name="name">the message name</param>
        /// <param name="sender">the sender information (server)</param>
        /// <param name="msg">the message which was sent across the network</param>
        /// <returns></returns>
        public bool SayMessageReceived(string name, NetConnection sender, NetPipeMessage msg)
        {
            // Retrieve the message
            var chatMessage = msg.GetMessage<Message>();

            // this can be null if someone is sending data which is erroneous or they're sending the wrong arguments
            if (chatMessage == null) return false;

            // message is for other channel
            if (chatMessage.ChannelName == CurrentChannel.Name)
            {
                _chatTerminal.Print(chatMessage.User.Nickname + ": " + chatMessage.Text);
            }

            // get channel by name
            var sourceChannel = GetChannelByName(chatMessage.ChannelName);
            sourceChannel.Messages.Add(chatMessage);

            Log.Information("Recieved message:" + chatMessage.Text);
            Debug.Log("Nickname said: " + chatMessage.Text);
            return true;
        }

        /// <summary>
        /// Set User Information - Set's the local user information and the initial nickname the user has.
        /// This is essentially only done when you connect to the server
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SetUserInformation(string name, NetConnection sender, NetPipeMessage msg)
        {
            var userInfo = msg.GetMessage<UserInfoMessage>();

            if (userInfo == null) return false;
           
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
            _chatChannels = userInfo.ChatChannels;
            
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
                button.onClick.AddListener(delegate { OnChannelSet(channel); });
            }

            // set default to be current channel and load chat history
            OnChannelSet(CurrentChannel);

            _chatTerminal.Print("My nickname is: " + userInfo.UserData.Nickname + ", to change it type /nickname yournickname");


            return true;
        }


        void OnChannelSet(ChatChannel Channel)
        {
            Debug.Log("Channel Changed");
            CurrentChannel = Channel;

            // Clear Console
            _chatTerminal.Clear();

            // Output all the messages which have been sent to this channel
            foreach (var message in Channel.Messages)
            {
                _chatTerminal.Print(message.User.Nickname + ": " + message.Text);
            }
        }

        void OnConnected()
        {
            Log.Information("Connection started");
            _chatTerminal.Print("Connected to server.");
        }

        void OnDisconnected()
        {
            Log.Information("Connection disconnected");
            _chatTerminal.Print("Disconnected from the server.");
        }

        public override void Update()
        {

        }
    }

}