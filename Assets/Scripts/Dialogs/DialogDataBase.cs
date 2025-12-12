using System;
using System.Linq;
using JetBrains.Annotations;
using Other.Extensions;
using UnityEngine;

namespace Dialogs
{
    [CreateAssetMenu(fileName = "Dialog DataBase", menuName = "ScriptableObjects/Dialog DataBase")]
    public class DialogDataBase : ScriptableObject
    {
        [SerializeField] private DialogData[] _data;
        public DialogData[] Data => _data;
        
        public DialogData Get(int index) => _data[index];
        public DialogData GetRandom(DialogType ofType) => 
            _data.Where(d => d._dialogType == ofType).ToArray().GetRandom();
    }
    
    [Serializable]
    public struct DialogData
    {
        public string _backgroundImagePath;
        [CanBeNull] public string _foregroundImagePath;
        public DialogType _dialogType;
        public string[] _dialogs;
    }
    
    public enum DialogType
    {
        PlayerBuff,
        EnemyBuff,
        Special
    }
}