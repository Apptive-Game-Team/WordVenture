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

        public TutorialChatData GetScriptData(int n)
        {
            if (n == -1)
            {
                return new TutorialChatData();
            }

            return script[n];
        }

        public int GetScriptNum()
        {
            return script.Count;
        }
    }

}

