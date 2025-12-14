using Cysharp.Threading.Tasks;
using Dialogs;
using ProgressionBuffs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Other.Buttons
{
    public class BuffOptionButton : MonoBehaviour
    {
        [SerializeField] private float _colorChangeSpeed = 10f;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        private Color _textTargetColor;
        private bool _isChangingColor;
        private const float VALUE_SNAP = 0.1f;
        
        public BuffBase Buff { get; private set; }

        private void OnEnable() => _button.onClick.AddListener(NotifyController);

        private void OnDisable() => _button.onClick.RemoveListener(NotifyController);

        public void Load(BuffBase option)
        {
            _button.enabled = true;
            _title.color = Color.black;
            _description.color = Color.black;
            _title.SetText(option.OptionTitle);
            _description.SetText(option.Description);
            Buff = option;
        }

        public void Select()
        {
            _textTargetColor = Color.white;
            if (!_isChangingColor && _textTargetColor != _title.color) ChangleTextColor().Forget();
        }

        public void Deselect()
        {
            _textTargetColor = Color.black;
            if (!_isChangingColor && _textTargetColor != _title.color) ChangleTextColor().Forget();
        }

        private void NotifyController()
        {
            var controller = DialogSceneController.Instance;
            if (controller != null) controller.SelectOption(this);
        }

        private async UniTask ChangleTextColor()
        {
            _isChangingColor = true;
            while (ColorDistance(_title.color, _textTargetColor) > VALUE_SNAP)
            {
                _title.color = Color.Lerp(_title.color, _textTargetColor, _colorChangeSpeed);
                _description.color = Color.Lerp(_description.color, _textTargetColor, _colorChangeSpeed);
                await UniTask.NextFrame(destroyCancellationToken);
            }

            _title.color = _textTargetColor;
            _description.color = _textTargetColor;
            _isChangingColor = false;
        }
        
        private static float ColorDistance(Color first, Color second)
        {
            return Mathf.Abs(first.r - second.r) + Mathf.Abs(first.g - second.g) +
                   Mathf.Abs(first.b - second.b);
        }
    }
}