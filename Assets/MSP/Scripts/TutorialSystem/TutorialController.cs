using System.Collections;
using System.Collections.Generic;
using TutorialSystem;
using UnityEditor.UIElements;
using UnityEngine;

namespace TutorialSystem
{

    public interface ITutorialCondition
    {
        public bool IsTutorialCondition();
    }

    public class TutorialConditon_000 : ITutorialCondition
    {
        public bool IsTutorialCondition()
        {
            return true;
        }
    }

    public class TutorialController : StoryController
    {
        public static TutorialController Instance;

        [SerializeField] TutorialChatWindow tutorialChatWindow;
        [SerializeField] TutorialScriptContainer tutorialScript;

        [SerializeField] TutorialFlag currentFlag = TutorialFlag.FLAG_START_TUTORIAL;
        ITutorialCondition tutorialCondition;

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
            tutorialCondition = new TutorialConditon_000();
        }

        public void OnTriggerTutorial()
        {
            GoNextFlag();
            StoryTelling();
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
            if(tutorialCondition.IsTutorialCondition())
            {
                OnTriggerTutorial();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ProceedTutorial();
            }
        }

        public bool IsFlagEqual(TutorialFlag flag)
        {
            return currentFlag.Equals(flag);
        }
    }

}

