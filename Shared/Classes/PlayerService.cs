using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace KeizerPairing.Shared
{
    public class PlayerService
    {
        public event Action OnChange;

        private List<Round> Rounds = new List<Round>();
        public List<Player> CurrentPlayers { get; set; } = new List<Player>();

        public List<Pairing> CurrentPairings { get; set; } = new List<Pairing>();

        public int CurrentRoundNumber { get; set; } = 1;

        public bool NextRoundDisabled { get; set; }

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
            var playersByRating = CurrentPlayers.Where(x => x.Presence == "Present").OrderByDescending(x => x.Value).ToArray();
            if (CurrentRoundNumber == 1)
            {
                playersByRating = CurrentPlayers.Where(x => x.Presence == "Present").OrderByDescending(x => x.Rating).ToArray();
            }
            for (int i = 0; i < playersByRating.Count() - 1; i += 2)
            {
                if (playersByRating[i + 1] != null)
                {
                    Pairing pairing = new Pairing() { White = playersByRating[i], Black = playersByRating[i + 1] };
                    CurrentPairings.Add(pairing);
                }
            }
        }
        //gli score si valutano con i value del round prima
        //nel primo round si calcola con l'elo
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

                if (CurrentPairings.Where(x => x.Result != null && x.Result != string.Empty).Any())
                {

                    foreach (var player in CurrentPlayers)
                    {
                        var opponentPlayer = CurrentPairings.Where(x => (x.White.Number == player.Number || x.Black.Number == player.Number) && x.Result != null && x.Result != string.Empty).FirstOrDefault();
                        if (opponentPlayer != null)
                        {
                            if (opponentPlayer.White.Number == player.Number)
                            {
                                if (opponentPlayer.Result == "1/2 - 1/2")
                                {
                                    player.Score = player.Value + (opponentPlayer.Black.Value / 2);
                                    player.Draws += 1;
                                }
                                else if (opponentPlayer.Result == "1 - 0")
                                {
                                    player.Score = player.Value + opponentPlayer.Black.Value;
                                    player.Wins += 1;

                                }
                                else
                                    player.Losses += 1;
                            }
                            else
                            {
                                if (opponentPlayer.Result == "1/2 - 1/2")
                                {
                                    player.Score = player.Value + (opponentPlayer.White.Value / 2);
                                    player.Draws += 1;

                                }
                                else if (opponentPlayer.Result == "0 - 1")
                                {
                                    player.Score = player.Value + opponentPlayer.White.Value;
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
            else
            {
                if (CurrentPairings.Where(x => x.Result != null && x.Result != string.Empty).Any())
                {
                    foreach (var player in CurrentPlayers)
                    {
                        var currentPairing = CurrentPairings.Where(x => (x.White.Number == player.Number || x.Black.Number == player.Number) && x.Result != null && x.Result != string.Empty).FirstOrDefault();
                        if (currentPairing != null)
                        {
                            var lastRoundplayer = Rounds[CurrentRoundNumber - 2].Players.Where(x => x.Number == player.Number).FirstOrDefault();

                            if (currentPairing.White.Number == player.Number)
                            {
                                int opponentLastRoundValue = Rounds[CurrentRoundNumber - 2].Players.Where(x => x.Number == currentPairing.Black.Number).FirstOrDefault().Value;
                                if (currentPairing.Result == "1/2 - 1/2")
                                {
                                    player.Score = lastRoundplayer.Score + (opponentLastRoundValue / 2);
                                    player.Draws = lastRoundplayer.Draws + 1;
                                    player.Wins = lastRoundplayer.Wins;
                                    player.Losses = lastRoundplayer.Losses;

                                }
                                else if (currentPairing.Result == "1 - 0")
                                {
                                    player.Score = lastRoundplayer.Score + opponentLastRoundValue;
                                    player.Wins = lastRoundplayer.Wins + 1;
                                    player.Losses = lastRoundplayer.Losses;
                                    player.Draws = lastRoundplayer.Draws;

                                }
                                else
                                {
                                    player.Losses = lastRoundplayer.Losses + 1;
                                    player.Wins = lastRoundplayer.Wins;
                                    player.Draws = lastRoundplayer.Draws;

                                }
                            }
                            else
                            {
                                int opponentLastRoundValue = Rounds[CurrentRoundNumber - 2].Players.Where(x => x.Number == currentPairing.White.Number).FirstOrDefault().Value;
                                if (currentPairing.Result == "1/2 - 1/2")
                                {
                                    player.Score = lastRoundplayer.Score + (opponentLastRoundValue / 2);
                                    player.Draws = lastRoundplayer.Draws + 1;
                                    player.Losses = lastRoundplayer.Losses;
                                    player.Draws = lastRoundplayer.Draws;

                                }
                                else if (currentPairing.Result == "0 - 1")
                                {
                                    player.Score = lastRoundplayer.Score + opponentLastRoundValue;
                                    player.Wins = lastRoundplayer.Wins + 1;
                                    player.Losses = lastRoundplayer.Losses;
                                    player.Draws = lastRoundplayer.Draws;
                                }
                                else
                                {
                                    player.Losses = lastRoundplayer.Losses + 1;
                                    player.Wins = lastRoundplayer.Wins;
                                    player.Draws = lastRoundplayer.Draws;
                                }

                            }

                        }
                    }
                }
            }
            NextRoundDisabled = CurrentPairings.Any(x => x.Result == null || x.Result == string.Empty);

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
                NextRoundDisabled = playerService.NextRoundDisabled;
            }
        }

        public void NextRound()
        {

            Round round = new Round();
            round.RoundNumber = CurrentRoundNumber++;
            round.Pairings = CurrentPairings.Clone();
            round.Players = CurrentPlayers.Clone();
            Rounds.Add(round);
            NextRoundDisabled = true;
            OnChange?.Invoke();
            UpdatePairings();
        }


    }
    public static class CloneExtensions
    {
        public static T Clone<T>(this T cloneable) where T : new()
        {
            var toJson = JsonSerializer.Serialize(cloneable);
            return JsonSerializer.Deserialize<T>(toJson);
        }
    }
}
