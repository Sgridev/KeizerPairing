using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeizerPairing.Shared
{
    public class PlayerService
    {
            public event Action OnChange;

            public List<Player> Players { get; private set; } = new List<Player>();

            public void Add(Player player)
            {
                Players.Add(player);
                OnChange?.Invoke(); ;
            }


        
    }
}
