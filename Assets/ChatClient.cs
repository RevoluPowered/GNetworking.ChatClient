// LICENSE
// GNetworking, SimpleUnityClient and GameServer are property of Gordon Alexander MacPherson
// No warranty is provided with this code, and no liability shall be granted under any circumstances.
// All rights reserved GORDONITE LTD 2018 (c) Gordon Alexander MacPherson.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.Service;
using GNetworking;
using GNetworking.Data;
using GNetworking.Messages;
using Lidgren.Network;
using Serilog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace Assets
{
    public class ChatClient : GameService
    {
        public ChatChannel CurrentChannel = null;
        public User LocalUser;

        private Computer _chatTerminal = null;
        private NetworkClient _networkClient;
        private GameObject _channelList;
        private GameObject _channelPrefab;
        private List<ChatChannel> _chatChannels = new List<ChatChannel>();
        private TextMeshProUGUI _userListText;
        private GameManager _manager;
        private ConfigHandler _configHandler;
        private EmoticonHandler _emoticonHandler;
        private BadWordHandler _badWordHandler;
        
        public ChatClient( GameManager manager, GameObject channelList, GameObject channelPrefab, TextMeshProUGUI userListText ) : base("Chat Client")
        {
            this._channelList = channelList;
            this._channelPrefab = channelPrefab;
            this._userListText = userListText;
            this._manager = manager;
        }

        public override void Start()
        {
            _badWordHandler = new BadWordHandler();
            _emoticonHandler = new EmoticonHandler();
            _chatTerminal = Object.FindObjectOfType<Computer>();
            _networkClient = GameServiceManager.GetService<NetworkClient>();
            _configHandler = GameServiceManager.GetService<ConfigHandler>();
            _networkClient.OnClientConnectionSuccessful += OnConnected;
            _networkClient.OnClientDisconnected += OnDisconnected;

            // register network data function for the message type
            _networkClient.MessagePipe.On("say", SayMessageReceived);
            _networkClient.MessagePipe.On("UserInfo", SetUserInformation);
            _networkClient.MessagePipe.On("ChannelUpdate", OnChannelUpdate);
            _networkClient.MessagePipe.On("OnServerNotification", OnServerNotification);

            // Connect
            Connect();
        }

        /// <summary>
        /// Connect to server
        /// </summary>
        private void Connect()
        {
            var config = _configHandler.GetConfiguration();
            _networkClient.Connect(IPAddress.Parse(config.ServerAddress), config.ServerPort);
        }

        /// <summary>
        /// The server address
        /// </summary>
        private IPAddress serverAddress = IPAddress.Loopback;

        private bool reconnecting = false;
        /// <summary>
        /// Attempt reconnection
        /// </summary>
        public IEnumerator AttemptReconnection()
        {
            if (reconnecting) yield break;
            reconnecting = true; // start reconnecting

            var retryCount = 10;
            while (_networkClient.NetworkSocket.ConnectionStatus != NetConnectionStatus.Connected && retryCount > 0)
            {
                _chatTerminal.Print("Retrying connection to server... retries left: " + retryCount);
                Connect();

                yield return new WaitForSeconds(3.0f);

                if (_networkClient.NetworkSocket.ConnectionStatus != NetConnectionStatus.Connected)
                {
                    retryCount--;
                }
                else
                {
                    reconnecting = false;
                }
            }

            reconnecting = false;
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


                if (commands.Length >= 1)
                {
                    if (commands[0] == "/help")
                    {
                        _chatTerminal.Print(
                            "help\tThe following commands are available:</b>\n" +
                            "\tnote: you can have as many groups as you want\n" +
                            "\tyou can change group with the buttons at the top\n" +
                            "<b>/group FEDS</b> \t\t Make a new group to invite private members to your group.\n" +
                            "<b>/invite bobmarley</b> \t Invite a user to a group chat\n" +
                            "<b>/nickname bobmarley</b> \t Change your nickname to bobmarley\n");
                    }
                }

                if (commands.Length >= 2)
                {
                    var parameters = string.Join(" ", commands.Skip(1));
                    if (commands[0] == "/nickname")
                    {
                        //_chatTerminal.Print("New nickname: " + parameters);
                        _networkClient.MessagePipe.SendReliable("request-nickname-change", new User(parameters));
                    }
                    else if (commands[0] == "/group")
                    {
                        //_chatTerminal.Print("group requested: " + parameters);
                        _networkClient.MessagePipe.SendReliable("request-new-group", new ChatChannel
                        {
                            Name = parameters
                        });
                    }
                    else if (commands[0] == "/invite")
                    {
                        //_chatTerminal.Print("group requested: " + parameters);
                        _networkClient.MessagePipe.SendReliable("request-invite-user", new UserChannelInvite
                        {
                            Nickname = parameters,
                            ChannelName = CurrentChannel.Name
                        });
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

        /// <summary>
        /// On Nickname Response
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool OnServerNotification(string name, NetConnection sender, NetPipeMessage msg)
        {
            var message = msg.GetMessage<Message>();

            if (message != null)
            {
                _chatTerminal.Print("[server] " + message.Text);
            }

            return true;
        }


        /// <summary>
        /// Find ChatChannel by name
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
        private bool SayMessageReceived(string name, NetConnection sender, NetPipeMessage msg)
        {
            // Retrieve the message
            var chatMessage = msg.GetMessage<Message>();

            // this can be null if someone is sending data which is erroneous or they're sending the wrong arguments
            if (chatMessage == null) return false;

            // filter text - bad words, smiley conversion
            chatMessage.Text = FilterHandler(chatMessage.Text);

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
        /// Called when a string needs filtered based on the current config settings
        /// E.G. Bad word filtering, emoticon code conversion
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string FilterHandler( string input )
        {
            // filter emoticons into text properly
            input = _emoticonHandler.ConvertString(input);

            // filter _badWords out if this enabled
            if (_configHandler.GetConfiguration().FilterBadWords)
            {
                input = _badWordHandler.Filter(input);
            }

            return input;
        }

        /// <summary>
        /// Filter channel, filters entire channel data when requested
        /// </summary>
        /// <param name="channel"></param>
        private void FilterChannel(ChatChannel channel)
        {
            // order list properly by time sent noteworthy that this value is trusted by the client, so that needs improved.
            // people might game the history potentially, therefore the client.log is the most valid representation of 
            // chat history past 10 messages.
            channel.Messages = channel.Messages.OrderBy(m => m.Timestamp.TimeOfDay).ToList();
            
            // maximum data points retrieved should be 10
            channel.Messages = channel.Messages.Skip(Math.Max(0, channel.Messages.Count() - 10)).ToList();

            foreach (var message in channel.Messages)
            {
                message.Text = FilterHandler(message.Text);
            }
        }

        /// <summary>
        /// OnChannelUpdate Network Handler
        /// Called when something changes in a channel, this is only executed on the clients.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool OnChannelUpdate(string name, NetConnection sender, NetPipeMessage msg)
        {
            var channel = msg.GetMessage<ChatChannel>();

            if (channel == null) return true; // error

            // check if channel already exists
            var localChannel = GetChannelByName(channel.Name);

            if (localChannel != null)
            {
                localChannel.Participants = channel.Participants;

                // only update visual list if that channel is selected.
                if (CurrentChannel == localChannel)
                {
                    OnChannelUserUpdate(localChannel);
                }
            }
            else
            {
                // add channel to list, participants came from server so it is trustworthy.
                _chatChannels.Add(channel);

                // update the channel list
                UpdateChannelList(_chatChannels);
            }

            return false;
        }


        /// <summary>
        /// Set User Information - Set's the local user information and the initial nickname the user has.
        /// This is essentially only done when you connect to the server
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool SetUserInformation(string name, NetConnection sender, NetPipeMessage msg)
        {
            var userInfo = msg.GetMessage<UserInfoMessage>();

            if (userInfo == null) return false;

            // Default first channel as the selected channel.
            CurrentChannel = userInfo.ChatChannels.First();
            _chatChannels = userInfo.ChatChannels;

            // update the channel list
            UpdateChannelList(_chatChannels);

            // assign current user as default
            LocalUser = userInfo.UserData;

            // set default to be current channel and load chat history
            OnChannelSet(CurrentChannel);

            _chatTerminal.Print("My nickname is: " + userInfo.UserData.Nickname + ", to change it type /nickname yournickname also /help shows you a list of commands.");


            return true;
        }

        /// <summary>
        /// Update the channel list
        /// </summary>
        /// <param name="channels">list of the channels</param>
        void UpdateChannelList( List<ChatChannel> channels )
        {
            Debug.Log("Init channel..." + channels.Count);
            // clear the existing list
            foreach (Transform trans in _channelList.transform)
            {
                GameObject.Destroy(trans.gameObject);
            }

            // populate channel list
            foreach (var channel in channels)
            {
                Debug.Log("Adding channel...");
                var go = Object.Instantiate(_channelPrefab, _channelList.transform, false);
                var button = go.GetComponent<Button>();
                var uiChannelLink = go.AddComponent<ChannelUILink>();
                uiChannelLink.ChatChannel = channel;

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                text.text = channel.Name;

                // event handler assignment -> with data pre-attached on call.
                button.onClick.AddListener(delegate { OnChannelSet(channel); });
            }
        }


        void OnChannelSet(ChatChannel chatChannel)
        {
            Debug.Log("Channel Changed");
            CurrentChannel = chatChannel;

            // Clear Console
            _chatTerminal.Clear();

            // filter current channel
            FilterChannel(CurrentChannel);

            // Output all the messages which have been sent to this channel
            foreach (var message in chatChannel.Messages)
            {
                _chatTerminal.Print(message.User.Nickname + ": " + message.Text);
            }

            // update online user list
            OnChannelUserUpdate(chatChannel);
        }

        void OnChannelUserUpdate(ChatChannel channel)
        {
            // update online users list
            var stringBuilder = new StringBuilder();
            foreach (var participant in CurrentChannel.Participants)
            {
                stringBuilder.Append(participant.Nickname + "\n");
            }

            // update ui text
            _userListText.text = stringBuilder.ToString();
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
            _manager.StartCoroutine(AttemptReconnection());
        }


        public override void Update()
        {

        }
    }

}