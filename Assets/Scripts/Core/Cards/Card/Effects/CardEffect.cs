using System;
using Core.Cards.Board;
using UnityEngine;

namespace Core.Cards.Card.Effects
{
    [Serializable]
    public abstract class CardEffect
    {
        [SerializeField] private string _description;
        public string Description => _description;

        public abstract void Execute(BoardContext context);
    }
}