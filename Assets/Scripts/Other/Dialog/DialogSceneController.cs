using System;
using Core.Behaviour;
using Core.SessionStorage;
using Other.Dialog.SceneObjects;
using Player.Progression.Buffs;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Other.Dialog
{
    public class DialogSceneController : SingletonBase<DialogSceneController>
    {
        [SerializeField] private Image _cgi;
        [SerializeField] private TMP_Text _dialogWindow;
        [SerializeField] private Button _confirmButton;

        [SerializeField] private OptionButton[] _dialogOptions;
        private int _optionsCount;
        private bool _dialogIsFinished;
        private string[] _dialogs;
        private int _currentDialogIndex;
        
        private OptionButton _currentlySelected;

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveAllListeners();
        }

        protected override void Initialize() { }

        public void Load(Sprite cgi, string[] dialogues, BuffBase[] options, Action<DialogSceneController> onConfirm)
        {
            _cgi.sprite = cgi;
            _dialogs = dialogues;
            _optionsCount = options.Length;
            _currentDialogIndex = -1;
            for (var i = 0; i < _dialogOptions.Length; i++)
            {
                if (i < options.Length)
                {
                    _dialogOptions[i].gameObject.SetActive(true);
                    _dialogOptions[i].Load(options[i]);
                }
                
                _dialogOptions[i].gameObject.SetActive(false);
            }

            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(() => onConfirm(this));
            
            _confirmButton.interactable = false;
            _confirmButton.gameObject.SetActive(false);
            AdvanceDialog();
        }

        public void ConfirmAndLoadNext(Sprite cgi, string[] dialogues, BuffBase[] options, Action<DialogSceneController> onConfirm)
        {
            if (_currentlySelected == null)
            {
                _confirmButton.interactable = false;
                return;
            }

            SessionStorage.Instance.AddBuff(_currentlySelected.Buff);
            Load(cgi, dialogues, options, onConfirm);
        }

        public void ConfirmAndExit()
        {
            if (_currentlySelected == null)
            {
                _confirmButton.interactable = false;
                return;
            }

            SessionStorage.Instance.AddBuff(_currentlySelected.Buff);
            // Other setup here
            SceneManager.LoadScene("GameScene");
        }
        
        public void SelectOption(OptionButton selected)
        {
            foreach (var button in _dialogOptions)
            {
                if (button == selected) button.Select();
                else button.Deselect();
            }

            _confirmButton.interactable = true;
            _currentlySelected = selected;
        }

        public void AdvanceDialog()
        {
            if (_currentDialogIndex >= _dialogs.Length) return;

            _dialogWindow.SetText(_dialogs[_currentDialogIndex]);
            _currentDialogIndex++;

            if (_currentDialogIndex >= _dialogs.Length)
            {
                LoadOptions();
            }
        }

        private void LoadOptions()
        {
            for (var i = 0; i < _optionsCount; i++)
            {
                _dialogOptions[i].gameObject.SetActive(true);
            }
            _dialogWindow.SetText(string.Concat(_dialogs, '\n'));
            _confirmButton.gameObject.SetActive(true);
        }
    }
}