using Other;
using Other.Interactions;
using UI;
using UI.View.GameView;
using UI.View.MainMenuView;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cards.Deck
{
    public class DeckCardHandler : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private float _holdThreshold = 0.3f;
        private DeckCardModel _thisModel;
        private float _lastPointerDownTime;

        protected void Awake()
        {
            _thisModel = GetComponent<DeckCardModel>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _lastPointerDownTime = Time.unscaledTime;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var duration = Time.unscaledTime - _lastPointerDownTime;
            
            if (duration >= _holdThreshold)
            {
                var detailView = UIManager.Instance.GetHUDCanvas<CardDetailView>();
                detailView.LoadData(_thisModel.CardData);
                detailView.Enable();
            }
            else
            {
                HideInfoOnClick.HideInfo();
                var playerCards = UIManager.Instance.GetUICanvas<DeckView>().PlayerCards;
                var otherCards = UIManager.Instance.GetUICanvas<DeckView>().OtherCards;

                if (_thisModel.InPlayerHand)
                {
                    playerCards.RemoveCard(_thisModel.IndexInLayout);
                    otherCards.AddCard(_thisModel);
                }
                else if (playerCards.CanAddToDeck(_thisModel.CardData.Cost))
                {
                    otherCards.RemoveCard(_thisModel.IndexInLayout);
                    playerCards.AddCard(_thisModel);
                }
            }
        }
    }
}