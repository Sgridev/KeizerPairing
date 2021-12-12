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

    }
}

