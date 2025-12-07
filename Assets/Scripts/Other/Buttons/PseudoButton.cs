using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Other.Buttons
{
    public class PseudoButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private bool _interactable;
        private readonly HashSet<Action> _events = new HashSet<Action>();

        public void SetInteractable(bool interactable) => _interactable = interactable; 
        public void AddListener(Action onClick) => _events.Add(onClick);
        public void RemoveListener(Action onCLick) => _events.Remove(onCLick);
        public void RemoveAllListeners() => _events.Clear();

        public void OnPointerDown(PointerEventData eventData) { }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_interactable || _events == null) return;
            foreach (var action in _events) action();
        }
    }
}