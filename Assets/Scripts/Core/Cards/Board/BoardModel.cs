using System.Threading;
using Core.Cards.Hand;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Cards.Board
{
    public class BoardModel : MonoBehaviour
    {
        private const float FINAL_ATTACK_DISPLAY_DELAY = 0.5f;
        [SerializeField] private Button _playButton;   
        [SerializeField] private Transform _playerCards;
        [Header("Player")]
        [SerializeField] private CardSlot[] _playerCardSlots;
        [SerializeField] private PlayerHand _playerHand;
        [Space]
        [SerializeField] private Image _playerHpFill;
        [SerializeField] private TMP_Text _playerHpText;
        [Space]
        [SerializeField] private Image _playerHopeFill;
        [SerializeField] private TMP_Text _playerHopeText;
        
        [Header("Other")]
        [SerializeField] private CardSlot[] _otherCardSlots;
        [SerializeField] private PlayerHand _otherHand;
        [Space]
        [SerializeField] private Image _otherHpFill;
        [SerializeField] private TMP_Text _otherHpText;
        [Space]
        [SerializeField] private Image _otherHopeFill;
        [SerializeField] private TMP_Text _otherHopeText;

        public void PlayTurn()
        {
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                
            }
        }

        private async UniTask DisplayFinalAttacks(CancellationToken ct)
        {
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                if (!_playerCardSlots[i].IsEmpty)
                {
                    _playerCardSlots[i].Card.SetRandomFinalAttack(default);
                    await UniTask.WaitForSeconds(FINAL_ATTACK_DISPLAY_DELAY * 1000, cancellationToken:ct);
                }
                
                if (!_otherCardSlots[i].IsEmpty)
                {
                    _playerCardSlots[i].Card.SetRandomFinalAttack(default);
                    if (i < _playerCardSlots.Length) await UniTask.WaitForSeconds(FINAL_ATTACK_DISPLAY_DELAY * 1000, cancellationToken:ct);
                }
            }
        }
    }

    public readonly struct BoardContext
    {
        public readonly (int final, CardSlot card)[] Player;
        public readonly int PlayerHealth;
        public readonly int PlayerHope;
        
        public readonly (int final, CardSlot card)[] Other;
        public readonly int OtherHealth;
        public readonly int OtherHope;

        public BoardContext((int final, CardSlot card)[] player, int playerHealth, int playerHope, 
                            (int final, CardSlot card)[] other, int otherHealth, int otherHope)
        {
            OtherHope = otherHope;
            Other = other;
            OtherHealth = otherHealth;
            Player = player;
            PlayerHealth = playerHealth;
            PlayerHope = playerHope;
        }
    }
}