using System.Collections.Generic;
using Core.Behaviour;
using Core.Cards.Card;
using Core.SessionStorage;
using JetBrains.Annotations;
using Other.Dialog.SceneObjects;
using Other.Extensions;
using Player.Progression.Buffs;
using Player.Progression.SaveStates;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Other.Dialog
{
    public class DialogSceneController : SingletonBase<DialogSceneController>, IGameSerializable<DialogState>
    {
        private LinkedList<DialogSettings> _nextDialogs = new LinkedList<DialogSettings>();
        [SerializeField] private Image _sprite;
        [SerializeField] private Image _foreground;
        private string _currentSpritePath;
        [CanBeNull] private string _currentForegroundPath;
        [SerializeField] private TMP_Text _dialogWindow;
        [SerializeField] private Button _confirmButton;
        // Because my button is nested inside actual gameObject of button:
        private GameObject ConfirmButtonGameObject => _confirmButton.transform.parent.gameObject;

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
            _sprite.sprite = dialog.Sprite;
            _foreground.sprite = dialog.Foreground == null
                ? CardDataProvider.ImageNull
                : dialog.Foreground;
            _currentForegroundPath = dialog.ForegroundPath;
            _currentSpritePath = dialog.SpritePath;
            _dialogs = dialog.Dialogues;
            _optionsCount = dialog.Options.Count;
            _dialogIsFinished = false;
            _currentDialogIndex = 0;
            for (var i = 0; i < _dialogOptions.Length; i++)
            {
                if (i < _optionsCount) _dialogOptions[i].Load(dialog.Options[i]);
                
                _dialogOptions[i].gameObject.SetActive(false);
            }
            
            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(ConfirmButtonPress);
            
            _confirmButton.interactable = false;
            ConfirmButtonGameObject.SetActive(false);
            AdvanceDialog();
        }

        public DialogState SerializeSelf()
        {
            var buffIds = new int[_optionsCount];
            for (var i = 0; i < _optionsCount; i++)
            {
                buffIds[i] = _dialogOptions[i].Buff.ID;
            }

            var current =
                new SerializableDialog(_currentSpritePath, _currentForegroundPath, _dialogs, buffIds, _currentDialogIndex);
            var next = new SerializableDialog[_nextDialogs.Count];

            var index = 0;
            foreach (var settings in _nextDialogs)
            {
                next[index] = new SerializableDialog(settings);
                index++;
            }

            return new DialogState(current, next);
        }

        public void Deserialize(DialogState self)
        {
            var db = GameStorage.Instance.BuffDataBase;
            _nextDialogs = self.Next.ToLinkedList(s => s.ToDialogSetting(db));
            _currentSpritePath = self.Current._spritePath;
            _sprite.sprite = Resources.Load<Sprite>(self.Current._spritePath);
            _foreground.sprite = self.Current._foregroundPath == null
                ? CardDataProvider.ImageNull
                : Resources.Load<Sprite>(self.Current._foregroundPath);
            _currentForegroundPath = self.Current._foregroundPath;
            _dialogs = self.Current._dialogs;
            
            _currentDialogIndex = self.Current._currentDialogIndex;
            _dialogIsFinished = _currentDialogIndex >= _dialogs.Length;
            
            _optionsCount = self.Current._options.Length;
            for (var i = 0; i < _dialogOptions.Length; i++)
            {
                if (i < _optionsCount) _dialogOptions[i].Load(db.Get<BuffBase>(self.Current._options[i]));
                _dialogOptions[i].gameObject.SetActive(false);
            }
            
            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(ConfirmButtonPress);
            
            if (_dialogIsFinished) LoadOptions();
            else
            {
                _currentDialogIndex -= 1;
                if (_currentDialogIndex < 0) _currentDialogIndex = 0;
                _confirmButton.interactable = false;
                ConfirmButtonGameObject.SetActive(false);
                AdvanceDialog();
            }
        }

        private void ConfirmButtonPress()
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
            _confirmButton.interactable = false;
            ConfirmButtonGameObject.SetActive(true);
        }
    }

    public struct DialogSettings
    {
        public readonly Sprite Sprite;
        [CanBeNull] public readonly Sprite Foreground;
        public readonly string SpritePath;
        [CanBeNull] public readonly string ForegroundPath;
        public readonly string[] Dialogues;
        public readonly IList<BuffBase> Options;

        public DialogSettings(string spritePath, [CanBeNull] string foregroundPath, string[] dialogues, IList<BuffBase> options)
        {
            SpritePath = spritePath;
            ForegroundPath = foregroundPath;
            Sprite = Resources.Load<Sprite>(spritePath);
            Foreground = Resources.Load<Sprite>(foregroundPath);
            Dialogues = dialogues;
            Options = options;
        }
    }
}