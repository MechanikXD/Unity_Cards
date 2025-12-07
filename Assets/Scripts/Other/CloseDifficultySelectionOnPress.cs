using UI;
using UI.View.MainMenuView;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Other
{
    public class CloseDifficultySelectionOnPress : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData) => 
            UIManager.Instance.ExitHudCanvas<DifficultySelectionView>();
    }
}