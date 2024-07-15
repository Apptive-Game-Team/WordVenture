using System.Collections;
using System.Collections.Generic;
using TutorialSystem;
using UnityEngine;

namespace TutorialSystem
{
    public class TutorialController : StoryController
    {
        public static TutorialController Instance;

        [SerializeField] TutorialChatWindow tutorialChatWindow;
        [SerializeField] TutorialScriptContainer tutorialScript;

        [SerializeField] TutorialFlag currentFlag = TutorialFlag.FLAG_001_START_TUTORIAL;
        [SerializeField] ITutorialCondition tutorialCondition;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            if (Map_scene.MapScene.StagePosition > 0)
            {
                gameObject.SetActive(false);
                return;
            }
            StoryTelling();
            tutorialCondition = new TutorialConditon_002();
        }

        public void OnTriggerTutorial()
        {
            tutorialChatWindow.gameObject.SetActive(true);
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
            TutorialChatData tutorialChatData = tutorialScript.GetScriptData(currentFlag);
            tutorialChatWindow.SetSpeakerImage(tutorialScript.GetSprite(tutorialChatData.portraitID));
            tutorialChatWindow.UpdateChatStream(tutorialChatData.name, tutorialChatData.text);
        }

        public void ProceedTutorial()
        {
            if(tutorialCondition.isMeetCondition())
            {
                OnTriggerTutorial();
            }
        }

        private void Update()
        {
            if(currentFlag.Equals(TutorialFlag.FLAG_014_END_TUTORIAL)) 
            {
                gameObject.SetActive(false);
            }

            if (Time.time > tutorialChatWindow.chatRemainTime && tutorialChatWindow.ChatStatus.Equals(ChatStatus.DEFAULT))
            {
                ProceedTutorial();
            }
            if (Time.time > tutorialChatWindow.chatCloseTime && tutorialChatWindow.ChatStatus.Equals(ChatStatus.DEFAULT))
            {
                tutorialChatWindow.gameObject.SetActive(false);
            }
        }

        public bool IsFlagEqual(TutorialFlag flag)
        {
            return currentFlag.Equals(flag);
        }
    }

}

