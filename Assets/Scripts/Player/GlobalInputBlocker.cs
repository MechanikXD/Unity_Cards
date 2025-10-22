using Core.Behaviour;
using UnityEngine;

namespace Player
{
    public class GlobalInputBlocker : SingletonBase<GlobalInputBlocker>
    {
        private GameObject _blocker;
        public bool InputEnabled { get; private set; }
        
        protected override void Initialize()
        {
            _blocker = gameObject;
            EnableInput();
        }
        
        public void DisableInput()
        {
            _blocker.SetActive(true);
            InputEnabled = false;
        }

        public void EnableInput()
        {
            _blocker.SetActive(false);
            InputEnabled = true;
        }
    }
}