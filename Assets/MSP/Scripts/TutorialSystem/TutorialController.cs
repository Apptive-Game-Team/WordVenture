using System.Collections;
using System.Collections.Generic;
using TutorialSystem;
using UnityEditor.UIElements;
using UnityEngine;

namespace TutorialSystem
{
    public class TutorialController : StoryController
    {
        public static TutorialController Instance;

        [SerializeField] TutorialChatWindow tutorialChatWindow;
        [SerializeField] TutorialScriptContainer tutorialScript;

        [SerializeField] TutorialFlag currentFlag = TutorialFlag.FLAG_START_TUTORIAL;
        [SerializeField] ITutorialCondition tutorialCondition;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            StoryTelling();
            tutorialCondition = new TutorialConditon_002();
        }

        public void OnTriggerTutorial()
        {
            GoNextFlag();
            StoryTelling();
            tutorialCondition = tutorialCondition.GetNextCondition();
        }

        void GoNextFlag()
        {
            currentFlag = currentFlag.Next();
        }

        void StoryTelling()
        {
            tutorialChatWindow.UpdateChatStream(tutorialScript.GetScriptData(currentFlag).name, tutorialScript.GetScriptData(currentFlag).text);
        }

        public void ProceedTutorial()
        {
            if(tutorialCondition.isMeetCondition() && tutorialChatWindow.ChatStatus == ChatStatus.DEFAULT)
            {
                OnTriggerTutorial();
            }
        }

        private void Update()
        {
            if(currentFlag.Equals(TutorialFlag.FLAG_END_TUTORIAL)) 
            {
                gameObject.SetActive(false);
            }
            ProceedTutorial();
        }

        public bool IsFlagEqual(TutorialFlag flag)
        {
            return currentFlag.Equals(flag);
        }
    }

}

