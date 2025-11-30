using System;
using System.Collections.Generic;
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

        public void Load(Sprite cgi, string[] dialogues, IList<BuffBase> options, Action<DialogSceneController> onConfirm)
        {
            _cgi.sprite = cgi;
            _dialogs = dialogues;
            _optionsCount = options.Count;
            _dialogIsFinished = false;
            _currentDialogIndex = 0;
            for (var i = 0; i < _dialogOptions.Length; i++)
            {
                if (i < options.Count) _dialogOptions[i].Load(options[i]);
                
                _dialogOptions[i].gameObject.SetActive(false);
            }

            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(() => onConfirm(this));
            
            _confirmButton.interactable = false;
            _confirmButton.gameObject.SetActive(false);
            AdvanceDialog();
        }

        public void ConfirmAndLoadNext(Sprite cgi, string[] dialogues, IList<BuffBase> options, Action<DialogSceneController> onConfirm)
        {
            if (_currentlySelected == null)
            {
                _confirmButton.interactable = false;
                return;
            }

            GameStorage.Instance.AddBuff(_currentlySelected.Buff);
            Load(cgi, dialogues, options, onConfirm);
        }

        public void ConfirmAndExit()
        {
            if (_currentlySelected == null)
            {
                _confirmButton.interactable = false;
                return;
            }

            GameStorage.Instance.AddBuff(_currentlySelected.Buff);
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
            if (_dialogIsFinished) return;
            else if (_currentDialogIndex >= _dialogs.Length)
            {
                LoadOptions();
                _dialogIsFinished = true;
                return;
            }

            _dialogWindow.SetText(_dialogs[_currentDialogIndex]);
            _currentDialogIndex++;
        }

        private void LoadOptions()
        {
            for (var i = 0; i < _optionsCount; i++)
            {
                _dialogOptions[i].gameObject.SetActive(true);
            }
            _dialogWindow.SetText(string.Join('\n', _dialogs));
            _confirmButton.gameObject.SetActive(true);
        }
    }
}