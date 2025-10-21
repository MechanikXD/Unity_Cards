using Core.Cards.Card.Data;
using Core.Cards.Hand;
using Other;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Core.Cards.Card
{
    public class CardModel : MonoBehaviour
    {
        private const string EMPTY_FINAL_ATTACK_CHAR = "#";
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private Animator _animator;
        [SerializeField] private string[] _cardAnimations = {
            "Forward Attack", "Left Attack", "Right Attack"
        };
        [SerializeField] private string _reverseAnimationSuffix = " Reverse";
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private SpriteRenderer _background;
        [SerializeField] private TMP_Text _descriptionField;
        [SerializeField] private SpriteRenderer _affinityImage;
        [Header("Informative")]
        [SerializeField] private TMP_Text _costField;
        [SerializeField] private TMP_Text _attackField;
        [SerializeField] private TMP_Text _healthField;
        [Header("Dynamic")]
        [SerializeField] private TMP_Text _finalAttackField;
        public int FinalAttack { get; private set; }
        private PlayerHand _hand;
        private CardData _data;
        private int _currentHealth;
        
        public int IndexInLayout { get; set; }
        public CardData CardData => _data;
        public bool CanBePlaced => _hand.CanUseCard(_data.Cost);
        public bool IsDefeated { get; private set; }
        public Animator Animator => _animator;
        public SortingGroup SortingGroup => _sortingGroup;

        private void Awake()
        {
            _animator.enabled = false;
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
            _currentHealth = data.Health;
            
            _hand = owner;
        }

        public void SetPlaced()
        {
            _hand.GetCardFromHand(_data);
            _hand.UseHope(_data.Cost);
            _hand = null;
            _animator.enabled = true;
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

        public void TakeDamage(int damage)
        {
            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                IsDefeated = true;
                _currentHealth = 0;
            }
            _healthField.SetText(_currentHealth.ToString());
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
            PlayAnimation(_cardAnimations.GetRandom());
        }

        public void PlayRandomAnimationReverse()
        {
            PlayAnimation(_cardAnimations.GetRandom() + _reverseAnimationSuffix);
        }

        public void PlayAnimation(string animationName)
        {
            _animator.Play(animationName, -1, 0f);
        }
    }
}