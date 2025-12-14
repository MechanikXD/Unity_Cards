using System;
using System.Collections.Generic;
using Cards.Card;
using Cards.Card.Data;
using Other.Extensions;
using SaveLoad;
using SaveLoad.Serializeables;
using UI.View.GameView;
using UnityEngine;

namespace Cards.Hand
{
    /// <summary>
    /// PlayerData represents one of the players with deck, health and "light".
    /// </summary>
    public class PlayerData : MonoBehaviour, IGameSerializable<SerializablePlayerHand>
    {
        [SerializeField] private PlayerStatView _statView; // To display values within this class
        //          Health
        [SerializeField] private int _defaultHealth;
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        //          Light
        [SerializeField] private int _defaultLight;
        public int MaxLight { get; private set; } // Max amount of "Light"
        public int CurrentLight { get; private set; } // Currency used to play cards

        [SerializeField] private int _defaultLightRegeneration;
        public int LightRegeneration { get; private set; } // Amount of "Light" that will be added each turn
        //          Card Draw
        [SerializeField] private int _defaultStartingHandSize;
        [SerializeField] private int _defaultCardDrawCount;
        public int StartingHandSize { get; private set; } // Amount of cards in hand at the start of the game
        public int DrawCount { get; private set; } // Amount of cards added per turn

        // All cards in this player hand; INTERACT AS IF ITS READONLY, UNLESS CHANGED WITH BUFFS
        public CardData[] Deck { get; private set; } 
        // Deck that player has draw cards from during the game
        public LinkedList<CardData> CurrentDeck { get; private set; }
        // Cards that player can play (as in player hand)
        public List<CardData> CardsInHand { get; private set; }

        public bool HasAnyCards => CardsInHand.Count > 0 || CurrentDeck.Count > 0;
        public bool IsDefeated { get; private set; }
        private readonly LinkedList<Action<PlayerData>> _combatStartEvents = new LinkedList<Action<PlayerData>>();
        public event Action PlayerDefeated; 
        
        /// <summary>
        /// Use to initialize this class, all values are taken from serialized fields.
        /// </summary>
        /// <param name="cardIds"> the deck of this player </param>
        public void Initialize(int[] cardIds)
        {
            Deck = CreateDeck(cardIds);
            CurrentDeck = new LinkedList<CardData>();
            CardsInHand = new List<CardData>();

            CurrentHealth = _defaultHealth;
            MaxHealth = _defaultHealth;

            CurrentLight = _defaultLight;
            MaxLight = _defaultLight;
            
            StartingHandSize = _defaultStartingHandSize;
            DrawCount = _defaultCardDrawCount;

            LightRegeneration = _defaultLightRegeneration;
            UpdateStatView(true);
        }
        
        // Workaround, because player hand located on a class that migrate between scenes.
        public void SetStatView(PlayerStatView view) => _statView = view;
        
        /// <summary> Update attached StatView of this player  </summary>
        /// <param name="isInstant"> update instantly (no smoothing) </param>
        public void UpdateStatView(bool isInstant)
        {
            _statView.SetHealth(CurrentHealth, MaxHealth, isInstant);
            _statView.SetLight(CurrentLight, MaxLight, isInstant);
        }

        // Used to apply buffs to cards, because cards are structs, and we don't want to modify they elsewhere
        public void ApplyBuffToCard(int index, Func<CardData, CardData> buff)
        {
            Deck[index] = buff(Deck[index]);
        }

        /// <summary>
        /// Resets player current deck, hand and light (use on act end for clean up)
        /// </summary>
        public void Reset()
        {
            CurrentDeck.Clear();
            CardsInHand.Clear();

            CurrentLight = MaxLight;
            _combatStartEvents.Clear();
            
            if (_statView != null) _statView.SetLight(CurrentLight, MaxLight, true);
        }

        // Methods to modify light-related values within this class
        #region Light Related

        public bool CanUseCard(int cardCost) => cardCost <= CurrentLight;

