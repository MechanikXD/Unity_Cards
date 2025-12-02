using System;
using Player.Progression.SaveStates;

namespace Core.SessionStorage
{
    [Serializable]
    public struct BoardState
    {
        public SerializableBoard Board { get; }
        public SerializablePlayerHand Enemy { get; }

        public BoardState(SerializableBoard board, SerializablePlayerHand enemy)
        {
            Board = board;
            Enemy = enemy;
        }
    }

    [Serializable]
    public struct DialogState
    {
        public SerializableDialog Current { get; }
        public SerializableDialog[] Next { get; }
        
        public DialogState(SerializableDialog current, SerializableDialog[] next)
        {
            Current = current;
            Next = next;
        }
    }
}