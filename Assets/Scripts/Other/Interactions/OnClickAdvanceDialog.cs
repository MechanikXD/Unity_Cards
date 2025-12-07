using Dialogs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Other.Interactions
{
    public class OnClickAdvanceDialog : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public void OnPointerUp(PointerEventData eventData)
        {
            DialogSceneController.Instance.AdvanceDialog();
        }

        public void OnPointerDown(PointerEventData eventData) { }
    }
}