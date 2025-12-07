using System;
using JetBrains.Annotations;
using Other.Extensions;
using UnityEngine;

namespace Dialogs
{
    [CreateAssetMenu(fileName = "Dialog DataBase", menuName = "ScriptableObjects/Dialog DataBase")]
    public class DialogDataBase : ScriptableObject
    {
        [SerializeField] private DialogData[] _data;

        public DialogData Get(int index) => _data[index];
        public DialogData GetRandom() => _data.GetRandom();
    }
    
    [Serializable]
    public struct DialogData
    {
        public string _backgroundImagePath;
        [CanBeNull] public string _foregroundImagePath;
        public string[] _dialogs;
    }
}