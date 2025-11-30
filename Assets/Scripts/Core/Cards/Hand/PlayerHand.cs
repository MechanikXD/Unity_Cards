using System;
using System.Collections.Generic;
using Core.Cards.Card;
using Core.Cards.Card.Data;
using Other.Extensions;
using UI.View.GameView;
using UnityEngine;

namespace Core.Cards.Hand
{
    public class PlayerHand : MonoBehaviour
    {
        [SerializeField] private PlayerStatView _statView;
        [SerializeField] private int _defaultHealth;

        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }

        [SerializeField] private int _defaultHope;
        
        public int MaxHope { get; private set; }
        public int CurrentHope { get; private set; }

        [SerializeField] private int _defaultHopeRegeneration;
        private int _hopeRegeneration;
        
        [SerializeField] private int _defaultStartingHandSize;
        [SerializeField] private int _defaultCardDrawCount;
        public int StartingHandSize { get; private set; }
        private int _drawCount;

        private CardData[] _deck;
        private LinkedList<CardData> _currentDeck;
        private List<CardData> _hand;

        public CardData[] Deck => _deck;
        public LinkedList<CardData> CurrentDeck => _currentDeck;
        public List<CardData> CardsInHand => _hand;
        
        public bool HasAnyCards => _hand.Count > 0 || _currentDeck.Count > 0;
        public bool IsDefeated { get; private set; }

        public event Action PlayerDefeated; 

        public void Initialize(int[] cardIds)
        {
            _deck = CreateDeck(cardIds);
            _currentDeck = new LinkedList<CardData>();
            _hand = new List<CardData>();

            CurrentHealth = _defaultHealth;
            MaxHealth = _defaultHealth;

            CurrentHope = _defaultHope;
            MaxHope = _defaultHope;
            
            StartingHandSize = _defaultStartingHandSize;
            _drawCount = _defaultCardDrawCount;

            _hopeRegeneration = _defaultHopeRegeneration;
            
            _statView.SetHealth(CurrentHealth, MaxHealth, true);
            _statView.SetHope(CurrentHope, MaxHope, true);
        }

        public void SetStatView(PlayerStatView view) => _statView = view;

        public void UpdateStatView(bool isInstant)
        {
            _statView.SetHealth(CurrentHealth, MaxHealth, isInstant);
            _statView.SetHope(CurrentHope, MaxHope, isInstant);
        }

        public void ApplyBuffToCard(int index, Func<CardData, CardData> buff)
        {
            _deck[index] = buff(_deck[index]);
        }

        public void ResetAll()
        {
            _currentDeck.Clear();
            _hand.Clear();

            CurrentHope = MaxHope;
            
            if (_statView != null) _statView.SetHope(CurrentHope, MaxHope, true);
        }

        #region Hope Related

        public bool CanUseCard(int cardCost) => cardCost <= CurrentHope;

        public void UseHope(int cardCost)
        {
            if (CurrentHope < cardCost)
            {
                Debug.LogWarning("Hope will fall below 0! Check ability to use this card prior");
                CurrentHope = 0;
            }
            else
            {
                CurrentHope -= cardCost;
            }
            
            if (_statView != null) _statView.SetHope(CurrentHope, MaxHope);
        }

        public void AddHope(int count)
        {
            CurrentHope = Mathf.Min(CurrentHope + count, MaxHope);
            if (_statView != null) _statView.SetHope(CurrentHope, MaxHope);
        }

        public void SetMaxHope(int newValue)
        {
            if (newValue <= 0)
            {
                Debug.LogWarning("Max Hope can't be less than or equal to zero!");
                MaxHope = 1;
                CurrentHope = 1;
            }
            else
            {
                var oldMaxHope = MaxHope;
                MaxHope = newValue;

                if (oldMaxHope > MaxHope) CurrentHope = Mathf.Min(oldMaxHope, CurrentHope);
                else CurrentHope += MaxHope - oldMaxHope;
            }
            
            if (_statView != null) _statView.SetHope(CurrentHope, MaxHope);
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
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                PlayerDefeated?.Invoke();
                IsDefeated = true;
                CurrentHealth = 0;
            }
            
            if (_statView != null) _statView.SetHealth(CurrentHealth, MaxHealth);
        }

        public void RestoreHealth(int amount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            if (_statView != null) _statView.SetHealth(CurrentHealth, MaxHealth);
        }

        public void SetMaxHealth(int newValue)
        {
            if (newValue <= 0)
            {
                Debug.LogWarning("Max Health can't be less than or equal to zero!");
                MaxHealth = 1;
                CurrentHealth = 1;
            }
            else
            {
                var oldMaxHealth = MaxHealth;
                MaxHealth = newValue;

                if (oldMaxHealth > MaxHealth) CurrentHealth = Mathf.Min(oldMaxHealth, CurrentHealth);
                else CurrentHealth += MaxHealth - oldMaxHealth;
            }
            
            if (_statView != null) _statView.SetHealth(CurrentHealth, MaxHealth);
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