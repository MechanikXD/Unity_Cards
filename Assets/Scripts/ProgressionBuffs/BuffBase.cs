using Cards.Card.Data;
using Cards.Hand;
using UnityEngine;

namespace ProgressionBuffs
{
    [CreateAssetMenu(fileName = "Buff", menuName = "ScriptableObjects/Buff/")]
    public abstract class BuffBase : ScriptableObject
    {
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _title;
        [SerializeField] private string _optionTitle;
        [SerializeField] private int _tier;
        [SerializeField] private ActivationType _activation;
        [SerializeField] private string _description;

        public Sprite Icon => _icon;
        public string Title => _title;
        public string OptionTitle => _optionTitle;
        public string Description => _description;
        public int Tier => _tier;
        public ActivationType Activation => _activation;
        public int ID { get; internal set; }

        public abstract void Apply(PlayerHand hand);

        protected virtual CardData Modify(CardData data) => default;
    }
}