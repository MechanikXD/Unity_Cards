using System;
using System.IO;
using Core.Cards.Card;
using Core.Cards.Card.Data;
using Core.Cards.Hand;
using Other.Dialog;
using Player.Progression.Buffs;
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

        public SerializableDialog(DialogSettings settings)
        {
            _spritePath = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(settings.Cgi));
            _dialogs = settings.Dialogues;
            _options = new int[settings.Options.Count];
            for (var i = 0; i < settings.Options.Count; i++) _options[i] = settings.Options[i].ID;
            _currentDialogIndex = 0;
        }

        public DialogSettings ToDialogSetting(BuffDataBase db)
        {
            var sprite = Resources.Load<Sprite>(_spritePath);
            var buffs = new BuffBase[_options.Length];
            for (var i = 0; i < _options.Length; i++)
            {
                buffs[i] = db.Get<BuffBase>(_options[i]);
            }

            return new DialogSettings(sprite, _dialogs, buffs);
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
            for (var i = 0; i < hand.Deck.Length; i++) 
                _hand[i] = hand.CardsInHand[i].SerializeSelf();
        }
    }

    [Serializable]
    public struct SerializableCardData
    {
        public int CardId { get; }
        public int Health { get; }
        public Vector2Int Attack{ get; }
        public int Cost{ get; }

        public SerializableCardData(int id, int health, Vector2Int attack, int cost)
        {
            CardId = id;
            Health = health;
            Attack = attack;
            Cost = cost;
        }

        public CardData ToCardData()
        {
            var original = CardDataProvider.DataBank.Get(CardId);
            original.Attack = Attack;
            original.Health = Health;
            original.Cost = Cost;
            return original;
        }
    }
}