using System.Collections;
using System.Collections.Generic;
using Deck_Manage;
using UnityEngine;

namespace Deck_Manage 
{
    [System.Serializable]
    public class Word
    {
        public string name;
        public int percent;
        public string tag;
        public MagicType magicType;
    }


    [CreateAssetMenu(fileName = "WordSO", menuName = "Scriptable Object/WordSO")]
    public class WordSO : ScriptableObject
    {
        public Word[] words;
    }

}
