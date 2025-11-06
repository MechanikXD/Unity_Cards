using UnityEngine;

namespace Player
{
    public abstract class DragHandler : MonoBehaviour
    {
        protected const int CardOrder = 99;
        [SerializeField] protected LayerMask _mouseReleaseMask;
        [SerializeField] protected float _cardMoveSpeed = 2f;
        [SerializeField] protected Vector2 _xMoveBorders;
        
        protected Vector3 OriginalPosition;
        protected Plane Plane = new Plane(Vector3.forward, Vector3.zero);
        protected Camera Camera;

        protected virtual void Awake()
        {
            if (Camera.main == null)
            {
                Debug.LogError("There is no main camera on this scene!");
            }
            else Camera = Camera.main;
        }

        public void OnMouseDown()
        {
            if (!GlobalInputBlocker.Instance.InputEnabled) return;

            OriginalPosition = transform.position;
            PickUpObject();
        }

        public void OnMouseDrag()
        {
            var newPoint = GetRaycastHitPoint();
            newPoint.x = Mathf.Clamp(newPoint.x, _xMoveBorders.x, _xMoveBorders.y);
            transform.position = newPoint;
        }

        public void OnMouseUp()
        {
            DropObject();
        }
        
        protected abstract void PickUpObject();

        protected abstract void DropObject();

        protected Vector3 GetRaycastHitPoint()
        {
            var ray = Camera.ScreenPointToRay(Input.mousePosition);
            return Plane.Raycast(ray, out var intersection) 
                ? ray.GetPoint(intersection) : Vector3.zero;
        }
    }
}