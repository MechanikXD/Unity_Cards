using Enemy;
using Other.Buttons;
using UnityEngine;

namespace UI.View.MainMenuView
{
    public class DifficultySelectionView : CanvasView
    {
        [SerializeField] private Transform _container;
        [SerializeField] private DifficultyButton _buttonPrefab;
        private bool _hasLoadedButtons;

        public void CreateButtons(EnemyDifficultySettings[] settings)
        {
            if (_hasLoadedButtons) return;
            foreach (var setting in settings)
            {
                var newDifficulty = Instantiate(_buttonPrefab, _container);
                newDifficulty.SetData(setting);
            }

            _hasLoadedButtons = true;
        }
    }
}