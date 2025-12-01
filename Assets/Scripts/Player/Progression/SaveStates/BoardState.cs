using System;
using System.IO;
using Core.Cards.Card.Data;
using UnityEditor;
using UnityEngine;

namespace Player.Progression.SaveStates
{
    [Serializable]
    public struct SerializableBoard
    {
        public SerializableCardData?[] PlayerCards;
        public SerializableCardData?[] EnemyCards;

        public SerializableBoard(SerializableCardData?[] playerCards, SerializableCardData?[] enemyCards)
        {
            PlayerCards = playerCards;
            EnemyCards = playerCards;
        }
    }

    [Serializable]
    public struct SerializableDialog
    {
        public string _spritePath;
        public int[] _options;
        public string[] _dialogs;
        public int _currentDialogIndex;

        public SerializableDialog(string spritePath, string[] dialogs, int[] options, int currentDialogIndex)
        {
            _spritePath = spritePath;
            _dialogs = dialogs;
            _options = options;
            _currentDialogIndex = currentDialogIndex;
        }

        public SerializableDialog(Sprite sprite, string[] dialogs, int[] options, int currentDialogIndex)
        {
            _spritePath = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sprite));
            _dialogs = dialogs;
            _options = options;
            _currentDialogIndex = currentDialogIndex;
        }
    }

    [Serializable]
    public struct SerializablePlayerHand
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
    }

    [Serializable]
    public struct SerializableCardData
    {
        private int _cardId;
        private int _health;
        private Vector2Int _attack;
        private int _cost;

        public SerializableCardData(int id, int health, Vector2Int attack, int cost)
        {
            _cardId = id;
            _health = health;
            _attack = attack;
            _cost = cost;
        }

        public void FromCardData(CardData data)
        {
            _cardId = data.ID;
            _health = data.Health;
            _attack = data.Attack;
            _cost = data.Cost;
        }

        public CardData ToCardData(CardDataBank db)
        {
            var origData = db.Get(_cardId);
            origData.Attack = _attack;
            origData.Health = _health;
            origData.Cost = _cost;
            return origData;
        }
    }
}