        public void UseLight(int cardCost)
        {
            if (CurrentLight < cardCost)
            {
                Debug.LogWarning("Hope will fall below 0! Check ability to use this card prior");
                CurrentLight = 0;
            }
            else
            {
                CurrentLight -= cardCost;
            }
            
            if (_statView != null) _statView.SetLight(CurrentLight, MaxLight);
        }

        public void AddLight(int count)
        {
            CurrentLight = Mathf.Min(CurrentLight + count, MaxLight);
            if (_statView != null) _statView.SetLight(CurrentLight, MaxLight);
        }

        public void SetMaxLight(int newValue)
        {
            if (newValue <= 0)
            {
                Debug.LogWarning("Max Hope can't be less than or equal to zero!");
                MaxLight = 1;
                CurrentLight = 1;
            }
            else
            {
                var oldMaxHope = MaxLight;
                MaxLight = newValue;

                if (oldMaxHope > MaxLight) CurrentLight = Mathf.Min(oldMaxHope, CurrentLight);
                else CurrentLight += MaxLight - oldMaxHope;
            }
            
            if (_statView != null) _statView.SetLight(CurrentLight, MaxLight);
        }

        public void RegenerateLight() => AddLight(LightRegeneration);

        public void SetLightRestore(int newValue, bool allowZeroValue=false)
        {
            LightRegeneration = newValue;
            if (LightRegeneration <= 0)
            {
                LightRegeneration = allowZeroValue ? 0 : 1;
            }
        }

        public void HighLightUi() => _statView.HighLight();

        #endregion
        // Methods to modify health-related values within this class
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
        // Methods to modify card-related values within this class
        #region Card Related

        // Creates array of cards from it ids
        private static CardData[] CreateDeck(int[] ids)
        {
            var db = CardDataProvider.DataBank;
            var deck = new CardData[ids.Length];
            for (var i = 0; i < ids.Length; i++)
            {
                deck[i] = db.Get(ids[i]);
            }
            
            return deck;
        }
        
        /// <summary>
        /// Take and remove card from players hand at index
        /// </summary>
        /// <param name="index"> Index of the card </param>
        public CardData GetCardFromHand(int index)
        {
            var card = CardsInHand[index];
            CardsInHand.RemoveAt(index);
            return card;
        }
        
        /// <summary>
        /// Take and remove card from players hand using data of this card
        /// </summary>
        /// <param name="card"></param>
        public CardData GetCardFromHand(CardData card)
        {
            var index = -1;
            for (var i = 0; i < CardsInHand.Count; i++)
            {
                if (CardsInHand[i] != card) continue;

                index = i;
                break;
            }

            if (index == -1) return default;    // No card found 
            var result = CardsInHand[index];
            CardsInHand.RemoveAt(index);
            return result;
        }

        /// <summary>  Add all cards from deck to current deck  </summary>
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

        public void DoCombatStartEvents()
        {
            foreach (var action in _combatStartEvents) action(this);
        }

        public void AddCombatStartEvent(Action<PlayerData> action) => 
            _combatStartEvents.AddLast(action);

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

            return new SerializablePlayerHand(MaxHealth, CurrentHealth, MaxLight, CurrentLight,
                LightRegeneration, StartingHandSize, DrawCount, deck, currentDeck, currentHand);
        }

        public void Deserialize(SerializablePlayerHand self)
        {
            MaxHealth = self._maxHealth;
            CurrentHealth = self._currentHealth;
            MaxLight = self._maxHope;
            CurrentLight = self._currentHope;
            
            StartingHandSize = self._startingHandSize;
            DrawCount = self._drawCount;
            LightRegeneration = self._hopeRegeneration;

            Deck = new CardData[self._deck.Length];
            for (var i = 0; i < self._deck.Length; i++) 
                Deck[i] = self._deck[i].ToCardData();
            
            CurrentDeck = new LinkedList<CardData>();
            foreach (var card in self._currentDeck) 
                CurrentDeck.AddLast(card.ToCardData());

            CardsInHand = new List<CardData>();
            foreach (var card in self._hand)
                CardsInHand.Add(card.ToCardData());
        }
    }
}