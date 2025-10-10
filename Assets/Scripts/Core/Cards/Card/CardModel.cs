using Core.Cards.Card.Data;
using Core.Cards.Hand;
using Other;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Core.Cards.Card
{
    public class CardModel : MonoBehaviour
    {
        private const string EMPTY_FINAL_ATTACK_CHAR = "#";
        [SerializeField] private Animator _animator;
        [SerializeField] private string[] _cardAnimations = {
            "Forward Attack", "Left Attack", "Right Attack"
        };
        
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
        private RectTransform _rectTransform;
        
        public int IndexInLayout { get; set; }
        public RectTransform RectTransform => _rectTransform;
        public CardData CardData => _data;
        public bool CanBePlaced => _hand.CanUseCard(_data.Cost);

        private void Awake()
        {
            _animator.enabled = false;
            _rectTransform = (RectTransform)transform;
        }

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
            
            _animator.enabled = true;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
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

        public void PlayRandomAnimation()
        {
            _animator.Play(_cardAnimations.GetRandom(), -1, 0f);
        }

        public void PlayAnimation(string animationName)
        {
            _animator.Play(animationName, -1, 0f);
        }
    }
}