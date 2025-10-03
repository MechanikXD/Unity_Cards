using Core.Cards.Card.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Cards.Card
{
    public class CardModel : MonoBehaviour
    {
        private const string EMPTY_FINAL_ATTACK_CHAR = "#";
        
        [Header("Visual")]
        [SerializeField] private Image _mainImage;
        [SerializeField] private TMP_Text _descriptionField;
        [SerializeField] private Image _affinityImage;
        [Header("Informative")]
        [SerializeField] private TMP_Text _costField;
        [SerializeField] private TMP_Text _attackField;
        [SerializeField] private TMP_Text _healthField;
        [Header("Dynamic")]
        [SerializeField] private TMP_Text _finalAttackField;
        public int FinalAttack { get; private set; }
        private int _cardReferenceId;

        // TODO: Get databank from game manager of sort
        public CardData GetCard(CardDataBank source) => source.Get(_cardReferenceId);
        
        public void Set(CardData data)
        {
            _mainImage.sprite = data.Image;
            _descriptionField.SetText(CardDataProvider.MakeDescription(data));
            _affinityImage.sprite = CardDataProvider.GetAffinitySprite(data.Affinity);
            
            _attackField.SetText(CardDataProvider.AttackToString(data.Attack));
            _costField.SetText(data.Cost.ToString());
            _healthField.SetText(data.Health.ToString());
            
            _cardReferenceId = data.ID;
        }

        public void SetRandomFinalAttack(CardDataBank source)
        {
            var attackRange = source.Get(_cardReferenceId).Attack;
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
            _mainImage.sprite = null;
            _descriptionField.SetText(string.Empty);
            _affinityImage.sprite = null;
            
            _attackField.SetText(string.Empty);
            _costField.SetText(string.Empty);
            _healthField.SetText(string.Empty);
            ClearFinalAttack();

            _cardReferenceId = -1;
        }
        
        public void ClearFinalAttack()
        {
            FinalAttack = -1;
            _finalAttackField.SetText(EMPTY_FINAL_ATTACK_CHAR);
        }
    }
}