using System;
using System.Collections;
using System.Collections.Generic;
using Enemy;
using UnityEngine;


namespace TutorialSystem
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
    }

}

