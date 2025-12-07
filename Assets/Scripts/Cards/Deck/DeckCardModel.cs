using Cards.Card;
using Cards.Card.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Cards.Deck
{
    public class DeckCardModel : MonoBehaviour
    {
        [SerializeField] private SortingGroup _sortingGroup;
        [Header("Visual")]
        [SerializeField] private Image _sprite;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _descriptionField;
        [SerializeField] private Image _affinityImage;
        [Header("Informative")]
        [SerializeField] private TMP_Text _costField;
        [SerializeField] private TMP_Text _attackField;
        [SerializeField] private TMP_Text _healthField;

        public bool InPlayerHand { get; set; }
        public int IndexInLayout { get; set; }
        public CardData CardData { get; private set; }
        public SortingGroup SortingGroup => _sortingGroup;
        
        public void Set(CardData data)
        {
            CardData = data;
            _sprite.sprite = data.Sprite;
            _background.sprite = data.Background;
            _descriptionField.SetText(CardDataProvider.MakeDescription(data));
            _affinityImage.sprite = CardDataProvider.GetAffinitySprite(data.Affinity);
            
            _attackField.SetText(CardDataProvider.AttackToString(data.Attack));
            _costField.SetText(data.Cost.ToString());
            _healthField.SetText(data.Health.ToString());
        }

        public void Clear()
        {
            var imageNull = CardDataProvider.ImageNull;
            _sprite.sprite = imageNull;
            _background.sprite = imageNull;
            _descriptionField.SetText(string.Empty);
            _affinityImage.sprite = imageNull;
            
            _attackField.SetText(string.Empty);
            _costField.SetText(string.Empty);
            _healthField.SetText(string.Empty);

            CardData = default;
        }
    }
}