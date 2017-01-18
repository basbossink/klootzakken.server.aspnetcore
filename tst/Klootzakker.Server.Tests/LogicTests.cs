﻿using System.Linq;
using Klootzakken.Server;
using Klootzakken.Server.Model;
using Xunit;

namespace Klootzakker.Server.Tests
{
    public class LogicTests
    {
        [Fact]
        public void InitialGameStateHasNoCenterCard()
        {
            var sut = DealFourPlayerGame();
            Assert.Null(sut.CenterCard);
        }

        [Fact]
        public void InitialGameStateActivePlayerIsBetween0AndThree()
        {
            for (int q = 0; q < 250; q++)
                Assert.InRange(DealFourPlayerGame().ActivePlayer, 0, 3);
        }

        [Fact]
        public void InitialGameStateAllPlayersHaveEightCards()
        {
            var game = DealFourPlayerGame();
            Assert.All(game.Players, player => Assert.Equal(8, player.CardsInHand.Length));
        }

        [Fact]
        public void InitialGameStateAllPlayersHaveCardsOfSevenOrHigher()
        {
            var game = DealFourPlayerGame();
            Assert.All(game.Players,
                player => Assert.All(player.CardsInHand, c => Assert.InRange(c.Value, CardValue.Seven, CardValue.Ace)));
        }

        [Fact]
        public void InitialGameStateAllPlayersHaveCardsSortedByValue()
        {
            var game = DealFourPlayerGame();
            Assert.All(game.Players,
                player =>
                    Assert.Equal(player.CardsInHand.Select(c => c.Value),
                        player.CardsInHand.Select(c => c.Value).OrderBy(v => v)));
        }

        [Fact]
        public void InitialGameStateActivePlayerIsDistributedEvenly()
        {
            var timesStartPlayer = new int[4];
            for (int q = 0; q < 400; q++)
                timesStartPlayer[DealFourPlayerGame().ActivePlayer]++;
            Assert.All(timesStartPlayer, times => Assert.InRange(times, 75, 125));
        }

        [Fact]
        public void InitialGameStateActivePlayerHasSevenOfClubsInHand()
        {
            var game = DealFourPlayerGame();
            var activePlayer = game.Players[game.ActivePlayer];
            Assert.Contains(new Card(CardSuit.Clubs, CardValue.Seven), activePlayer.CardsInHand);
        }

        [Fact]
        public void InitialGameStateActivePlayerCannotPass()
        {
            var game = DealFourPlayerGame();
            var activePlayer = game.Players[game.ActivePlayer];
            Assert.DoesNotContain(new Play(new Card[0]), activePlayer.PossibleActions);
        }

        [Fact]
        public void InitialGameStateActivePlayerHasOptions()
        {
            var game = DealFourPlayerGame();
            var activePlayer = game.Players[game.ActivePlayer];
            Assert.NotEmpty(activePlayer.PossibleActions);
        }

        [Fact]
        public void StreetResultsInEightStartOptions()
        {
            var cardsInHand = HandOfOnlySingles;
            var possiblePlays = cardsInHand.StartOptions();
            Assert.Equal(8, possiblePlays.Length);
            Assert.All(possiblePlays, play => Assert.Equal(1, play.PlayedCards.Length));
        }

        [Fact]
        public void OnlyPairsResultsInTwelveStartOptions()
        {
            var cardsInHand = HandOfFourPairs;
            var possiblePlays = cardsInHand.StartOptions();
            Assert.Equal(12, possiblePlays.Length);
            Assert.Equal(8, possiblePlays.Count(play => play.PlayedCards.Length == 1));
            Assert.Equal(4, possiblePlays.Count(play => play.PlayedCards.Length == 2));
        }

        [Fact]
        public void TriplesAndAPairResultsInSeventeenStartOptions()
        {
            var cardsInHand = HandOfTwoTriplesAndAPair;
            var possiblePlays = cardsInHand.StartOptions();
            Assert.Equal(17, possiblePlays.Length);
            Assert.Equal(8, possiblePlays.Count(play => play.PlayedCards.Length == 1));
            Assert.Equal(7, possiblePlays.Count(play => play.PlayedCards.Length == 2));
            Assert.Equal(2, possiblePlays.Count(play => play.PlayedCards.Length == 3));
        }

