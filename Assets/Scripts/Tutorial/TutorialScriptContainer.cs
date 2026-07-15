using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using WordVenture.Combat.Enemies;
using WordVenture.Combat.UI;
using WordVenture.Story;

namespace WordVenture.Tutorial
{
    [Serializable]
    public struct TutorialChatData
    {
        [SerializeField] public string name;
        [SerializeField] public string text;
        [SerializeField] public int portraitID;
        [SerializeField] public TutorialFlag tutorialFlag;
}


    [CreateAssetMenu]
    public class TutorialScriptContainer : ScriptableObject
    {

        [SerializeField] List<TutorialChatData> script = new List<TutorialChatData>();
        [SerializeField] List<Sprite> speakerImage = new List<Sprite>();

        public TutorialChatData GetScriptData(TutorialFlag flag)
        {
            return script.Find(x=>x.tutorialFlag.Equals(flag));
        }

        public int GetScriptNum()
        {
            return script.Count;
        }

        public Sprite GetSprite(int id)
        {
            return speakerImage[id];
        }
    }

}

