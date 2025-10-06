using Core.Behaviour;
using UnityEngine;

namespace Player
{
    public class InputBlocker : SingletonBase<InputBlocker>
    {
        private GameObject _blocker;
        
        protected override void Initialize()
        {
            _blocker = gameObject;
            EnableInput();
        }
        
        public void DisableInput() => _blocker.SetActive(true);
        
        public void EnableInput() => _blocker.SetActive(false);
    }
}