using System;
using System.Collections.Generic;
using Core.Cards.Card;
using Core.Cards.Card.Data;
using Core.SessionStorage;
using Other.Extensions;
using Player.Progression.SaveStates;
using UI.View.GameView;
using UnityEngine;

namespace Core.Cards.Hand
{
    public class PlayerHand : MonoBehaviour, IGameSerializable<SerializablePlayerHand>
    {
        [SerializeField] private PlayerStatView _statView;
        [SerializeField] private int _defaultHealth;

        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }

        [SerializeField] private int _defaultHope;
        
        public int MaxHope { get; private set; }
        public int CurrentHope { get; private set; }

        [SerializeField] private int _defaultHopeRegeneration;
        public int HopeRegeneration { get; private set; }
        
        [SerializeField] private int _defaultStartingHandSize;
        [SerializeField] private int _defaultCardDrawCount;
        public int StartingHandSize { get; private set; }
        public int DrawCount { get; private set; }

        public CardData[] Deck { get; private set; }
        public LinkedList<CardData> CurrentDeck { get; private set; }
        public List<CardData> CardsInHand { get; private set; }

        public bool HasAnyCards => CardsInHand.Count > 0 || CurrentDeck.Count > 0;
        public bool IsDefeated { get; private set; }

        public event Action PlayerDefeated; 

        public void Initialize(int[] cardIds)
        {
            Deck = CreateDeck(cardIds);
            CurrentDeck = new LinkedList<CardData>();
            CardsInHand = new List<CardData>();

            CurrentHealth = _defaultHealth;
            MaxHealth = _defaultHealth;

            CurrentHope = _defaultHope;
            MaxHope = _defaultHope;
            
            StartingHandSize = _defaultStartingHandSize;
            DrawCount = _defaultCardDrawCount;

            HopeRegeneration = _defaultHopeRegeneration;
            UpdateStatView(true);
        }

        public void SetStatView(PlayerStatView view) => _statView = view;

        public void UpdateStatView(bool isInstant)
        {
            _statView.SetHealth(CurrentHealth, MaxHealth, isInstant);
            _statView.SetHope(CurrentHope, MaxHope, isInstant);
        }

        public void ApplyBuffToCard(int index, Func<CardData, CardData> buff)
        {
            Deck[index] = buff(Deck[index]);
        }

        public void ResetAll()
        {
            CurrentDeck.Clear();
            CardsInHand.Clear();

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

        public void RegenerateHope() => AddHope(HopeRegeneration);

        public void SetHopeRestore(int newValue, bool allowZeroValue=false)
        {
            HopeRegeneration = newValue;
            if (HopeRegeneration <= 0)
            {
                HopeRegeneration = allowZeroValue ? 0 : 1;
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
            var card = CardsInHand[index];
            CardsInHand.RemoveAt(index);
            return card;
        }
        
        public CardData GetCardFromHand(CardData card)
        {
            var index = 0;
            for (var i = 0; i < CardsInHand.Count; i++)
            {
                if (CardsInHand[i] != card) continue;

                index = i;
                break;
            }
            CardsInHand.RemoveAt(index);
            return card;
        }

        public void RefillDeck()
        {
            CurrentDeck.Clear();
            foreach (var card in Deck) CurrentDeck.AddLast(card);
            CurrentDeck.Shuffle();
        }

        public CardData[] DrawCardsFromDeck(bool shuffleBeforeDraw=false) => 
            DrawCardsFromDeck(DrawCount, shuffleBeforeDraw);
        
        public CardData[] DrawCardsFromDeck(int count, bool shuffleBeforeDraw=false)
        {
            if (shuffleBeforeDraw) CurrentDeck.Shuffle();
            count = Mathf.Min(CurrentDeck.Count, count);
            var drawn = new CardData[count];
            var drawnIndex = 0;
            for (var i = 0; i < count; i++)
            {
                if (CurrentDeck.Count <= 0) break;
                
                var newCard = CurrentDeck.First.Value;
                CardsInHand.Add(newCard);
                CurrentDeck.RemoveFirst();
                
                drawn[drawnIndex] = newCard;
                drawnIndex++;
            }
            
            return drawn;
        }

        public void SetCardDrawCount(int newCount)
        {
            DrawCount = newCount < 0 ? 1 : newCount;
        }
        
        public void SetStartingHandSize(int newSize)
        {
            StartingHandSize = newSize < 0 ? 1 : newSize;
        }

        #endregion

        public SerializablePlayerHand SerializeSelf()
        {
            var deck = new SerializableCardData[Deck.Length];
            for (var i = 0; i < Deck.Length; i++) 
                deck[i] = Deck[i].SerializeSelf();
            
            var currentDeck = new SerializableCardData[CurrentDeck.Count];
            var index = 0;
            foreach (var card in CurrentDeck)
            {
                currentDeck[index] = card.SerializeSelf();
                index++;
            }
            
            var currentHand = new SerializableCardData[CardsInHand.Count];
            for (var i = 0; i < CardsInHand.Count; i++) 
                currentHand[i] = CardsInHand[i].SerializeSelf();

            return new SerializablePlayerHand(MaxHealth, CurrentHealth, MaxHope, CurrentHope,
                HopeRegeneration, StartingHandSize, DrawCount, deck, currentDeck, currentHand);
        }

        public void Deserialize(SerializablePlayerHand self)
        {
            MaxHealth = self._maxHealth;
            CurrentHealth = self._currentHealth;
            MaxHope = self._maxHope;
            CurrentHope = self._currentHope;
            
            StartingHandSize = self._startingHandSize;
            DrawCount = self._drawCount;
            HopeRegeneration = self._hopeRegeneration;

            Deck = new CardData[self._deck.Length];
            for (var i = 0; i < self._deck.Length; i++) 
                Deck[i].Deserialize(self._deck[i]);
            
            CurrentDeck = new LinkedList<CardData>();
            foreach (var card in self._currentDeck) 
                CurrentDeck.AddLast(card.ToCardData());

            CardsInHand = new List<CardData>();
            foreach (var card in self._hand)
                CardsInHand.Add(card.ToCardData());
        }
    }
}