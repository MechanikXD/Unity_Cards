using System;
using System.Collections.Generic;
using Core.Behaviour;
using UnityEngine;

namespace UI
{
    public class UIManager : SingletonBase<UIManager>
    {
        private Dictionary<Type, CanvasView> _uiCanvases;
        private Dictionary<Type, CanvasView> _hudCanvases;

        [SerializeField] private CanvasView[] _sceneUiCanvases;
        [SerializeField] private CanvasView[] _sceneHudCanvases;

        private Stack<CanvasView> _uiStack;
        public bool HasOpenedUI { get; private set; }
        
        protected override void Initialize() {
            _hudCanvases = new Dictionary<Type, CanvasView>();
            _uiCanvases = new Dictionary<Type, CanvasView>();
            _uiStack = new Stack<CanvasView>();
            HasOpenedUI = false;
            SortCanvases();
            DisableCanvases();
        }

        public void EnterUICanvas<T>() where T : CanvasView {
            if (!HasOpenedUI) EnterPauseState();
            
            if (_uiStack.Count > 0) _uiStack.Peek().Disable();
            var canvas = GetUICanvas<T>();
            _uiStack.Push(canvas);
            HasOpenedUI = true;
            canvas.Enable();
        }

        public void ExitLastCanvas() {
            if (_uiStack.Count > 0) _uiStack.Pop().Disable();
            
            if (_uiStack.Count > 0) _uiStack.Peek().Enable();
            else ExitPauseState();
        }
        
        public void EnterHUDCanvas<T>() where T : CanvasView {
            GetHUDCanvas<T>().Enable();
        }

        public void ExitHudCanvas<T>() where T : CanvasView {
            if (_hudCanvases.TryGetValue(typeof(T), out var hud)) hud.Disable();
        }
        
        private void EnterPauseState() {
            HasOpenedUI = true;
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void ExitPauseState() {
            HasOpenedUI = false;
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public T GetUICanvas<T>() where T : CanvasView => (T)_uiCanvases[typeof(T)];
        public T GetHUDCanvas<T>() where T : CanvasView => (T)_hudCanvases[typeof(T)];

        private void SortCanvases() {
            foreach (var hudCanvas in _sceneHudCanvases) {
                _hudCanvases.Add(hudCanvas.GetType(), hudCanvas);
            }
            
            foreach (var uiCanvas in _sceneUiCanvases) {
                _uiCanvases.Add(uiCanvas.GetType(), uiCanvas);
            }
        }
        
        private void DisableCanvases() {
            foreach (var uiCanvas in _uiCanvases.Values) {
                // Safe exit from canvas (disables only canvas, not gameObject)
                if (!uiCanvas.gameObject.activeInHierarchy) {
                    uiCanvas.gameObject.SetActive(true);
                }
                if (uiCanvas.HideOnStart) uiCanvas.Disable();
            }
            
            foreach (var hudCanvas in _hudCanvases.Values) {
                // Safe exit from canvas (disables only canvas, not gameObject)
                if (!hudCanvas.gameObject.activeInHierarchy) {
                    hudCanvas.gameObject.SetActive(true);
                }
                if (hudCanvas.HideOnStart) hudCanvas.Disable();
            }
        }
    }
}