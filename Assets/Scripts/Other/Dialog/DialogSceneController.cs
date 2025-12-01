using System.Collections.Generic;
using Core.Behaviour;
using Core.SessionStorage;
using Other.Dialog.SceneObjects;
using Player.Progression.Buffs;
using Player.Progression.SaveStates;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Other.Dialog
{
    public class DialogSceneController : SingletonBase<DialogSceneController>
    {
        private LinkedList<DialogSettings> _nextDialogs = new LinkedList<DialogSettings>();
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

        public void Load(DialogSettings[] dialogs)
        {
            foreach(var dialog in dialogs) _nextDialogs.AddLast(dialog);
            var current = _nextDialogs.First;
            _nextDialogs.RemoveFirst();
            
            Load(current.Value);
        }
        
        public void Load(DialogSettings dialog)
        {
            _cgi.sprite = dialog.Cgi;
            _dialogs = dialog.Dialogues;
            _optionsCount = dialog.Options.Count;
            _dialogIsFinished = false;
            _currentDialogIndex = 0;
            for (var i = 0; i < _dialogOptions.Length; i++)
            {
                if (i < _optionsCount) _dialogOptions[i].Load(dialog.Options[i]);
                
                _dialogOptions[i].gameObject.SetActive(false);
            }
            
            _confirmButton.interactable = false;
            _confirmButton.gameObject.SetActive(false);
            AdvanceDialog();
        }

        public void LoadFromSerialized(SerializableDialog serialized, BuffDataBase db)
        {
            _cgi.sprite = Resources.Load<Sprite>(serialized._spritePath);
            _dialogs = serialized._dialogs;
            
            _currentDialogIndex = serialized._currentDialogIndex;
            _dialogIsFinished = _currentDialogIndex >= _dialogs.Length;
            
            _optionsCount = serialized._options.Length;
            for (var i = 0; i < _optionsCount; i++) 
                _dialogOptions[i].Load(db.Get<BuffBase>(serialized._options[i]));
            
            if (_dialogIsFinished) LoadOptions();
            else
            {
                _currentDialogIndex -= 1;
                if (_currentDialogIndex < 0) _currentDialogIndex = 0;
                AdvanceDialog();
            }
        }

        public void ConfirmButtonPress()
        {
            if (_currentlySelected == null)
            {
                _confirmButton.interactable = false;
                return;
            }
            GameStorage.Instance.AddBuff(_currentlySelected.Buff);
            
            if (_nextDialogs.Count == 0)
            {
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                var next = _nextDialogs.First;
                _nextDialogs.RemoveFirst();
                Load(next.Value);
            }
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

    public struct DialogSettings
    {
        public Sprite Cgi { get; }
        public string[] Dialogues { get; }
        public IList<BuffBase> Options { get; }

        public DialogSettings(Sprite cgi, string[] dialogues, IList<BuffBase> options)
        {
            Cgi = cgi;
            Dialogues = dialogues;
            Options = options;
        }
    }
}