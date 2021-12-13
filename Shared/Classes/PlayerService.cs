using System;
using System.Collections.Generic;
using System.Linq;

namespace KeizerPairing.Shared
{
    public class PlayerService
    {
        public event Action OnChange;

        private List<Round> Rounds = new List<Round>();
        public List<Player> CurrentPlayers { get; set; } = new List<Player>();

        public List<Pairing> CurrentPairings { get; set; } = new List<Pairing>();

        public int CurrentRoundNumber { get; set; } = 1;

        public void Add(Player player)
        {
            CurrentPlayers.Add(player);
            UpdatePairings();
            UpdateRankings();
        }


        public void Update()
        {
            UpdatePairings();
            UpdateRankings();
        }
        private void UpdatePairings()
        {
            CurrentPairings.Clear();
            if (CurrentRoundNumber == 1)
            {
                var playersByRating = CurrentPlayers.Where(x => x.Presence == "Present").OrderByDescending(x => x.Rating).ToArray();
                for (int i = 0; i < playersByRating.Count() - 1; i += 2)
                {
                    if (playersByRating[i + 1] != null)
                    {
                        Pairing pairing = new Pairing() { White = playersByRating[i], Black = playersByRating[i + 1] };
                        CurrentPairings.Add(pairing);
                    }
                }
            }
        }
        public void UpdateRankings()
        {
            if (CurrentRoundNumber == 1)
            {
                int counter = 30;
                int position = 1;

                foreach (var player in CurrentPlayers.OrderByDescending(x => x.Rating))
                {
                    player.Value = counter + CurrentPlayers.Count;
                    player.Score = player.Value;
                    player.Wins = 0;
                    player.Losses = 0;
                    player.Draws = 0;
                    counter--;
                }

                if (CurrentPairings.Where(x => x.Result != null && x.Result != string.Empty).Count() > 0)
                {

                    foreach (var player in CurrentPlayers)
                    {
                        Console.WriteLine(CurrentPairings.Where(x => (x.White.Number == player.Number))?.FirstOrDefault()?.White.Name);
                        var playerPairing = CurrentPairings.Where(x => (x.White.Number == player.Number || x.Black.Number == player.Number) && x.Result != null && x.Result != string.Empty).FirstOrDefault();
                        if (playerPairing != null)
                        {
                            if (playerPairing.White.Number == player.Number)
                            {
                                if (playerPairing.Result == "1/2 - 1/2")
                                {
                                    player.Score = player.Value + (playerPairing.Black.Value / 2);
                                    player.Draws += 1;
                                }
                                else if (playerPairing.Result == "1 - 0")
                                {
                                    player.Score = player.Value + playerPairing.Black.Value;
                                    player.Wins += 1;

                                }
                                else
                                    player.Losses += 1;
                            }
                            else
                            {
                                if (playerPairing.Result == "1/2 - 1/2")
                                {
                                    player.Score = player.Value + (playerPairing.White.Value / 2);
                                    player.Draws += 1;

                                }
                                else if (playerPairing.Result == "0 - 1")
                                {
                                    player.Score = player.Value + playerPairing.White.Value;
                                    player.Wins += 1;
                                }
                                else
                                    player.Losses += 1;

                            }

                        }
                    }
                }

                counter = 30;
                foreach (var player in CurrentPlayers.OrderByDescending(x => x.Score))
                {
                    player.Value = counter + CurrentPlayers.Count;
                    counter--;
                    player.Position = position++;
                }
            }
            OnChange?.Invoke();

        }

        public void LoadDataFromStorage(PlayerService playerService)
        {
            if (playerService != null)
            {
                Rounds = playerService.Rounds;
                CurrentPlayers = playerService.CurrentPlayers;
                CurrentPairings = playerService.CurrentPairings;
                CurrentRoundNumber = playerService.CurrentRoundNumber;
            }
        }

    }
}
