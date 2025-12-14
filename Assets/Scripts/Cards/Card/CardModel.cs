using System;
using System.Collections.Generic;
using System.Threading;
using Cards.Board;
using Cards.Card.Data;
using Cards.Hand;
using Cysharp.Threading.Tasks;
using Other.Buttons;
using Other.Extensions;
using Structure.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Cards.Card
{
    public class CardModel : MonoBehaviour
    {
        private const string EMPTY_FINAL_ATTACK_CHAR = "#";
        private CancellationTokenSource _cts = new CancellationTokenSource();
        [SerializeField] private CardController _controller;
        [SerializeField] private SortingGroup _sortingGroup;
        // Animator stuff
        [SerializeField] private Animator _animator;
        [SerializeField] private string[] _cardAnimations = {
            "Forward Attack", "Left Attack", "Right Attack"
        };
        [SerializeField] private string _reverseAnimationSuffix = " Reverse";
        // Action and movement speed/position
        [SerializeField] private float _liftSpeed = 3f;
        [SerializeField] private Vector3 _moveStartLift = new Vector3(0f, 0f, 1f);
        [SerializeField] private GameObject _buttonRoot;
        [SerializeField] private CardActionButton _destroyButton;
        [SerializeField] private CardActionButton _moveButton;
        [SerializeField] private AudioClip[] _hitSounds;
        [SerializeField] private Vector2 _soundPitch;
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private SpriteRenderer _background;
        [SerializeField] private TMP_Text _descriptionField;
        [SerializeField] private SpriteRenderer _affinityImage;
        [Header("Informative")]
        [SerializeField] private TMP_Text _costField;
        [SerializeField] private TMP_Text _attackField;
        [SerializeField] private TMP_Text _healthField;
        [SerializeField] private TMP_Text _finalAttackField;
        public CardController Controller => _controller;
        public PlayerData Hand { get; private set; }
        public int FinalAttack { get; private set; }
        public CardData Data { get; private set; }
        public int CurrentHealth { get; private set; }
        public int IndexInLayout { get; set; }
        public bool IsDefeated { get; private set; }
        public bool RequestMove { get; private set; }
        public bool CanBePlaced => Hand.CanUseCard(Data.Cost);
        public SortingGroup SortingGroup => _sortingGroup;
        // Invoked each time this card attacks
        private readonly LinkedList<Action> _persistentOnAttackAction = new LinkedList<Action>();
        // Invoked only once during next attack
        private readonly LinkedList<Action> _singleOnAttackAction = new LinkedList<Action>();
        // For other stuff to store statuses/checks
        public readonly Dictionary<string, float> LocalStatuses = new Dictionary<string, float>();
        
        private void Awake()
        {
            _persistentOnAttackAction.AddLast(PlayHitSound);
            HideActions();
        }

        private void OnEnable()
        {
            _destroyButton.AddListener(DestroyCard);
            _moveButton.AddListener(StartMove);
        }

        public void Set(CardData data, PlayerData owner)
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

            Hand = null;
            Data = default;
        }

        public void SetPlaced()
        {
            Hand.GetCardFromHand(Data);
            Hand.UseLight(Data.Cost);
            Hand = null;
        }

        private void PlayHitSound() => AudioManager.Instance.Play(_hitSounds, _soundPitch);

        #region Actions During the Game
        
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
        
        public void ClearFinalAttack()
        {
            FinalAttack = -1;
            _finalAttackField.SetText(EMPTY_FINAL_ATTACK_CHAR);
        }

        public void PlayRandomAnimation() => PlayAnimation(_cardAnimations.GetRandom());

        public void PlayRandomAnimationReverse() => 
            PlayAnimation(_cardAnimations.GetRandom() + _reverseAnimationSuffix);

        public void PlayAnimation(string animationName) => 
            _animator.Play(animationName, -1, 0f);

        public void AddActionOnHit(Action act) => _singleOnAttackAction.AddLast(act);
        public void AddPersistentActionOnHit(Action act) => _persistentOnAttackAction.AddLast(act);

        public void InvokeAttackActions()
        {
            foreach (var act in _persistentOnAttackAction) act();
            foreach (var act in _singleOnAttackAction) act();
            _singleOnAttackAction.Clear();
        }

        #endregion

        #region Action With Card
        
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
            MoveToLocalAsync(transform.localPosition + _moveStartLift, _liftSpeed).Forget();
        }
        
        public void MoveCard(CardSlot newSlot)
        {
            if (!newSlot.CanSnapTo || !newSlot.IsEmpty) CancelMove();
            else newSlot.Attach(this);
        }

        public void CancelMove()
        {
            MoveToLocalAsync(transform.localPosition - _moveStartLift, _liftSpeed).Forget();
            HideActions();
        }
        
        public async UniTask MoveToLocalAsync(Vector3 final, float moveSpeed, float snapDistance = 0.1f, 
            bool reenableController=true)
        {
            _cts = _cts.Reset();
            Controller.Interactable = false;
            var current = transform.localPosition;
            while (Vector3.Distance(current, final) > snapDistance)
            {
                current = Vector3.Lerp(current, final, moveSpeed * Time.deltaTime);
                transform.localPosition = current;
                await UniTask.NextFrame(_cts.Token);
            }

            if (reenableController) Controller.Interactable = true;
            RequestMove = false;
            transform.localPosition = final;
        }

        public async UniTask SwapCardsAsync(CardSlot from, CardSlot to, float moveSpeed, float snapDistance = 0.1f)
        {
            to.Deactivate();
            from.Deactivate();
            
            Controller.Interactable = false;
            var otherCard = to.Detach();
            otherCard.Controller.Interactable = false;
            
            await otherCard.MoveToLocalAsync(otherCard.transform.localPosition + _moveStartLift,
                moveSpeed, snapDistance);
            otherCard.Controller.Interactable = false; // Because it's set true after each move
            
            to.Attach(this);
            from.Attach(otherCard);

            to.Activate();
            from.Activate();
        }

        #endregion
        
        private void OnDestroy() => _cts.Cancel();
    }   
}