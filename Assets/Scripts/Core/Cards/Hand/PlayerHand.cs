using System;
using System.Collections.Generic;
using Core.Cards.Card;
using Core.Cards.Card.Data;
using Other;
using UI.View;
using UI.View.GameView;
using UnityEngine;

namespace Core.Cards.Hand
{
    public class PlayerHand : MonoBehaviour
    {
        [SerializeField] private PlayerStatView _statView;
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
        
        [SerializeField] private int _defaultStartingHandSize;
        [SerializeField] private int _defaultCardDrawCount;
        public int StartingHandSize { get; private set; }
        private int _drawCount;

        private CardData[] _deck;
        private LinkedList<CardData> _currentDeck;
        private List<CardData> _hand;
        
        public List<CardData> CardsInHand => _hand;
        
        public bool HasAnyCards => _hand.Count > 0 || _currentDeck.Count > 0;
        public bool IsDefeated { get; private set; }

        public event Action PlayerDefeated; 

        public void Initialize(int[] cardIds)
        {
            _deck = CreateDeck(cardIds);
            _currentDeck = new LinkedList<CardData>();
            _hand = new List<CardData>();

            _health = _defaultHealth;
            _maxHealth = _defaultHealth;

            _hope = _defaultHope;
            _maxHope = _defaultHope;
            
            StartingHandSize = _defaultStartingHandSize;
            _drawCount = _defaultCardDrawCount;

            _hopeRegeneration = _defaultHopeRegeneration;
            
            _statView.SetHealth(_health, _maxHealth, true);
            _statView.SetHope(_hope, _maxHope, true);
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
            
            _statView.SetHope(_hope, _maxHope);
        }

        public void AddHope(int count)
        {
            _hope = Mathf.Min(_hope + count, _maxHope);
            _statView.SetHope(_hope, _maxHope);
        }

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
            
            _statView.SetHope(_hope, _maxHope);
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
                PlayerDefeated?.Invoke();
                IsDefeated = true;
                _health = 0;
            }
            
            _statView.SetHealth(_health, _maxHealth);
        }

        public void RestoreHealth(int amount)
        {
            _health = Mathf.Min(_health + amount, _maxHealth);
            _statView.SetHealth(_health, _maxHealth);
        }

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
            
            _statView.SetHealth(_health, _maxHealth);
        }

        #endregion

        #region Card Related

        private CardData[] CreateDeck(int[] ids)
        {
            var db = CardDataProvider.DataBank;
            var deck = new CardData[ids.Length];
            for (var i = 0; i < ids.Length; i++)
            {
                deck[i] = db.Get(ids[i]);
            }
            
            return deck;
        }
        
        public CardData GetCardFromHand(int index)
        {
            var card = _hand[index];
            _hand.RemoveAt(index);
            return card;
        }
        
        public CardData GetCardFromHand(CardData card)
        {
            var index = 0;
            for (var i = 0; i < _hand.Count; i++)
            {
                if (_hand[i] != card) continue;

                index = i;
                break;
            }
            _hand.RemoveAt(index);
            return card;
        }

        public void RefillDeck()
        {
            _currentDeck.Clear();
            foreach (var card in _deck) _currentDeck.AddLast(card);
            _currentDeck.Shuffle();
        }

        public CardData[] DrawCardsFromDeck(bool shuffleBeforeDraw=false) => 
            DrawCardsFromDeck(_drawCount, shuffleBeforeDraw);
        
        public CardData[] DrawCardsFromDeck(int count, bool shuffleBeforeDraw=false)
        {
            if (shuffleBeforeDraw) _currentDeck.Shuffle();
            count = Mathf.Min(_currentDeck.Count, count);
            var drawn = new CardData[count];
            var drawnIndex = 0;
            for (var i = 0; i < count; i++)
            {
                if (_currentDeck.Count <= 0) break;
                
                var newCard = _currentDeck.First.Value;
                _hand.Add(newCard);
                _currentDeck.RemoveFirst();
                
                drawn[drawnIndex] = newCard;
                drawnIndex++;
            }
            
            return drawn;
        }

        public void SetCardDrawCount(int newCount)
        {
            _drawCount = newCount < 0 ? 1 : newCount;
        }
        
        public void SetStartingHandSize(int newSize)
        {
            StartingHandSize = newSize < 0 ? 1 : newSize;
        }

        #endregion
    }
}