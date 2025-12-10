using Dialogs;
using ProgressionBuffs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Other.Buttons
{
    public class BuffOptionButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        
        public BuffBase Buff { get; private set; }

        private void OnEnable() => _button.onClick.AddListener(NotifyController);

        private void OnDisable() => _button.onClick.RemoveListener(NotifyController);

        public void Load(BuffBase option)
        {
            _button.enabled = true;
            _title.SetText(option.OptionTitle);
            _description.SetText(option.Description);
            Buff = option;
        }

        public void Select()
        {
            
        }

        public void Deselect()
        {
            
        }

        private void NotifyController()
        {
            var controller = DialogSceneController.Instance;
            if (controller != null) controller.SelectOption(this);
        }
    }
}