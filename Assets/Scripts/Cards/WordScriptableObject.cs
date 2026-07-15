using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.UI;
using WordVenture.Core;

namespace WordVenture.Cards
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
    public class WordScriptableObject : ScriptableObject
    {
        public Word[] words;
    }

}
