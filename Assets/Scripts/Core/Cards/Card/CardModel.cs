using System.Threading;
using Core.Cards.Board;
using Core.Cards.Card.Data;
using Core.Cards.Hand;
using Cysharp.Threading.Tasks;
using Other;
using Other.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Core.Cards.Card
{
    public class CardModel : MonoBehaviour
    {
        private const string EMPTY_FINAL_ATTACK_CHAR = "#";
        private CancellationTokenSource _cts = new CancellationTokenSource();
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private Animator _animator;
        [SerializeField] private string[] _cardAnimations = {
            "Forward Attack", "Left Attack", "Right Attack"
        };
        [SerializeField] private string _reverseAnimationSuffix = " Reverse";
        [SerializeField] private float _liftSpeed = 3f;
        [SerializeField] private Vector3 _moveStartLift = new Vector3(0f, 0f, 1f);
        [SerializeField] private GameObject _buttonRoot;
        [SerializeField] private PseudoButton _destroyButton;
        [SerializeField] private PseudoButton _moveButton;
        
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
        public PlayerHand Hand { get; private set; }
        public int FinalAttack { get; private set; }
        public CardData Data { get; private set; }
        public int CurrentHealth { get; private set; }
        public int IndexInLayout { get; set; }
        public bool IsDefeated { get; private set; }
        public bool RequestMove { get; private set; }
        public bool CanBePlaced => Hand.CanUseCard(Data.Cost);
        public Animator Animator => _animator;
        public SortingGroup SortingGroup => _sortingGroup;

        private void Awake()
        {
            _animator.enabled = false;
            HideActions();
        }

        private void OnEnable()
        {
            _destroyButton.AddListener(DestroyCard);
            _moveButton.AddListener(StartMove);
        }

        public void Set(CardData data, PlayerHand owner)
        {
            Data = data;
            _sprite.sprite = data.Sprite;
            _background.sprite = data.Background;
            _descriptionField.SetText(CardDataProvider.MakeDescription(data));
            _affinityImage.sprite = CardDataProvider.GetAffinitySprite(data.Affinity);
            
            _attackField.SetText(CardDataProvider.AttackToString(data.Attack));
            _costField.SetText(data.Cost.ToString());
            _healthField.SetText(data.Health.ToString());
            CurrentHealth = data.Health;
            
            Hand = owner;
        }

        public void SetPlaced()
        {
            Hand.GetCardFromHand(Data);
            Hand.UseHope(Data.Cost);
            Hand = null;
            _animator.enabled = true;
        }

        public void SetRandomFinalAttack()
        {
            var attackRange = Data.Attack;
            var final = Random.Range(attackRange.x, attackRange.y + 1);
            SetFinalAttack(final);
        }
        
        public void SetFinalAttack(int damage)
        {
            FinalAttack = damage;
            _finalAttackField.SetText(damage.ToString());
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                IsDefeated = true;
                CurrentHealth = 0;
            }
            _healthField.SetText(CurrentHealth.ToString());
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

            Data = default;
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

        public void ShowActions()
        {
            if (RequestMove) return;
            _buttonRoot.SetActive(true);
        }

        public void HideActions() => _buttonRoot.SetActive(false);

        private void DestroyCard()
        {
            foreach (var slot in GameManager.Instance.Board.PlayerSlots)
            {
                if (slot.Card != this) continue;

                slot.Detach();
                break;
            }
            Destroy(gameObject);
        }

        private void StartMove()
        {
            RequestMove = true;
            HideActions();
            MoveToLocalAsync(transform.localPosition + _moveStartLift, _liftSpeed, reenableAnimator:false).Forget();
        }
        
        public void MoveCard(CardSlot newSlot)
        {
            if (!newSlot.IsEmpty || !newSlot.CanAttach)
            {
                CancelMove();
                return;
            }
            newSlot.Attach(this);
        }

        public void CancelMove()
        {
            MoveToLocalAsync(transform.localPosition - _moveStartLift, _liftSpeed).Forget();
            HideActions();
        }
        
        public async UniTask MoveToLocalAsync(Vector3 final, float moveSpeed, float snapDistance = 0.1f, bool reenableAnimator=true)
        {
            _cts = _cts.Reset();
            _animator.enabled = false;
            var current = transform.localPosition;
            while (Vector3.Distance(current, final) > snapDistance)
            {
                current = Vector3.Lerp(current, final, moveSpeed * Time.deltaTime);
                transform.localPosition = current;
                await UniTask.NextFrame(_cts.Token);
            }

            transform.localPosition = final;
            if (reenableAnimator)
            {
                RequestMove = false;
                _animator.enabled = true;
            }
        }

        private void OnDestroy() => _cts.Cancel();
    }   
}