using System.Collections;
using Cards;
using Combat.Enemies;
using Combat.Stage;
using Combat.UI;
using Core;
using Map;
using Story;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Battle.Turns;

namespace Tutorial
{
    public class TutorialController : StoryController
    {
        public static TutorialController Instance;

        [SerializeField] TutorialChatWindow tutorialChatWindow;
        [SerializeField] TutorialScriptContainer tutorialScript;
        [SerializeField] GameObject inputBlocker;
        [SerializeField] TutorialOverlay overlay;
        [SerializeField] TutorialFlag currentFlag = TutorialFlag.FLAG_001_START_TUTORIAL;
        [SerializeField] bool waitingForAcknowledge;
        [SerializeField] bool castStarted;
        [SerializeField] bool targetSelected;

        CombineZone battleZone;
        CombineButton combineButton;
        TurnBattleSystem turnSystem;
        Button turnEndButton;
        bool turnEnded;
        bool confirmingSkip;
        bool finished;
        Coroutine unlockCoroutine;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            if (Instance == this)
                SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Start()
        {
            if (Instance != this) return;
            if (MapMove.StagePosition > 0 || SaveLoadController.IsTutorialEnded)
            {
                finished = true;
                Destroy(gameObject);
                return;
            }
            overlay.Initialize(this);
            inputBlocker.transform.SetAsFirstSibling();
            BindBattle();
            StoryTelling();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;
            overlay.ClearGuidance();
            if (scene.name == "TitleScene" || scene.name == "GameOverScene")
            {
                Destroy(gameObject);
                return;
            }
            if (scene.name == "TurnBattleScene")
            {
                castStarted = false;
                targetSelected = false;
                turnEnded = false;
            }
            BindBattle();
        }

        void BindBattle()
        {
            UnbindBattle();
            battleZone = FindObjectOfType<CombineZone>(true);
            combineButton = FindObjectOfType<CombineButton>(true);
            turnSystem = FindObjectOfType<TurnBattleSystem>(true);
            if (turnSystem != null)
            {
                turnSystem.PlayerTurnEnded += OnPlayerTurnEnded;
                foreach (Button button in FindObjectsOfType<Button>(true))
                    for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                        if (button.onClick.GetPersistentTarget(i) == turnSystem
                            && button.onClick.GetPersistentMethodName(i) == nameof(TurnBattleSystem.TurnEndButton))
                            turnEndButton = button;
            }
            if (battleZone == null) return;
            battleZone.SpellCastStarted += OnSpellCastStarted;
            battleZone.TargetSelected += OnTargetSelected;
        }

        void UnbindBattle()
        {
            if (turnSystem != null) turnSystem.PlayerTurnEnded -= OnPlayerTurnEnded;
            turnSystem = null;
            turnEndButton = null;
            if (battleZone != null)
            {
                battleZone.SpellCastStarted -= OnSpellCastStarted;
                battleZone.TargetSelected -= OnTargetSelected;
            }
            battleZone = null;
            combineButton = null;
        }

        void OnSpellCastStarted() { castStarted = true; }
        void OnTargetSelected(SelectableObject target) { targetSelected = true; }
        void OnPlayerTurnEnded() { turnEnded = true; }

        TutorialActionState ReadActionState()
        {
            return new TutorialActionState
            {
                battleStarted = SceneManager.GetActiveScene().name == "TurnBattleScene"
                    && StageDataSingleton.Instance != null && StageDataSingleton.Instance.stagePosition == 0,
                handReady = CardManager.Inst != null && CardManager.Inst.HandCards.Count >= 2,
                combineOpen = battleZone != null && battleZone.gameObject.activeInHierarchy,
                hasSpell = battleZone != null && battleZone.spellCards.Count == 1,
                hasElemental = battleZone != null && battleZone.magicTypeCards.Count == 1,
                castStarted = castStarted,
                targetSelected = targetSelected,
                turnEnded = turnEnded,
                battleCleared = SceneManager.GetActiveScene().name == "GameClearScene"
            };
        }

        public void OnTriggerTutorial()
        {
            if (finished || confirmingSkip || waitingForAcknowledge) return;
            currentFlag = currentFlag.Next();
            StoryTelling();
        }

        void StoryTelling()
        {
            overlay.ClearGuidance();
            TutorialChatData data = tutorialScript.GetScriptData(currentFlag);
            SetChatWindowVisible(true);
            tutorialChatWindow.SetSpeakerImage(tutorialScript.GetSprite(data.portraitID));
            tutorialChatWindow.SetAnyKeyPromptVisible(false);
            tutorialChatWindow.UpdateChatStream(data.name, data.text);
            waitingForAcknowledge = true;
        }

        public void ProceedTutorial()
        {
            if (!finished && !confirmingSkip && !waitingForAcknowledge
                && TutorialCondition.CanAdvance(currentFlag, ReadActionState()))
                OnTriggerTutorial();
        }

        void Update()
        {
            if (Instance != this || finished || confirmingSkip) return;
            UpdateGuidance();
            if (waitingForAcknowledge)
            {
                if (!IsAdvanceKeyDown()) return;
                if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                    && overlay.IsSkipPointer(Input.mousePosition)) return;
                AcknowledgeDialogue();
                return;
            }
            ProceedTutorial();
        }

