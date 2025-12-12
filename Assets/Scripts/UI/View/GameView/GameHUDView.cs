using Cysharp.Threading.Tasks;
using Other.Interactions;
using ProgressionBuffs;
using ProgressionBuffs.Enemy;
using ProgressionBuffs.Player;
using ProgressionBuffs.Scriptables;
using Structure.Managers;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace UI.View.GameView
{
    public class GameHUDView : CanvasView
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _buffShowButton;
        [SerializeField] private GameObject _buffListRoot;
        [SerializeField] private TMP_Text _playerBuffTextArea;
        [SerializeField] private TMP_Text _enemyBuffTextArea;
        
        public void EnablePlayButton(bool enable) => _playButton.interactable = enable;
        
        private void OnEnable() => SubscribeToEvents();

        private void OnDisable() => UnSubscribeFromEvents();

        public void LoadBuffList(BuffStorage<PlayerBuff> player, BuffStorage<EnemyBuff> enemy)
        {
            _playerBuffTextArea.SetText(player.ToString());
            _enemyBuffTextArea.SetText(enemy.ToString());
        }

        private void ShowBuffList()
        {
            _buffShowButton.interactable = false;
            _buffListRoot.SetActive(true);
        }

        public void HideBuffList()
        {
            _buffShowButton.interactable = true;
            _buffListRoot.SetActive(false);
        }
        
        private void SubscribeToEvents()
        {
            _playButton.onClick.AddListener(PlayTurn);
            _pauseButton.onClick.AddListener(Pause);
            _buffShowButton.onClick.AddListener(ShowBuffList);
        }

        private void UnSubscribeFromEvents()
        {
            _playButton.onClick.RemoveListener(PlayTurn);
            _pauseButton.onClick.RemoveListener(Pause);
            _buffShowButton.onClick.RemoveListener(ShowBuffList);
        }

        public override void Enable()
        {
            base.Enable();
            HideBuffList();
        }

        public override void Disable()
        {
            base.Disable();
            HideBuffList();
        }

        private static void Pause()
        {
            if (UIManager.Instance != null) UIManager.Instance.EnterUICanvas<GameMenuView>();
        }

        private static void PlayTurn()
        {
            HideInfoOnClick.HideAll();
            if (GameManager.Instance != null) GameManager.Instance.Board.PlayTurnAsync().Forget();
        }
    }
}