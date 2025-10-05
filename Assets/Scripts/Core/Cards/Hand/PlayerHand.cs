using System.Collections.Generic;
using Core.Cards.Card.Data;
using Other;
using UnityEngine;

namespace Core.Cards.Hand
{
    public class PlayerHand : MonoBehaviour
    {
        [SerializeField] private int _defaultHealth;
        private int _health;
        private int _maxHealth;
        
        public int CurrentHealth => _health;

        [SerializeField] private int _defaultHope;
        private int _hope;
        private int _maxHope;
        
        public int CurrentHope => _hope;
        
        [SerializeField] private int _defaultHopeRegeneration;
        private int _hopeRegeneration;
        
        private CardData[] _deck;
        private LinkedList<CardData> _currentDeck;
        private List<CardData> _hand;

        public void Initialize(CardData[] deck)
        {
            _deck = deck;
            _currentDeck = new LinkedList<CardData>();
            _hand = new List<CardData>();

            _health = _defaultHealth;
            _maxHealth = _defaultHealth;

            _hope = _defaultHope;
            _maxHope = _defaultHope;

            _hopeRegeneration = _defaultHopeRegeneration;
        }

        #region Hope Related

        public bool CanUseCard(int cardCost) => cardCost <= _hope;

        public void UseHope(int cardCost)
        {
            if (_hope < cardCost)
            {
                Debug.LogWarning("Hope will fall below 0! Check ability to use this card prior");
                _hope = 0;
            }
            else
            {
                _hope -= cardCost;
            }
        }

        public void AddHope(int count) => _hope = Mathf.Min(_hope + count, _maxHope);

        public void SetMaxHope(int newValue)
        {
            if (newValue <= 0)
            {
                Debug.LogWarning("Max Hope can't be less than or equal to zero!");
                _maxHope = 1;
                _hope = 1;
            }
            else
            {
                var oldMaxHope = _maxHope;
                _maxHope = newValue;

                if (oldMaxHope > _maxHope) _hope = Mathf.Min(oldMaxHope, _hope);
                else _hope += _maxHope - oldMaxHope;
            }
        }

        public void RegenerateHope() => AddHope(_hopeRegeneration);

        public void SetHopeRestore(int newValue, bool allowZeroValue=false)
        {
            _hopeRegeneration = newValue;
            if (_hopeRegeneration <= 0)
            {
                _hopeRegeneration = allowZeroValue ? 0 : 1;
            }
        }

        #endregion

        #region Health Related

        public void TakeDamage(int damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                // TODO: Defeat sequence
                _health = 0;
            }
        }

        public void RestoreHealth(int amount) => _health = Mathf.Min(_health + amount, _maxHealth);

        public void SetMaxHealth(int newValue)
        {
            if (newValue <= 0)
            {
                Debug.LogWarning("Max Health can't be less than or equal to zero!");
                _maxHealth = 1;
                _health = 1;
            }
            else
            {
                var oldMaxHealth = _maxHealth;
                _maxHealth = newValue;

                if (oldMaxHealth > _maxHealth) _health = Mathf.Min(oldMaxHealth, _health);
                else _health += _maxHealth - oldMaxHealth;
            }
        }

        #endregion

        #region Card Related

        public CardData GetCardFromHand(int index)
        {
            var card = _hand[index];
            _hand.RemoveAt(index);
            return card;
        }

        public void RefillDeck()
        {
            _currentDeck.Clear();
            foreach (var card in _deck) _currentDeck.AddLast(card);
            ShuffleDeck();
        }

        public void DrawCardsFromDeck(int count, bool shuffleBeforeDraw=false)
        {
            if (shuffleBeforeDraw) ShuffleDeck();
            for (var i = 0; i < count; i++)
            {
                if (_currentDeck.Count <= 0) break;
                
                _hand.Add(_currentDeck.First.Value);
                _currentDeck.RemoveFirst();
            }
        }

        private void ShuffleDeck() => _currentDeck = (LinkedList<CardData>)_currentDeck.Shuffle();

        #endregion
    }
}