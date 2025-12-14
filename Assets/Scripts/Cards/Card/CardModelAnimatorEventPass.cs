using UnityEngine;

namespace Cards.Card
{
    public class CardModelAnimatorEventPass : MonoBehaviour
    {
        [SerializeField] private CardModel _model;

        private void PassOnEvent() => _model.InvokeAttackActions();
    }
}