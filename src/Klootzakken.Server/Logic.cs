using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Klootzakken.Server.Model;

namespace Klootzakken.Server
{
    public static class Logic
    {
        public static Card[] TopDownDeck { get; } = CreateTopDownDeck().ToArray();

        private static IEnumerable<Card> CreateTopDownDeck()
        {
            for (var cardValue = CardValue.Ace; cardValue >= CardValue.Two; cardValue--)
            {
                yield return new Card(CardSuit.Hearts, cardValue);
                yield return new Card(CardSuit.Spades, cardValue);
                yield return new Card(CardSuit.Diamonds, cardValue);
                yield return new Card(CardSuit.Clubs, cardValue);
            }
        }

        public static GameState Deal(this Lobby lobby)
        {
            var playerCount = lobby.Players.Length;
            var deck = new Stack<Card>(TopDownDeck.Take(8*playerCount).Reverse());
            var deal = new List<Card>[playerCount];
            for(int playerNo = 0; playerNo<playerCount; playerNo++)
                deal[playerNo] = new List<Card>();

            int playerForCard = 0;
            while (deck.Count != 0)
            {
                var dealtCard = deck.Pop();
                while (true)
                {
                    playerForCard = Random(playerCount);
                    if (deal[playerForCard].Count == 8)
                        continue;
                    deal[playerForCard].Add(dealtCard);
                    break;
                }
            }

            var startPlayer = playerForCard;
            var players = lobby.Players
                .Select((pl, i) => new YourPlayer(pl, new Play[0], deal[i].ToArray(), new Play[0]))
                .Select((pl, i) => i == startPlayer?pl.WithStartOptions():pl)
                .ToArray();
            return new GameState(players, null, startPlayer);
        }

        public static YourPlayer WithStartOptions(this YourPlayer player)
        {
            var possibleActions = player.CardsInHand.StartOptions();
            return new YourPlayer(player.Name, new Play[0], player.CardsInHand, possibleActions );
        }

        public static Play[] StartOptions(this Card[] cardsInHand)
        {
            var retVal = cardsInHand.GroupBy( card => card.Value).SelectMany( AllPermutations).Select( cardArray => new Play(cardArray)).Distinct();
            return retVal.ToArray();
        }

        private static IEnumerable<Card[]> AllPermutations(IEnumerable<Card> cardsOfSameValue)
        {
            var cards = cardsOfSameValue.ToArray();
            // Individual Cards
            foreach (var card in cards)
                yield return new[] {card};

            if (cards.Length != 1)
                // All Cards
                yield return cards;

            if (cards.Length >= 3)
                // Skip only one
                for (int q = 0; q < cards.Length; q++)
                    yield return cards.Take(q).Concat(cards.Skip(q + 1)).ToArray();

            if (cards.Length == 4)
                // Take Two out of Four
                for (int q = 0; q < cards.Length; q++)
                for (int w = q + 1; w < cards.Length; w++)
                    yield return cards.Skip(q).Take(1).Concat(cards.Skip(w).Take(1)).ToArray();
        }

        private static Random RandomGenerator { get; } = new Random();

        private static int Random(int playerCount)
        {
            return RandomGenerator.Next(playerCount);
        }
    }
}