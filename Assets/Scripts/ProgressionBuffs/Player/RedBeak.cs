using Cards.Card.Effects;
using Cards.Hand;
using ProgressionBuffs.Scriptables;
using Structure.Managers;
using UnityEngine;

namespace ProgressionBuffs.Player
{
    [CreateAssetMenu(fileName = "RedBeak", menuName = "ScriptableObjects/Buff/Player/RedBeak")]
    public class RedBeak : PlayerBuff
    {
        [Header("Apply on enemy")]
        [SerializeField] private TriggerType _enemyTrigger;
        [SerializeField] private CardEffect _enemyEffect;
        [Header("Apply to player")]
        [SerializeField] private TriggerType _playerTrigger;
        [SerializeField] private CardEffect _playerEffect;
        
        public override void Apply(PlayerData data)
        {
            foreach (var card in GameManager.Instance.Board.EnemyData.Deck) 
                card.AddEffect(_enemyTrigger, _enemyEffect);
            foreach (var card in data.Deck) 
                card.AddEffect(_playerTrigger, _playerEffect);
        }
    }
}