using System.Collections.Generic;

namespace KeizerPairing.Shared
{
    public class Round
    {
        public List<Player> Players { get; set; }

        public List<Pairing> Pairings { get; set; }

        public int RoundNumber { get; set; }
    }
}
