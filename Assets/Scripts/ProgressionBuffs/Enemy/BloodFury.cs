using Cards.Card.Data;
using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "BloodFury", menuName = "ScriptableObjects/Buff/Enemy/BloodFury")]
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
}