        [Fact]
        public void TwoFoursResultsInThirtyStartOptions()
        {
            var cardsInHand = HandOfTwoFoursomes;
            var possiblePlays = cardsInHand.StartOptions();
            Assert.Equal(30, possiblePlays.Length);
            Assert.Equal(8, possiblePlays.Count(play => play.PlayedCards.Length == 1));
            Assert.Equal(12, possiblePlays.Count(play => play.PlayedCards.Length == 2));
            Assert.Equal(8, possiblePlays.Count(play => play.PlayedCards.Length == 3));
            Assert.Equal(2, possiblePlays.Count(play => play.PlayedCards.Length == 4));
        }

        [Fact]
        public void HandOfSinglesCanRespondToSevenWithPassOrAnyHigherCard()
        {
            var cardsInHand = HandOfOnlySingles;
            var possiblePlays = cardsInHand.Options(new Play(new[] {new Card(CardSuit.Clubs, CardValue.Seven),}));
            Assert.Equal(8, possiblePlays.Length);
            Assert.Equal(1, possiblePlays.Count(play => play.PlayedCards.Length == 0));
            Assert.Equal(7, possiblePlays.Count(play => play.PlayedCards.Length == 1));
            Assert.All(possiblePlays,
                play => Assert.True(play.PlayedCards.Length == 0 || play.PlayedCards[0].Value > CardValue.Seven));
        }

        [Fact]
        public void HandOfSinglesCanRespondToKingWithPassOrAce()
        {
            var cardsInHand = HandOfOnlySingles;
            var possiblePlays = cardsInHand.Options(new Play(new[] {new Card(CardSuit.Clubs, CardValue.King),}));
            Assert.Equal(2, possiblePlays.Length);
            Assert.Equal(1, possiblePlays.Count(play => play.PlayedCards.Length == 0));
            Assert.Equal(1, possiblePlays.Count(play => play.PlayedCards.Length == 1));
            Assert.All(possiblePlays,
                play => Assert.True(play.PlayedCards.Length == 0 || play.PlayedCards[0].Value > CardValue.King));
        }

        [Fact]
        public void HandOfSinglesCanRespondToPairWithPass()
        {
            var cardsInHand = HandOfOnlySingles;
            var possiblePlays =
                cardsInHand.Options(
                    new Play(new[]
                        {new Card(CardSuit.Clubs, CardValue.Seven), new Card(CardSuit.Spades, CardValue.Seven),}));
            Assert.Equal(1, possiblePlays.Length);
            Assert.Equal(1, possiblePlays.Count(play => play.PlayedCards.Length == 0));
        }

        [Fact]
        public void HandOfFoursomesCanRespondToPairWithHigherPair()
        {
            var cardsInHand = HandOfTwoFoursomes;
            var possiblePlays =
                cardsInHand.Options(
                    new Play(new[]
                        {new Card(CardSuit.Clubs, CardValue.Seven), new Card(CardSuit.Spades, CardValue.Seven),}));
            Assert.Equal(7, possiblePlays.Length);
            Assert.Equal(1, possiblePlays.Count(play => play.PlayedCards.Length == 0));
            Assert.Equal(6, possiblePlays.Count(play => play.PlayedCards.Length == 2));
        }

        [Fact]
        public void FollowPlayersHaveOptionToPass()
        {
            var game = DealFourPlayerGame();
            var iter = game.WhenPlaying(game.Players[game.ActivePlayer].PossibleActions[0]);
            for (int q = 1; q <= 3; q++)
            {
                Assert.Equal((game.ActivePlayer + q) % 4, iter.ActivePlayer);
                Assert.Contains(Play.Pass, iter.Players[iter.ActivePlayer].PossibleActions);
                iter = iter.WhenPlaying(Play.Pass);
            }
        }

