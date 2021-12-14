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

        public void AddingPlayer(Player player)
        {
            if (CurrentRoundNumber > 1)
            {
                player.Value = CurrentPlayers.Min(x => x.Value - 1);
                player.Score = player.Value;
                player.Position = CurrentPlayers.Count + 1;
                Rounds[CurrentRoundNumber - 2].Players.Add(player.Clone());
            }

            CurrentPlayers.Add(player);
            UpdatePairings();
            UpdateRankings();
        }

        public void UpdatingPlayer()
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
            for (int i = 0; i < playersByRating.Length; i += 2)
            {
                if (i + 1 < playersByRating.Length)
                {
                    Pairing pairing = new() { White = playersByRating[i], Black = playersByRating[i + 1] };
                    CurrentPairings.Add(pairing);
                }
                else
                {
                    Pairing pairing = new() { White = playersByRating[i], Black = new Player { Name = "Bye" }, Result = "1 - 0" };
                    CurrentPairings.Add(pairing);
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

                if (CurrentPairings.Where(x => x.Result != null && x.Result != string.Empty).Any())
                {

                    foreach (var player in CurrentPlayers)
                    {
                        var currentPairing = CurrentPairings.Where(x => (x.White.Number == player.Number || x.Black.Number == player.Number) && x.Result != null && x.Result != string.Empty).FirstOrDefault();
                        if (currentPairing != null)
                        {
                            if (currentPairing.White.Number == player.Number)
                            {
                                if (currentPairing.Result == "1/2 - 1/2")
                                {
                                    player.Score = player.Value + (currentPairing.Black.Value / 2);
                                    player.Draws += 1;
                                }
                                else if (currentPairing.Result == "1 - 0")
                                {
                                    if (currentPairing.Black.Name == "Bye")
                                        player.Score = player.Value + player.Value;
                                    else
                                        player.Score = player.Value + currentPairing.Black.Value;

                                    player.Wins += 1;

                                }
                                else
                                    player.Losses += 1;
                            }
                            else
                            {
                                if (currentPairing.Result == "1/2 - 1/2")
                                {
                                    player.Score = player.Value + (currentPairing.White.Value / 2);
                                    player.Draws += 1;

                                }
                                else if (currentPairing.Result == "0 - 1")
                                {
                                    player.Score = player.Value + currentPairing.White.Value;
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
                                if (Rounds[CurrentRoundNumber - 2].Players.Any(x => x.Number == currentPairing.Black.Number))
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
                                    player.Score = lastRoundplayer.Score + lastRoundplayer.Value;
                                    player.Wins = lastRoundplayer.Wins + 1;
                                    player.Losses = lastRoundplayer.Losses;
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

        public void NextRound()
        {
            Round round = new Round();
            round.RoundNumber = CurrentRoundNumber++;
            round.Pairings = CurrentPairings.Clone();
            Rounds.Add(round);
            UpdatePlayerValuesForNextRound();
            round.Players = CurrentPlayers.Clone();
            NextRoundDisabled = true;
            OnChange?.Invoke();
            UpdatePairings();
        }

        private void UpdatePlayerValuesForNextRound()
        {
            List<Player> playerRoundStats = CurrentPlayers.Clone();

            int counter = 30;
            foreach (var player in CurrentPlayers.OrderByDescending(x => x.Score))
            {
                player.Value = counter + CurrentPlayers.Count;
                player.Score = player.Value;
                player.Wins = 0;
                player.Losses = 0;
                player.Draws = 0;
                counter--;
            }

            foreach (var round in Rounds)
            {
                foreach (Pairing pairing in round.Pairings)
                {
                    Player whitePlayer = CurrentPlayers.Where(x => x.Number == pairing.White.Number).FirstOrDefault();
                    Player blackPlayer = CurrentPlayers.Where(x => x.Number == pairing.Black.Number).FirstOrDefault();
                    if (blackPlayer != null)
                    {
                        int whitePlayerRoundValue = playerRoundStats.Where(x => x.Number == pairing.White.Number).FirstOrDefault().Value;
                        int blackPlayerRoundValue = playerRoundStats.Where(x => x.Number == pairing.Black.Number).FirstOrDefault().Value;
                        switch (pairing.Result)
                        {
                            case "1 - 0":
                                whitePlayer.Score += blackPlayerRoundValue;
                                whitePlayer.Wins++;
                                blackPlayer.Losses++;
                                break;
                            case "0 - 1":
                                blackPlayer.Score += whitePlayerRoundValue;
                                blackPlayer.Wins++;
                                whitePlayer.Losses++;
                                break;
                            case "1/2 - 1/2":
                                whitePlayer.Score += blackPlayerRoundValue / 2;
                                blackPlayer.Score += whitePlayerRoundValue / 2;
                                break;
                        }
                    }
                    else
                    {
                        int whitePlayerRoundValue = playerRoundStats.Where(x => x.Number == pairing.White.Number).FirstOrDefault().Value;
                        whitePlayer.Score += whitePlayerRoundValue;
                        whitePlayer.Wins++;
                    }
                }
            }

            counter = 30;
            int position = 1;
            foreach (var player in CurrentPlayers.OrderByDescending(x => x.Score))
            {
                player.Value = counter + CurrentPlayers.Count;
                counter--;
                player.Position = position++;
            }
        }

        public void ClearTournament()
        {
            Rounds = new List<Round>();
            CurrentPlayers = new List<Player>();
            CurrentPairings = new List<Pairing>();
            CurrentRoundNumber = 1;
            NextRoundDisabled = true;
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
    }



}
