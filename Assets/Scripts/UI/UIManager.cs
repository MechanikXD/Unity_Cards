using System;
using System.Collections.Generic;
using Structure;
using UnityEngine;

namespace UI
{
    public class UIManager : SingletonBase<UIManager>
    {
        private Dictionary<Type, CanvasView> _uiCanvases;
        private Dictionary<Type, CanvasView> _hudCanvases;
        [SerializeField] private bool _enterPauseOnUiOpen = true;
        [SerializeField] private bool _stopTimeOnPause = true;
        [SerializeField] private bool _blockInputOnPause = true;

        [SerializeField] private Canvas _sceneInputBlock;
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
            ExitPauseState();
        }

        public void EnterUICanvas<T>() where T : CanvasView {
            if (!HasOpenedUI && _enterPauseOnUiOpen) EnterPauseState();
            
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

        public bool ContainsHUD<T>() where T : CanvasView => _hudCanvases.ContainsKey(typeof(T));
        
        public void EnterHUDCanvas<T>() where T : CanvasView => GetHUDCanvas<T>().Enable();

        public void ExitHudCanvas<T>() where T : CanvasView {
            if (_hudCanvases.TryGetValue(typeof(T), out var hud)) hud.Disable();
        }
        
        private void EnterPauseState() {
            HasOpenedUI = true;
            if (_stopTimeOnPause) Time.timeScale = 0f;
            if (_blockInputOnPause) _sceneInputBlock.enabled = true;
        }

        private void ExitPauseState() {
            HasOpenedUI = false;
            Time.timeScale = 1f;
            _sceneInputBlock.enabled = false;
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
                else _uiStack.Push(uiCanvas);
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