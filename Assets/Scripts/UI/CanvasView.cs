using UnityEngine;

namespace UI
{
    public abstract class CanvasView : MonoBehaviour
    {
        [SerializeField] private bool _hideOnStart = false;
        protected Canvas ThisCanvas;
        
        protected virtual void Awake()
        {
            ThisCanvas = GetComponent<Canvas>();
            if (_hideOnStart) Disable();
        }

        public virtual void Enable()
        {
            ThisCanvas.enabled = true;
        }

        public virtual void Disable()
        {
            ThisCanvas.enabled = false;
        }
    }
}