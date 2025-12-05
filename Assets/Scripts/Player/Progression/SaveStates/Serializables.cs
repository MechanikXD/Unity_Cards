#nullable enable
using System;
using Core.Cards.Card;
using Core.Cards.Card.Data;
using Core.Cards.Hand;
using Newtonsoft.Json;
using Other.Dialog;
using Player.Progression.Buffs;
using UnityEngine;

namespace Player.Progression.SaveStates
{
    [Serializable]
    public class SerializableBoard
    {
        public SerializableCardData?[] _playerCards;
        public SerializableCardData?[] _enemyCards;

        public SerializableBoard(SerializableCardData?[] playerCards, SerializableCardData?[] enemyCards)
        {
            _playerCards = playerCards;
            _enemyCards = enemyCards;
        }
    }

    [Serializable]
    public class SerializableDialog
    {
        public string _spritePath;
        public int[] _options;
        public string[] _dialogs;
        public int _currentDialogIndex;
        
        [JsonConstructor]
        public SerializableDialog(string spritePath, string[] dialogs, int[] options, int currentDialogIndex)
        {
            _spritePath = spritePath;
            _dialogs = dialogs;
            _options = options;
            _currentDialogIndex = currentDialogIndex;
        }

        public SerializableDialog(DialogSettings settings)
        {
            _spritePath = settings.SpritePath;
            _dialogs = settings.Dialogues;
            _options = new int[settings.Options.Count];
            for (var i = 0; i < settings.Options.Count; i++) _options[i] = settings.Options[i].ID;
            _currentDialogIndex = 0;
        }

        public DialogSettings ToDialogSetting(BuffDataBase db)
        {
            var buffs = new BuffBase[_options.Length];
            for (var i = 0; i < _options.Length; i++)
            {
                buffs[i] = db.Get<BuffBase>(_options[i]);
            }

            return new DialogSettings(_spritePath, _dialogs, buffs);
        }
    }

    [Serializable]
    public class SerializablePlayerHand
    {
        public int _maxHealth;
        public int _currentHealth;

        public int _maxHope;
        public int _currentHope;

        public int _hopeRegeneration;
        public int _startingHandSize;
        public int _drawCount;

        public SerializableCardData[] _deck;
        public SerializableCardData[] _currentDeck;
        public SerializableCardData[] _hand;

        [JsonConstructor]
        public SerializablePlayerHand(
            int maxHealth, int currentHealth, int maxHope, int currentHope, int hopeRegeneration,
            int startingHandSize, int drawCount, SerializableCardData[] deck,
            SerializableCardData[] currentDeck, SerializableCardData[] hand)
        {
            _maxHealth = maxHealth;
            _currentHealth = currentHealth;
            _maxHope = maxHope;
            _currentHope = currentHope;
            _hopeRegeneration = hopeRegeneration;
            _startingHandSize = startingHandSize;
            _drawCount = drawCount;
            _deck = deck;
            _currentDeck = currentDeck;
            _hand = hand;
        }
        
        public SerializablePlayerHand(PlayerHand hand)
        {
            _maxHealth = hand.MaxHealth;
            _currentHealth = hand.CurrentHealth;
            _maxHope = hand.MaxHope;
            _currentHope = hand.CurrentHope;
            _hopeRegeneration = hand.HopeRegeneration;
            _startingHandSize = hand.StartingHandSize;
            _drawCount = hand.DrawCount;
            
            _deck = new SerializableCardData[hand.Deck.Length];
            for (var i = 0; i < hand.Deck.Length; i++) 
                _deck[i] = hand.Deck[i].SerializeSelf();
            
            _currentDeck = new SerializableCardData[hand.CurrentDeck.Count];
            var index = 0;
            foreach (var card in hand.CurrentDeck)
            {
                _currentDeck[index] = card.SerializeSelf();
                index++;
            }
            
            _hand = new SerializableCardData[hand.CardsInHand.Count];
            for (var i = 0; i < hand.CardsInHand.Count; i++) 
                _hand[i] = hand.CardsInHand[i].SerializeSelf();
        }
    }

    [Serializable]
    public class SerializableCardData
    {
        public int _cardId;
        public int _health;
        public Vector2Int _attack;
        public int _cost;

        public SerializableCardData(int id, int health, Vector2Int attack, int cost)
        {
            _cardId = id;
            _health = health;
            _attack = attack;
            _cost = cost;
        }

        public CardData ToCardData()
        {
            var original = CardDataProvider.DataBank.Get(_cardId);
            original.Attack = _attack;
            original.Health = _health;
            original.Cost = _cost;
            return original;
        }
    }
}