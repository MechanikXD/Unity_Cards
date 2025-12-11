using Cards.Card;
using Cards.Card.Data;
using Cards.Card.Effects;
using Cards.Hand;
using Other.Extensions;
using Structure.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "Strength", menuName = "ScriptableObjects/Buff/Enemy/Strength")]
    public class Toughness : EnemyBuff
    {
        [SerializeField] private float _healthMultiply;
        
        public override void Apply(PlayerData data) => 
            data.SetMaxHealth((int)(data.MaxHealth * _healthMultiply));
    }
    
    public class Strength : EnemyBuff
    {
        [SerializeField] private int _targetCount;
        [SerializeField] private int _strengthCount;
        
        public override void Apply(PlayerData data)
        {
            var shuffled = data.Deck.ShuffledIndexes();
            
            for (var i = 0; i < Mathf.Min(_targetCount, shuffled.Length); i++)
            {
                data.ApplyBuffToCard(shuffled[i], Modify);
            }
        }

        protected override CardData Modify(CardData data)
        {
            var v2I = data.Attack;
            v2I.x += _strengthCount;
            v2I.y += _strengthCount;
            data.Attack = v2I;
            return data;
        }
    }

    public class ThickSkin : EnemyBuff
    {
        [SerializeField] private int _targetCount;
        [SerializeField] private int _healthBoost;
        
        public override void Apply(PlayerData data)
        {
            var shuffled = data.Deck.ShuffledIndexes();
            
            for (var i = 0; i < Mathf.Min(_targetCount, shuffled.Length); i++)
            {
                data.ApplyBuffToCard(shuffled[i], Modify);
            }
        }
        
        protected override CardData Modify(CardData data)
        {
            data.Health += _healthBoost;
            return data;
        }
    }

    public class Survivability : EnemyBuff
    {
        [SerializeField] private int _playerHealthBoost;
        [SerializeField] private int _healthRegeneration;
        
        public override void Apply(PlayerData data)
        {
            data.SetMaxHealth(data.MaxHealth + _playerHealthBoost);
            data.AddCombatStartEvent(RegeneratePLayerHealth);
        }

        private void RegeneratePLayerHealth(PlayerData data) => 
            data.RestoreHealth(_healthRegeneration);
    }

    public class Doppelgangers : EnemyBuff
    {
        [SerializeField] private int _cardDuplicateCount;
        
        public override void Apply(PlayerData data)
        {
            var db = CardDataProvider.DataBank;
            var shuffled = data.Deck.ShuffledIndexes();
            
            for (var i = 0; i < Mathf.Min(_cardDuplicateCount, shuffled.Length); i++)
            {
                data.CurrentDeck.AddLast(db.Get(shuffled[i]));
                data.ApplyBuffToCard(shuffled[i], Modify);
            }
        }
    }

    public class BloodFury : EnemyBuff
    {
        [SerializeField] private int _targetCount;
        [SerializeField] private int _strengthGain;
        
        public override void Apply(PlayerData data) => 
            data.AddCombatStartEvent(AddStrengthToRandomCard);

        private void AddStrengthToRandomCard(PlayerData data)
        {
            switch (Random.Range(0, 2))
            {
                case 0:
                    if (data.CurrentDeck.Count > 0)
                    {
                        var random = Random.Range(0, data.CurrentDeck.Count);
                        var i = 0;
                        foreach (var card in data.CurrentDeck)
                        {
                            if (i != random)
                            {
                                i++;
                                continue;
                            }

                            var find = data.CurrentDeck.Find(card);
                            if (find != null) find.Value = Modify(card);
                            break;
                        }
                    }
                    
                    break;
                case 1:
                    if (data.CardsInHand.Count > 0)
                    {
                        var random = Random.Range(0, data.CardsInHand.Count);
                        data.CardsInHand[random] = Modify(data.CardsInHand[random]);
                    }
                    
                    break;
            }
        }

        protected override CardData Modify(CardData data)
        {
            var v2I = data.Attack;
            v2I.x += _strengthGain;
            v2I.y += _strengthGain;
            data.Attack = v2I;
            return data;
        }
    }

    public class AddEffects : EnemyBuff
    {
        [SerializeField] private CardEffect _effect;
        [SerializeField] private TriggerType _trigger;
        [SerializeField] private int _targetCount;
        
        public override void Apply(PlayerData data)
        {
            var shuffled = data.Deck.ShuffledIndexes();
            
            for (var i = 0; i < Mathf.Min(_targetCount, shuffled.Length); i++) 
                data.Deck[shuffled[i]].AddEffect(_trigger, _effect);
        }
    }
    
    public class AddEffectToAll : EnemyBuff
    {
        [SerializeField] private CardEffect _effect;
        [SerializeField] private TriggerType _trigger;
        
        public override void Apply(PlayerData data)
        {
            foreach (var card in data.Deck) card.AddEffect(_trigger, _effect);
        }
    }

    public class HastyDraw : EnemyBuff
    {
        [SerializeField] private int _drawCountBonus;
        
        public override void Apply(PlayerData data)
        {
            data.SetCardDrawCount(data.DrawCount + _drawCountBonus);
        }
    }

    public class DeadHand : EnemyBuff
    {
        [SerializeField] private CardData _cardData;
        [SerializeField] private int _cardCount;
        
        public override void Apply(PlayerData data)
        {
            var newCards = new CardData[_cardCount];
            for (var i = 0; i < _cardCount; i++) newCards[i] = _cardData;
            
            SessionManager.Instance.PlayerData.Deck.AddRange(newCards);
        }
    }

    public class Adaptation : EnemyBuff
    {
        [SerializeField] private int _strongestHealthBoost;
        [SerializeField] private int _weakestAttackBoost;
        
        public override void Apply(PlayerData data)
        {
            var weakestIndex = 0;
            var weakestAttack = float.MaxValue;
            var strongestIndex = 0;
            var strongestAttack = -1.0;
            
            for (var i = 0; i < data.Deck.Length; i++)
            {
                var average = data.Deck[i].Attack.Average();
                if (average < weakestAttack)
                {
                    weakestAttack = average;
                    weakestIndex = i;
                }
                if (average > strongestAttack)
                {
                    strongestAttack = average;
                    strongestIndex = i;
                }
            }

            data.Deck[strongestIndex].Health += _strongestHealthBoost;
            var v2I = data.Deck[weakestIndex].Attack;
            v2I.x += _weakestAttackBoost;
            v2I.y += _weakestAttackBoost;
            data.Deck[weakestIndex].Attack = v2I;
        }
    }

    public class DimmingAura : EnemyBuff
    {
        [SerializeField] private int _playerCostIncrease;
        
        public override void Apply(PlayerData data)
        {
            var playerDeck = SessionManager.Instance.PlayerData.Deck;
            for (var i = 0; i < playerDeck.Length; i++) playerDeck[i].Cost += _playerCostIncrease;
        }
    }
}