using UnityEngine;
using UnityEngine.EventSystems;

namespace Other.Dialog.SceneObjects
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