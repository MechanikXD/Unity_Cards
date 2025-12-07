#nullable enable
using System;
using Cards.Card;
using Cards.Card.Data;
using Cards.Hand;
using Dialogs;
using Newtonsoft.Json;
using ProgressionBuffs;
using UnityEngine;

namespace SaveLoad.Serializeables
{
    [Serializable]
    public class SerializableBoard
    {
        public SerializableCardData?[] _playerCards;
        public SerializableCardData?[] _enemyCards;
        public string _enemyDifficultyName;

        public SerializableBoard(SerializableCardData?[] playerCards, 
            SerializableCardData?[] enemyCards, string enemyDifficultyName)
        {
            _playerCards = playerCards;
            _enemyCards = enemyCards;
            _enemyDifficultyName = enemyDifficultyName;
        }
    }

    [Serializable]
    public class SerializableDialog
    {
        public string _spritePath;
        public string? _foregroundPath;
        public int[] _options;
        public string[] _dialogs;
        public int _currentDialogIndex;
        
        [JsonConstructor]
        public SerializableDialog(string spritePath, string? foregroundPath, string[] dialogs, 
            int[] options, int currentDialogIndex)
        {
            _spritePath = spritePath;
            _foregroundPath = foregroundPath;
            _dialogs = dialogs;
            _options = options;
            _currentDialogIndex = currentDialogIndex;
        }

        public SerializableDialog(DialogSettings settings)
        {
            _spritePath = settings.SpritePath;
            _foregroundPath = settings.ForegroundPath;
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

            return new DialogSettings(_spritePath, _foregroundPath, _dialogs, buffs);
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
        
        public SerializablePlayerHand(PlayerData data)
        {
            _maxHealth = data.MaxHealth;
            _currentHealth = data.CurrentHealth;
            _maxHope = data.MaxLight;
            _currentHope = data.CurrentLight;
            _hopeRegeneration = data.LightRegeneration;
            _startingHandSize = data.StartingHandSize;
            _drawCount = data.DrawCount;
            
            _deck = new SerializableCardData[data.Deck.Length];
            for (var i = 0; i < data.Deck.Length; i++) 
                _deck[i] = data.Deck[i].SerializeSelf();
            
            _currentDeck = new SerializableCardData[data.CurrentDeck.Count];
            var index = 0;
            foreach (var card in data.CurrentDeck)
            {
                _currentDeck[index] = card.SerializeSelf();
                index++;
            }
            
            _hand = new SerializableCardData[data.CardsInHand.Count];
            for (var i = 0; i < data.CardsInHand.Count; i++) 
                _hand[i] = data.CardsInHand[i].SerializeSelf();
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