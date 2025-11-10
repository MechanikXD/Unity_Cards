using System.Collections.Generic;
using System.Linq;
using Core.Cards.Deck;
using TMPro;
using UnityEngine;

namespace Player.Deck
{
    public class PlayerCardDragArea : MonoBehaviour
    {
        [SerializeField] private Transform _root;
        [SerializeField] private int _maxCapacity;
        [SerializeField] private TMP_Text _maxCapacityText;
        [SerializeField] private TMP_Text _currentCapacityText;
        private int _currentCapacity;
        private readonly List<DeckCardModel> _attachedCards = new List<DeckCardModel>();
        private readonly HashSet<int> _currentCardsInDeck = new HashSet<int>();

        public List<int> GetCardIDs => _currentCardsInDeck.ToList();

        public bool CanAddToDeck(int cost) => _maxCapacity - _currentCapacity >= cost;

        private void Awake()
        {
            _currentCapacityText.SetText(_currentCapacity.ToString());
            _maxCapacityText.SetText(_maxCapacity.ToString());
        }

        public void AddCard(DeckCardModel model)
        {
            _currentCardsInDeck.Add(model.CardData.ID);
            _currentCapacity += model.CardData.Cost;
            model.InPlayerHand = true;
            _currentCapacityText.SetText(_currentCapacity.ToString());
            model.transform.SetParent(_root);
            model.transform.localScale = Vector3.one;
            model.IndexInLayout = _attachedCards.Count;
            _attachedCards.Add(model);
        }

        public DeckCardModel RemoveCard(int index)
        {
            var model = _attachedCards[index];
            _currentCapacity -= model.CardData.Cost;
            model.InPlayerHand = false;
            _currentCapacityText.SetText(_currentCapacity.ToString());
            _attachedCards.RemoveAt(index);
            for (var i = index; i < _attachedCards.Count; i++)
            {
                _attachedCards[i].IndexInLayout = i;
            }
            
            _currentCardsInDeck.Remove(index);
            return model;
        }
    }
}