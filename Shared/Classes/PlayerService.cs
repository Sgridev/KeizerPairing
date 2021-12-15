using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace KeizerPairing.Shared
{
    public class PlayerService
    {
        public event Action OnChange;

        private List<Round> Rounds = new();
        public List<Player> CurrentPlayers { get; set; } = new();

        public List<Pairing> CurrentPairings { get; set; } = new();

        public int CurrentRoundNumber { get; set; } = 1;

        public bool NextRoundDisabled { get; set; }

        /// <summary>
        /// Load Data From Source
        /// </summary>
        /// <param name="playerService"></param>
        public void Load(PlayerService playerService)
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

        /// <summary>
        /// Clear all properties of the class
        /// </summary>
        public void Clear()
        {
            Rounds = new();
            CurrentPlayers = new();
            CurrentPairings = new();
            CurrentRoundNumber = 1;
            NextRoundDisabled = true;
            OnChange?.Invoke();
        }

        /// <summary>
        /// Add Player, Update Pairings and Rankings
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(Player player)
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

        /// <summary>
        /// Update Player, Update Pairings and Rankings
        /// </summary>
        public void UpdatePlayer()
        {
            UpdatePairings();
            UpdateRankings();
        }

        /// <summary>
        /// Update Pairings
        /// </summary>
        private void UpdatePairings()
        {
            CurrentPairings.Clear();
            var playersByRating = CurrentPlayers.Where(x => x.Presence == "Present").OrderByDescending(x => x.Value).ToArray();
            //in the first round we match players by their rating, giving the last player a bye
            if (CurrentRoundNumber == 1)
            {
                playersByRating = CurrentPlayers.Where(x => x.Presence == "Present").OrderByDescending(x => x.Rating).ToArray();
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
            else
            {
                //while there are players in the pool
                while (playersByRating.Length > 0)
                {
                    //last round pairing of the current player
                    var lastRoundPlayerPairing = Rounds[CurrentRoundNumber - 2].Pairings
                        .Where(x => x.White.Number == playersByRating[0].Number || x.Black.Number == playersByRating[0].Number).FirstOrDefault();
                    //if it's a player that has played last round
                    if (lastRoundPlayerPairing != null)
                    {
                        //if there are at least 2 players in the pool
                        if (playersByRating.Length > 1)
                        {
                            //if it's the same pairing as last round
                            if ((lastRoundPlayerPairing.White.Number == playersByRating[0].Number || lastRoundPlayerPairing.White.Number == playersByRating[1].Number)
                                && (lastRoundPlayerPairing.Black.Number == playersByRating[0].Number || lastRoundPlayerPairing.Black.Number == playersByRating[1].Number))
                            {
                                //if there's another player to match 
                                //we match them and remove them from the pool
                                if (playersByRating.Length > 2)
                                {
                                    Pairing pairing;
                                    if (lastRoundPlayerPairing.White.Number == playersByRating[0].Number)
                                        pairing = new() { White = playersByRating[2], Black = playersByRating[0] };
                                    else
                                        pairing = new() { White = playersByRating[0], Black = playersByRating[2] };
                                    playersByRating = playersByRating.Where((item, index) => index != 0 && index != 2).ToArray();
                                    CurrentPairings.Add(pairing);
                                }
                                //if there's no other player to match them, we match them with opposite colors and we remove them from the pool
                                else
                                {
                                    Pairing pairing;
                                    if (lastRoundPlayerPairing.White.Number == playersByRating[0].Number)
                                        pairing = new() { White = playersByRating[1], Black = playersByRating[0] };
                                    else
                                        pairing = new() { White = playersByRating[0], Black = playersByRating[1] };

                                    playersByRating = playersByRating.Skip(2).ToArray();
                                    CurrentPairings.Add(pairing);
                                }
                            }
                            //if it's not the same pairing we match the two players and remove them from the pool
                            else
                            {
                                Pairing pairing;
                                if (lastRoundPlayerPairing.White.Number == playersByRating[0].Number)
                                    pairing = new() { White = playersByRating[1], Black = playersByRating[0] };
                                else
                                    pairing = new() { White = playersByRating[0], Black = playersByRating[1] };
                                playersByRating = playersByRating.Skip(2).ToArray();
                                CurrentPairings.Add(pairing);
                            }
                        }
                        //if there's only one player in the pool, we give them a bye and remove him from the pool
                        else
                        {
                            Pairing pairing = new() { White = playersByRating[0], Black = new Player { Name = "Bye" }, Result = "1 - 0" };
                            playersByRating = playersByRating.Skip(1).ToArray();
                            CurrentPairings.Add(pairing);
                        }

                    }
                    //if it's a player that has not played last round
                    else
                    {
                        //if there's another player in the pool, we match them and remove them from the pool
                        if (playersByRating.Length > 1)
                        {
                            Pairing pairing;
                            var lastRoundOpponentPairing = Rounds[CurrentRoundNumber - 2].Pairings
                      .Where(x => x.White.Number == playersByRating[1].Number || x.Black.Number == playersByRating[1].Number).FirstOrDefault();
                            //if opponent played last round
                            if (lastRoundOpponentPairing != null)
                            {
                                if (lastRoundOpponentPairing.White.Number == playersByRating[1].Number)
                                    pairing = new() { White = playersByRating[0], Black = playersByRating[1] };
                                else
                                    pairing = new() { White = playersByRating[1], Black = playersByRating[0] };
                            }
                            else
                                pairing = new() { White = playersByRating[0], Black = playersByRating[1] };

                            playersByRating = playersByRating.Skip(2).ToArray();
                            CurrentPairings.Add(pairing);
                        }
                        //else we give him a bye and remove it from the pool
                        else
                        {
                            Pairing pairing = new() { White = playersByRating[0], Black = new Player { Name = "Bye" }, Result = "1 - 0" };
                            playersByRating = playersByRating.Skip(1).ToArray();
                            CurrentPairings.Add(pairing);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Update Rankings
        /// </summary>
        public void UpdateRankings()
        {
            //if it's the first round
            if (CurrentRoundNumber == 1)
            {
                int counter = 30;
                int position = 1;
                //set starting score and values by rating
                foreach (var player in CurrentPlayers.OrderByDescending(x => x.Rating))
                {
                    player.Value = counter + CurrentPlayers.Count;
                    player.Score = player.Value;
                    player.Wins = 0;
                    player.Losses = 0;
                    player.Draws = 0;
                    counter--;
                }
                //Calculate current scores and values by current pairings
                //if there's a pairing with a result
                if (CurrentPairings.Where(x => x.Result != null && x.Result != string.Empty).Any())
                {
                    foreach (var player in CurrentPlayers)
                    {
                        //if the current player has a pairing with a result
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
                //set new values and positions by score
                counter = 30;
                foreach (var player in CurrentPlayers.OrderByDescending(x => x.Score))
                {
                    player.Value = counter + CurrentPlayers.Count;
                    counter--;
                    player.Position = position++;
                }
            }
            //if it's not the first round
            else
            {
                //if there's a pairing with a result
                if (CurrentPairings.Where(x => x.Result != null && x.Result != string.Empty).Any())
                {
                    foreach (var player in CurrentPlayers)
                    {
                        var currentPairing = CurrentPairings.Where(x => (x.White.Number == player.Number || x.Black.Number == player.Number) && x.Result != null && x.Result != string.Empty).FirstOrDefault();
                        //if the current player has a pairing with a result
                        if (currentPairing != null)
                        {
                            //find the values of the current player in the previous round
                            var lastRoundplayer = Rounds[CurrentRoundNumber - 2].Players.Where(x => x.Number == player.Number).FirstOrDefault();
                            
                            //if player is white
                            if (currentPairing.White.Number == player.Number)
                            {
                                //if player had no bye
                                if (Rounds[CurrentRoundNumber - 2].Players.Any(x => x.Number == currentPairing.Black.Number))
                                {
                                    //find  the values of the opponent player in the previous round
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
                                //if player had bye
                                else
                                {
                                    player.Score = lastRoundplayer.Score + lastRoundplayer.Value;
                                    player.Wins = lastRoundplayer.Wins + 1;
                                    player.Losses = lastRoundplayer.Losses;
                                    player.Draws = lastRoundplayer.Draws;
                                }
                            }
                            //if player is black
                            else
                            {
                                //find the values of the opponent player in the previous round (black can't be against a bye)
                                int opponentLastRoundValue = Rounds[CurrentRoundNumber - 2].Players.Where(x => x.Number == currentPairing.White.Number).FirstOrDefault().Value;
                                if (currentPairing.Result == "1/2 - 1/2")
                                {
                                    player.Score = lastRoundplayer.Score + (opponentLastRoundValue / 2);
                                    player.Draws = lastRoundplayer.Draws + 1;
                                    player.Losses = lastRoundplayer.Losses;

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
            //if all the pairings are completed, enable the next round
            NextRoundDisabled = CurrentPairings.Any(x => x.Result == null || x.Result == string.Empty);

            OnChange?.Invoke();

        }

        /// <summary>
        /// Add current round to Rounds, update player values for next round, set players presence to present and update pairings
        /// </summary>
        public void NextRound()
        {
            Round round = new()
            {
                RoundNumber = CurrentRoundNumber++,
                Pairings = CurrentPairings.Clone()
            };
            Rounds.Add(round);
            UpdatePlayersForNextRound();
            round.Players = CurrentPlayers.Clone();
            SetPlayersPresenceToPresent();
            NextRoundDisabled = true;
            OnChange?.Invoke();
            UpdatePairings();
        }

        /// <summary>
        /// Update the presence property of every player to "Present"
        /// </summary>
        private void SetPlayersPresenceToPresent()
        {
            foreach (var player in CurrentPlayers)
                player.Presence = "Present";
        }

        /// <summary>
        /// Update players for next round
        /// </summary>
        private void UpdatePlayersForNextRound()
        {
            //Create a clone of this round stats
            List<Player> playerRoundStats = CurrentPlayers.Clone();

            //Set player starting value to score ranking
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

            //Calculate all scores based on new values
            foreach (var round in Rounds)
            {
                foreach (Pairing pairing in round.Pairings)
                {
                    //find black and white player values for each pairing for each round
                    Player whitePlayer = CurrentPlayers.Where(x => x.Number == pairing.White.Number).FirstOrDefault();
                    Player blackPlayer = CurrentPlayers.Where(x => x.Number == pairing.Black.Number).FirstOrDefault();
                    //if black wasn't a bye
                    if (blackPlayer != null)
                    {
                        //find black and white player values of the current round
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
                                blackPlayer.Draws++;
                                whitePlayer.Draws++;
                                break;
                        }
                    }
                    //if black was a bye
                    else
                    {
                        int whitePlayerRoundValue = playerRoundStats.Where(x => x.Number == pairing.White.Number).FirstOrDefault().Value;
                        whitePlayer.Score += whitePlayerRoundValue;
                        whitePlayer.Wins++;
                    }
                }
            }

            //Calculate new values and positions by calculated scores
            counter = 30;
            int position = 1;
            foreach (var player in CurrentPlayers.OrderByDescending(x => x.Score))
            {
                player.Value = counter + CurrentPlayers.Count;
                counter--;
                player.Position = position++;
            }
        }




    }



}
