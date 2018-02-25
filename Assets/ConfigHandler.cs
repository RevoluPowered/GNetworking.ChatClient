// LICENSE
// GNetworking, SimpleUnityClient and GameServer are property of Gordon Alexander MacPherson
// No warantee is provided with this code, and no liability shall be granted under any circumstances.
// All rights reserved GORDONITE LTD 2018 � Gordon Alexander MacPherson.

using System.IO;
using Core.Service;
using Newtonsoft.Json;
using Serilog;

namespace Assets
{
    public class ConfigHandler : GameService
    {
        private readonly string _configFile;
        private GameConfiguration _gameConfiguration = null;

        public ConfigHandler() : base("config handler")
        {
            _configFile = Path.GetFullPath(".") + "/config.json";
        }

        /// <summary>
        /// Return the game configuration
        /// </summary>
        /// <returns></returns>
        public GameConfiguration GetConfiguration()
        {
            return _gameConfiguration;
        }

        public override void Start()
        {
            if (File.Exists(_configFile))
            {
                Log.Information("Read existing configuration file");
                var file = File.ReadAllText(_configFile);
                _gameConfiguration = JsonConvert.DeserializeObject<GameConfiguration>(file);
            }
            else
            {
                Log.Information("Written new configuration file.");
                _gameConfiguration = new GameConfiguration();

                File.WriteAllText(
                    _configFile, 
                    JsonConvert.SerializeObject(_gameConfiguration)
                );
            }
        }

        public override void Stop()
        {
            // cannot be stopped
        }

        public override void Update()
        {
            // no need to do anything here
        }
    }
}
