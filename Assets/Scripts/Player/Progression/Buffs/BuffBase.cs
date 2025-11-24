using Core.Cards.Card.Data;
using Core.Cards.Hand;
using UnityEngine;

namespace Player.Progression.Buffs
{
    [CreateAssetMenu(fileName = "Buff", menuName = "ScriptableObjects/Buff/")]
    public abstract class BuffBase : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField] private int _tier;
        [SerializeField] private ActivationType _activation;
        [SerializeField] private string _description;
        public string Title => _title;
        public string Description => _description;
        public int Tier => _tier;
        public ActivationType Activation => _activation;
        public int ID { get; internal set; }

        public abstract void Apply(PlayerHand hand);

        protected virtual CardData Modify(CardData data)
        {
            return default;
        }
    }
}