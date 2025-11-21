using Core.Cards.Deck;
using UI;
using UI.View.MainMenuView;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player.Deck
{
    public class DeckCardHandler : MonoBehaviour, IPointerUpHandler
    {
        private DeckCardModel _thisModel;

        protected void Awake()
        {
            _thisModel = GetComponent<DeckCardModel>();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
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