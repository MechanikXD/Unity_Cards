using Core;
using UI;
using UI.View.GameView;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Other
{
    public class HideInfoOnClick : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData) { }
        public void OnPointerUp(PointerEventData eventData) => HideAll();

        public static void HideAll()
        {
            HideInfo();
            CancelCardMove();
        }
        
        public static void CancelCardMove()
        {
            var board = GameManager.Instance.Board;
            foreach (var slot in board.PlayerSlots)
            {
                if (slot.IsEmpty) continue;

                if (slot.Card.RequestMove) slot.Card.CancelMove();
                else slot.Card.HideActions();
                // break;
            }
        }

        public static void HideInfo() => UIManager.Instance.ExitHudCanvas<CardDetailView>();
    }
}