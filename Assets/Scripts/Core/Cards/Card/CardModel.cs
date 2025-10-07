using Core.Cards.Card.Data;
using Core.Cards.Hand;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Cards.Card
{
    public class CardModel : MonoBehaviour
    {
        private const string EMPTY_FINAL_ATTACK_CHAR = "#";
        
        [Header("Visual")]
        [SerializeField] private Image _sprite;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _descriptionField;
        [SerializeField] private Image _affinityImage;
        [Header("Informative")]
        [SerializeField] private TMP_Text _costField;
        [SerializeField] private TMP_Text _attackField;
        [SerializeField] private TMP_Text _healthField;
        [Header("Dynamic")]
        [SerializeField] private TMP_Text _finalAttackField;
        public int FinalAttack { get; private set; }
        private PlayerHand _hand;
        private CardData _data;

        public CardData CardData => _data;
        public bool CanBePlaced => _hand.CanUseCard(_data.Cost);
        
        public void Set(CardData data, PlayerHand owner)
        {
            _data = data;
            _sprite.sprite = data.Sprite;
            _background.sprite = data.Background;
            _descriptionField.SetText(CardDataProvider.MakeDescription(data));
            _affinityImage.sprite = CardDataProvider.GetAffinitySprite(data.Affinity);
            
            _attackField.SetText(CardDataProvider.AttackToString(data.Attack));
            _costField.SetText(data.Cost.ToString());
            _healthField.SetText(data.Health.ToString());
            
            _hand = owner;
        }

        public void SetPlaced()
        {
            _hand.GetCardFromHand(_data);
            _hand.UseHope(_data.Cost);
            _hand = null;
        }

        public void SetRandomFinalAttack()
        {
            var attackRange = _data.Attack;
            var final = Random.Range(attackRange.x, attackRange.y);
            SetFinalAttack(final);
        }
        
        public void SetFinalAttack(int damage)
        {
            FinalAttack = damage;
            _finalAttackField.SetText(damage.ToString());
        }

        public void Clear()
        {
            var imageNull = CardDataProvider.ImageNull;
            _sprite.sprite = imageNull;
            _background.sprite = imageNull;
            _descriptionField.SetText(string.Empty);
            _affinityImage.sprite = imageNull;
            
            _attackField.SetText(string.Empty);
            _costField.SetText(string.Empty);
            _healthField.SetText(string.Empty);
            ClearFinalAttack();

            _data = default;
        }
        
        public void ClearFinalAttack()
        {
            FinalAttack = -1;
            _finalAttackField.SetText(EMPTY_FINAL_ATTACK_CHAR);
        }
    }
}