namespace Assets
{
    public class GameConfiguration
    {
        public string ServerAddress { get; set; } = "127.0.0.1";
        public ushort ServerPort { get; set; } = 27015;
        public bool FilterBadWords = false;
    }
}
