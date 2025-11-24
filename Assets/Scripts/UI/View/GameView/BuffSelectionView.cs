using System.Collections.Generic;
using Core;
using Player.Progression.Buffs;
using Player.Progression.Buffs.Enemy;
using Player.Progression.Buffs.Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View.GameView
{
    public class BuffSelectionView : CanvasView
    {
        [SerializeField] private BuffCardModel[] _cards;
        [SerializeField] private Button _selectButton;
        
        [SerializeField] private Color _playerBuffColor;
        [SerializeField] private Color _enemyBuffColor;

        private int _currentTier;
        private int _currentAmount;
        private int _currentMaxDeviation;
        private bool _isPlayerBuffs;

        private BuffBase _currentlySelected;

        private void OnEnable()
        {
            _selectButton.onClick.AddListener(RegisterBuff);
        }
        
        private void OnDisable()
        {
            _selectButton.onClick.RemoveListener(RegisterBuff);
        }

        public void Select(BuffCardModel source)
        {
            _currentlySelected = source.Buff;
            _selectButton.interactable = true;
            foreach (var model in _cards)
            {
                if (model != source) model.Deselect();
            }
        }

        private void RegisterBuff()
        {
            foreach (var model in _cards) model.Interactable = false;
            _selectButton.interactable = false;
            var board = GameManager.Instance.Board;
            if (_isPlayerBuffs) board.PlayerBuff.Add((PlayerBuff)_currentlySelected);
            else board.EnemyBuffs.AddWithoutApply((EnemyBuff)_currentlySelected);
            
            ResetAll();
            if (_isPlayerBuffs) LoadRandomEnemyBuffs();
            else board.StartAct(GameManager.Instance.DifficultySettings);
        }

        public void ResetAll()
        {
            foreach (var model in _cards)
            {
                model.Clear();
                model.Deselect();
                _currentlySelected = null;
                _selectButton.interactable = false;
            }
        }

        public void LoadRandomPlayerBuffs(int tier, int amount=3, int maxDeviation=1)
        {
            _currentTier = tier;
            _currentAmount = amount;
            _currentMaxDeviation = maxDeviation;
            _isPlayerBuffs = true;
            
            var db = GameManager.Instance.BuffDb;
            var buffs = db.RandomPlayerBuff(amount, tier, maxDeviation);
            LoadCards(buffs, _playerBuffColor);
        }

        public void LoadRandomEnemyBuffs()
        {
            var db = GameManager.Instance.BuffDb;
            _isPlayerBuffs = false;
            var buffs = db.RandomEnemyBuff(_currentAmount, _currentTier, _currentMaxDeviation);
            LoadCards(buffs, _enemyBuffColor);
        }

        private void LoadCards<T>(List<T> buffs, Color color) where T : BuffBase
        {
            for (var i = 0; i < _cards.Length; i++)
            {
                if (i > buffs.Count) _cards[i].gameObject.SetActive(false);
                else
                {
                    _cards[i].Set(buffs[i], color);
                    _cards[i].gameObject.SetActive(true);
                }
                
                _cards[i].Interactable = true;
            }
        }
    }
}