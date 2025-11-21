using System.Collections.Generic;
using Core.Cards.Deck;
using UnityEngine;

namespace Player.Deck
{
    public class OtherCardDragArea : MonoBehaviour
    {
        [SerializeField] private Transform _root;
        private readonly List<DeckCardModel> _attachedCards = new List<DeckCardModel>();

        public void AddCard(DeckCardModel model)
        {
            model.transform.SetParent(_root);
            model.transform.localScale = Vector3.one;
            model.IndexInLayout = _attachedCards.Count;
            _attachedCards.Add(model);
        }

        public DeckCardModel RemoveCard(int index)
        {
            var model = _attachedCards[index];
            _attachedCards.RemoveAt(index);
            for (var i = index; i < _attachedCards.Count; i++)
            {
                _attachedCards[i].IndexInLayout = i;
            }
            
            return model;
        }
    }
}