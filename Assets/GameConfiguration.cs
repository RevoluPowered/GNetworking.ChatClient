// LICENSE
// GNetworking, SimpleUnityClient and GameServer are property of Gordon Alexander MacPherson
// No warantee is provided with this code, and no liability shall be granted under any circumstances.
// All rights reserved GORDONITE LTD 2018 � Gordon Alexander MacPherson.

namespace Assets
{
    public class GameConfiguration
    {
        public string ServerAddress { get; set; } = "127.0.0.1";
        public ushort ServerPort { get; set; } = 27015;
        public bool FilterBadWords = false;
    }
}
