using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KeizerPairing.Shared
{
    public class Player
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public int Rating { get; set; }
        public string Presence { get; set; } = "Present";
        public int Position { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int Score { get; set; }
        public int Value { get; set; }


    }
}

