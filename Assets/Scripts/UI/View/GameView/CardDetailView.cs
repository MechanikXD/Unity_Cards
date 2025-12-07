using Core.Cards.Card;
using Core.Cards.Card.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View.GameView
{
    public class CardDetailView : CanvasView
    {
        [SerializeField] private Image _sprite;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Image _affinity;
        [SerializeField] private TMP_Text _cost;
        [SerializeField] private TMP_Text _health;
        [SerializeField] private TMP_Text _attack;

        public void LoadData(CardModel model)
        {
            _sprite.sprite = model.Data.Sprite;
            _background.sprite = model.Data.Background;
            _affinity.sprite = CardDataProvider.GetAffinitySprite(model.Data.Affinity);
            _description.SetText(CardDataProvider.MakeDescription(model.Data));
            _cost.SetText(model.Data.Cost.ToString());
            _health.SetText(model.CurrentHealth.ToString());
            _attack.SetText(CardDataProvider.AttackToString(model.Data.Attack));
        }
        
        public void LoadData(CardData model)
        {
            _sprite.sprite = model.Sprite;
            _background.sprite = model.Background;
            _affinity.sprite = CardDataProvider.GetAffinitySprite(model.Affinity);
            _description.SetText(CardDataProvider.MakeDescription(model));
            _cost.SetText(model.Cost.ToString());
            _health.SetText(model.Health.ToString());
            _attack.SetText(CardDataProvider.AttackToString(model.Attack));
        }

        public void Clear()
        {
            var imageNull = CardDataProvider.ImageNull;
            _sprite.sprite = imageNull;
            _background.sprite = imageNull;
            _affinity.sprite = imageNull;
            _description.SetText("");
            _cost.SetText("");
            _health.SetText("");
            _attack.SetText("");
        }
    }
}