        [Fact]
        public void StartPlayerWinsHandIfThreePlayersPass()
        {
            var game = DealFourPlayerGame();
            var firstPlayed = game.WhenPlaying(game.Players[game.ActivePlayer].PossibleActions[0]);
            var onePassed = firstPlayed.WhenPlaying(Play.Pass);
            var twoPassed = onePassed.WhenPlaying(Play.Pass);
            var threePassed = twoPassed.WhenPlaying(Play.Pass);
            Assert.Equal(game.ActivePlayer, threePassed.ActivePlayer);
            Assert.DoesNotContain(Play.Pass, threePassed.Players[threePassed.ActivePlayer].PossibleActions);
        }

        [Fact]
        public void SecondPlayerWinsHandIfThreePlayersPass()
        {
            var game = DealFourPlayerGame();
            var firstPlayed = game.WhenPlaying(game.Players[game.ActivePlayer].PossibleActions[0]);
            var secondPlayed =
                firstPlayed.WhenPlaying(
                    firstPlayed.Players[firstPlayed.ActivePlayer].PossibleActions.First(pl => !pl.IsPass));
            var onePassed = secondPlayed.WhenPlaying(Play.Pass);
            var twoPassed = onePassed.WhenPlaying(Play.Pass);
            var threePassed = twoPassed.WhenPlaying(Play.Pass);
            Assert.Equal(firstPlayed.ActivePlayer, threePassed.ActivePlayer);
            Assert.DoesNotContain(Play.Pass, threePassed.Players[threePassed.ActivePlayer].PossibleActions);
        }

        [Fact]
        public void WhenGameEndsOnlyOnePlayerHasCardsLeftInHand()
        {
            var game = DealFourPlayerGame();
            game = PlayGameUntilEnded(game);
            Assert.Equal(1, game.Players.Count(pl => pl.CardsInHand.Length != 0));
        }

