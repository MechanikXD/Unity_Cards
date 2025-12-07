using System;
using Player.Progression.SaveStates;

namespace Core.SessionStorage
{
    // NOTE: Must be classes because Newtonsoft.json doesn't hold type names of other struct-fields.
    [Serializable]
    public class BoardState
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
    public class DialogState
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