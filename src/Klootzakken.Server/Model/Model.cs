﻿using System;
using System.Linq;

namespace Klootzakken.Server.Model
{
    public enum CardSuit
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs,
    }

    public enum CardValue
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14,
    }

    public class Card : IEquatable<Card>
    {
        public bool Equals(Card other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Suit == other.Suit && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Card) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public Card(CardSuit suit, CardValue value)
        {
            Suit = suit;
            Value = value;
        }

        public CardSuit Suit { get; }
        public CardValue Value { get; }
    }

    public class Play : IEquatable<Play>
    {
        public bool Equals(Play other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (PlayedCards.Length != other.PlayedCards.Length) return false;
            return PlayedCards.Union(other.PlayedCards).Count() == PlayedCards.Length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Play) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public Play(Card[] playedCards)
        {
            PlayedCards = playedCards;
        }

        public Card[] PlayedCards { get; }
    }

    public class Player
    {
        public Player(string name, Play[] playsThisRound)
        {
            Name = name;
            PlaysThisRound = playsThisRound;
        }

        public string Name { get; }
        public Play[] PlaysThisRound { get; }
    }

    public class YourPlayer : Player
    {
        public YourPlayer(string name, Play[] playsThisRound, Card[] cardsInHand, Play[] possibleActions) : base(name, playsThisRound)
        {
            CardsInHand = cardsInHand;
            PossibleActions = possibleActions;
        }

        public Card[] CardsInHand { get; }
        public Play[] PossibleActions { get; }
    }

    public class OtherPlayer : Player
    {
        public OtherPlayer(string name, Play[] playsThisRound, int cardCount) : base(name, playsThisRound)
        {
            CardCount = cardCount;
        }

        public int CardCount { get; }
    }

    public class GameState
    {
        public GameState(YourPlayer[] players, Card centerCard, int activePlayer)
        {
            Players = players;
            CenterCard = centerCard;
            ActivePlayer = activePlayer;
        }

        public YourPlayer[] Players { get; }
        public Card CenterCard { get; }
        public int ActivePlayer { get; }
    }

    public class PublicGameState
    {
        public PublicGameState(Player you, OtherPlayer[] otherPlayers, Card centerCard, string activePlayerName)
        {
            You = you;
            OtherPlayers = otherPlayers;
            CenterCard = centerCard;
            ActivePlayerName = activePlayerName;
        }

        public Player You { get; }
        public OtherPlayer[] OtherPlayers { get; }
        public Card CenterCard { get; }
        public string ActivePlayerName { get; }
    }

    public class Lobby
    {
        public Lobby(string[] players)
        {
            Players = players;
        }

        public string[] Players { get; }
    }
}