        [Fact]
        public void WhenGameEndsEachRoleIsAssigned()
        {
            var game = DealFourPlayerGame();
            game = PlayGameUntilEnded(game);
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.President));
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.VicePresident));
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.ViezeKlootzak));
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.Klootzak));
        }

        [Fact]
        public void WhenGameEndsEveryoneCanOnlyPass()
        {
            var game = DealFourPlayerGame();
            game = PlayGameUntilEnded(game);
            Assert.All(game.Players, pl => Assert.Equal(pl.PossibleActions, Play.PassOnly));
        }

        [Fact]
        public void WhenThreePlayerGameEndsProperRolesAreAssigned()
        {
            var game = DealThreePlayerGame();
            game = PlayGameUntilEnded(game);
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.President));
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.Neutraal));
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.Klootzak));
        }

        [Fact]
        public void WhenFivePlayerGameEndsProperRolesAreAssigned()
        {
            var game = DealFivePlayerGame();
            game = PlayGameUntilEnded(game);
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.President));
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.VicePresident));
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.Neutraal));
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.ViezeKlootzak));
            Assert.Equal(1, game.Players.Count(pl => pl.NewRank == Rank.Klootzak));
        }

        [Fact]
        public void ActivePlayerAlwaysHasPossibleActions()
        {
            var game = DealFourPlayerGame();
            while (game.Phase == GamePhase.Playing)
            {
                var activePlayer = game.Players[game.ActivePlayer];
                Assert.True(activePlayer.PossibleActions.Length > 0);
                game = game.WhenPlaying(activePlayer.PossibleActions[0]);
            }
        }

        [Fact]
        public void WhenOnePlayerPassesInEndedGamePhaseBecomesSwapping()
        {
            var endedGame = CreateEndedGame;
            var swappingGame = endedGame.WhenPlaying(Play.Pass);
            Assert.Equal(GamePhase.SwappingCards, swappingGame.Phase);
        }

        [Fact]
        public void SwappingGameHasCorrectOptions()
        {
            var game = CreateSwappingFivePlayerGame;
            var president = game.Players.Single(pl => pl.NewRank == Rank.President);
            var vicePresident = game.Players.Single(pl => pl.NewRank == Rank.VicePresident);
            var neutraal = game.Players.Single(pl => pl.NewRank == Rank.Neutraal);
            var viezeKlootzak = game.Players.Single(pl => pl.NewRank == Rank.ViezeKlootzak);
            var klootzak = game.Players.Single(pl => pl.NewRank == Rank.Klootzak);
            Assert.Equal(president.CardsInHand.Select(c => c.Value).OrderBy(c => c).Take(2), president.PossibleActions[0].PlayedCards.Select(c => c.Value).OrderBy(c => c));
            Assert.Equal(vicePresident.CardsInHand.Select(c => c.Value).OrderBy(c => c).Take(1), vicePresident.PossibleActions[0].PlayedCards.Select(c => c.Value).OrderBy(c => c));
            Assert.Equal(Play.PassOnly, neutraal.PossibleActions);
            Assert.Equal(viezeKlootzak.CardsInHand.Select(c => c.Value).OrderByDescending(c => c).Take(1), viezeKlootzak.PossibleActions[0].PlayedCards.Select(c => c.Value).OrderByDescending(c => c));
            Assert.Equal(klootzak.CardsInHand.Select(c => c.Value).OrderByDescending(c => c).Take(2), klootzak.PossibleActions[0].PlayedCards.Select(c => c.Value).OrderByDescending(c => c));
        }

        #region Helpers

        private static GameState CreateEndedGame => PlayGameUntilEnded(DealFourPlayerGame());
        private static GameState CreateSwappingGame => CreateEndedGame.WhenPlaying(Play.Pass);
        private static GameState CreateSwappingFivePlayerGame => PlayGameUntilEnded(DealFivePlayerGame()).WhenPlaying(Play.Pass);

        private static GameState PlayGameUntilEnded(GameState game)
        {
            var endedGame = game;
            while (endedGame.Phase != GamePhase.Ended)
            {
                var activePlayer = endedGame.Players.First(pl => pl.PossibleActions.Length > 0);
                endedGame = endedGame.WhenPlaying(activePlayer.PossibleActions[0]);
            }
            return endedGame;
        }

        private static Card[] HandOfOnlySingles
        {
            get { return Enumerable.Range(7, 8).Cast<CardValue>().Select(v => new Card(CardSuit.Hearts, v)).ToArray(); }
        }

        private static Card[] HandOfFourPairs
        {
            get
            {
                return Enumerable.Range(7, 4)
                    .Cast<CardValue>()
                    .SelectMany(v => new[] {new Card(CardSuit.Hearts, v), new Card(CardSuit.Spades, v)})
                    .ToArray();
            }
        }

        private static Card[] HandOfTwoTriplesAndAPair
        {
            get
            {
                return Enumerable.Range(7, 3)
                    .Cast<CardValue>()
                    .SelectMany(
                        v =>
                            new[]
                            {
                                new Card(CardSuit.Hearts, v), new Card(CardSuit.Diamonds, v),
                                new Card(CardSuit.Spades, v)
                            })
                    .Take(8)
                    .ToArray();
            }
        }

        private static Card[] HandOfTwoFoursomes
        {
            get
            {
                return Enumerable.Range(7, 2)
                    .Cast<CardValue>()
                    .SelectMany(
                        v =>
                            new[]
                            {
                                new Card(CardSuit.Hearts, v), new Card(CardSuit.Diamonds, v),
                                new Card(CardSuit.Spades, v), new Card(CardSuit.Clubs, v)
                            })
                    .ToArray();
            }
        }

        private static GameState DealFourPlayerGame()
        {
            var lobby = new Lobby(new[] {"HDB", "HDS", "HDM", "HDb"});
            var actual = lobby.DealFirstGame();
            return actual;
        }

        private static GameState DealFivePlayerGame()
        {
            var lobby = new Lobby(new[] {"HDB", "HDS", "HDM", "HDb", "HDK"});
            var actual = lobby.DealFirstGame();
            return actual;
        }

        private static GameState DealThreePlayerGame()
        {
            var lobby = new Lobby(new[] {"HDB", "HDS", "HDM"});
            var actual = lobby.DealFirstGame();
            return actual;
        }

        #endregion
    }
}
