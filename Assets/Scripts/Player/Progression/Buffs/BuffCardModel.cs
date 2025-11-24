using System.Collections.Generic;
using Core.Cards.Card;
using TMPro;
using UI.View.GameView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Player.Progression.Buffs
{
    public class BuffCardModel : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private MaskableGraphic[] _colorChange;
        [SerializeField] private BuffSelectionView _owner;
        
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _tier;
        [SerializeField] private TMP_Text _description;

        public BuffBase Buff { get; private set; }
        public bool Interactable { get; set; }
        
        private readonly static List<string> RomanNumerals = new List<string> { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        private readonly static List<int> Numerals = new List<int> { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Interactable)
            {
                HighLight();
                _owner.Select(this);
            }
        }

        public void HighLight()
        {
            Debug.Log($"Selected {gameObject.name} buff");
        }

        public void Deselect()
        {
            Debug.Log($"Deselected {gameObject.name} buff");
        }

        public void Set(BuffBase buff, Color cardColor)
        {
            Buff = buff;
            _icon.sprite = buff.Icon;
            _title.SetText(buff.Title);
            _tier.SetText(ToRomanNumeral(buff.Tier));
            _description.SetText(buff.Description);

            foreach (var image in _colorChange) image.color = cardColor;
        }

        public void Clear()
        {
            Buff = null;
            _icon.sprite = CardDataProvider.ImageNull;
            _title.SetText("");
            _tier.SetText("");
            _description.SetText("");

            foreach (var image in _colorChange) image.color = Color.white;
        }

        private static string ToRomanNumeral(int number)
        {
            var romanNumeral = string.Empty;
            while (number > 0)
            {
                var index = Numerals.FindIndex(x => x <= number);
                number -= Numerals[index];
                romanNumeral += RomanNumerals[index];
            }
            return romanNumeral;
        }
    }
}