        public void AcknowledgeDialogue()
        {
            if (!waitingForAcknowledge || confirmingSkip || finished) return;
            if (tutorialChatWindow.IsStreaming)
            {
                tutorialChatWindow.CompleteStream();
                return;
            }
            waitingForAcknowledge = false;
            tutorialChatWindow.SetAnyKeyPromptVisible(false);
            if (currentFlag == TutorialFlag.FLAG_013_END_BATTLE) FinishTutorial();
            else SetChatWindowVisible(false);
        }

        void UpdateGuidance()
        {
            if (waitingForAcknowledge)
            {
                overlay.ClearGuidance();
                return;
            }
            TutorialActionState state = ReadActionState();
            if (currentFlag == TutorialFlag.FLAG_010_END_TURN && !state.turnEnded && !state.battleCleared)
            {
                overlay.ShowGuidance(null, turnEndButton != null ? turnEndButton.transform : null,
                    "Turn End 버튼을 눌러 턴을 마치세요.", true);
                return;
            }
            if (currentFlag >= TutorialFlag.FLAG_004_COMBINATION
                && currentFlag <= TutorialFlag.FLAG_007_SET_ELEMENTAL && !state.castStarted)
            {
                if (!state.combineOpen)
                {
                    overlay.ShowGuidance(null, combineButton != null ? combineButton.transform : null, "조합 버튼을 눌러 조합창을 여세요.", true);
                    return;
                }
                if (currentFlag == TutorialFlag.FLAG_004_COMBINATION)
                {
                    overlay.ClearGuidance();
                    return;
                }
                if (!state.hasSpell)
                {
                    GuideCard("Spell", "마법 카드를 첫 번째 칸으로 끌어 놓으세요.");
                    return;
                }
                if (currentFlag >= TutorialFlag.FLAG_006_SET_MAGIC && !state.hasElemental)
                {
                    GuideCard("MagicType", "Fire 속성 카드를 두 번째 칸으로 끌어 놓으세요.");
                    return;
                }
                if (currentFlag == TutorialFlag.FLAG_007_SET_ELEMENTAL)
                {
                    overlay.ShowGuidance(null, battleZone.activateButton.transform, "조합 버튼을 눌러 마법을 시전하세요.", true);
                    return;
                }
            }
            if ((currentFlag == TutorialFlag.FLAG_008_CAST_SPELL || currentFlag == TutorialFlag.FLAG_009_CAST_END)
                && state.castStarted && !state.targetSelected && battleZone != null)
            {
                overlay.ShowGuidance(null, battleZone.GetTutorialTarget(), "강조된 적을 클릭해 공격 대상을 선택하세요.", true);
                return;
            }
            overlay.ClearGuidance();
        }

        void GuideCard(string tag, string message)
        {
            CardManager manager = CardManager.Inst;
            Card card = manager != null ? manager.GetTutorialCard(tag) : null;
            Transform slot = manager != null ? manager.GetTutorialSlot(tag) : null;
            overlay.ShowGuidance(card != null ? card.transform : null, slot,
                card != null ? message : (tag == "Spell" ? "마법 카드가 필요해요. 카드를 뽑으면 안내할게요." : "Fire 속성 카드가 필요해요. 카드를 뽑으면 안내할게요."));
        }

        public void RequestSkip()
        {
            if (finished || confirmingSkip) return;
            confirmingSkip = true;
            LockInput();
            if (CardManager.Inst != null) CardManager.Inst.CancelDrag();
            overlay.ShowConfirmation(true);
        }

        public void CancelSkip()
        {
            if (!confirmingSkip || finished) return;
            confirmingSkip = false;
            overlay.ShowConfirmation(false);
            SetChatWindowVisible(waitingForAcknowledge);
        }

        public void ConfirmSkip()
        {
            if (confirmingSkip) FinishTutorial();
        }

        void FinishTutorial()
        {
            if (finished) return;
            finished = true;
            confirmingSkip = false;
            currentFlag = TutorialFlag.FLAG_014_END_TUTORIAL;
            SaveLoadController.MarkTutorialEnded();
            UnbindBattle();
            tutorialChatWindow.CompleteStream();
            tutorialChatWindow.gameObject.SetActive(false);
            inputBlocker.SetActive(false);
            overlay.Hide();
            LockInput();
            StartCoroutine(FinishAfterFrame());
        }

        IEnumerator FinishAfterFrame()
        {
            yield return null;
            InteractionLock.IsLocked = false;
            Destroy(gameObject);
        }

        void LockInput()
        {
            if (unlockCoroutine != null) StopCoroutine(unlockCoroutine);
            unlockCoroutine = null;
            InteractionLock.IsLocked = true;
        }

        void SetChatWindowVisible(bool visible)
        {
            tutorialChatWindow.gameObject.SetActive(visible);
            inputBlocker.SetActive(visible);
            LockInput();
            if (!visible) unlockCoroutine = StartCoroutine(UnlockAfterFrame());
        }

        IEnumerator UnlockAfterFrame()
        {
            yield return null;
            unlockCoroutine = null;
            if (!confirmingSkip && !waitingForAcknowledge && !finished)
                InteractionLock.IsLocked = false;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnbindBattle();
            if (Instance != this) return;
            StopAllCoroutines();
            unlockCoroutine = null;
            InteractionLock.IsLocked = false;
            if (overlay != null) overlay.Hide();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool IsFlagEqual(TutorialFlag flag) { return currentFlag == flag; }
    }
}
