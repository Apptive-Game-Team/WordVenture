using System;
using System.Collections;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Story
{
    // 지역 대화는 기존 타이핑 연출을 재사용하고 지금 씬 위에 덮어서 재생한다.
    public sealed class StageDialogueView : MonoBehaviour
    {
        const string DialogueResource = "Story/ActOneDialogues";

        // 아직 보지 않은 대화만 돌려준다. 없으면 null.
        public static StageDialogueChapter FindUnseen(int stageID, StageDialogueMoment moment, int wave,
            out StageDialogueData data)
        {
            data = Resources.Load<StageDialogueData>(DialogueResource);
            StageDialogueChapter chapter = data != null ? data.FindChapter(stageID, moment, wave) : null;
            if (chapter == null || chapter.lines == null || chapter.lines.Length == 0
                || SaveLoadController.HasSeenStageDialogue(chapter.SeenBit))
                return null;
            return chapter;
        }

        // 다른 대화나 튜토리얼이 입력을 잡고 있으면 끝나기를 기다린 뒤 재생하고, 다 보면 읽음으로 기록한다.
        public static IEnumerator Play(StageDialogueData data, StageDialogueChapter chapter)
        {
            if (InteractionLock.IsLocked) yield return new WaitUntil(() => !InteractionLock.IsLocked);
            bool completed = false;
            var view = new GameObject("StageDialogue", typeof(RectTransform)).AddComponent<StageDialogueView>();
            view.Begin(data, chapter, () => completed = true);
            yield return new WaitUntil(() => completed);
            SaveLoadController.MarkStageDialogueSeen(chapter.SeenBit);
        }

        ChatWindowController chat;
        StageDialogueChapter chapter;
        StageDialogueData dialogueData;
        Action completed;
        int line;
        bool ownsLock;
        bool finishing;

        public void Begin(StageDialogueData data, StageDialogueChapter dialogue, Action onCompleted)
        {
            chapter = dialogue;
            dialogueData = data;
            completed = onCompleted;
            Build(data);
            ownsLock = true;
            InteractionLock.IsLocked = true;
            ShowLine();
        }

        void Build(StageDialogueData data)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            DialogueWindowPresentation.ConfigureCanvas(canvas);
            gameObject.AddComponent<GraphicRaycaster>();

            // 배경이 없는 대화(전투 중 마왕 대화 등)는 뒤의 씬이 비쳐 보이게 한다.
            if (chapter.background != null)
            {
                Image backdrop = DialogueWindowPresentation.Picture("Backdrop", transform, Vector2.zero, Vector2.one, Color.white);
                backdrop.sprite = chapter.background;
            }
            Image shade = DialogueWindowPresentation.Picture("Shade", transform, Vector2.zero, Vector2.one,
                new Color(0.04f, 0.03f, 0.09f, 0.35f));
            // 대화 중 클릭이 뒤의 맵이나 전투로 전달되지 않게 막는다.
            shade.raycastTarget = true;
            Label("ChapterTitle", transform, new Vector2(0.05f, 0.89f), new Vector2(0.95f, 0.97f),
                chapter.title, data.font, 38, new Color(1f, 0.88f, 0.63f));

            var window = new GameObject("ChatWindow", typeof(RectTransform));
            window.transform.SetParent(transform, false);
            chat = window.AddComponent<ChatWindowController>();
        }

        static TMP_Text Label(string name, Transform parent, Vector2 min, Vector2 max,
            string text, TMP_FontAsset font, int size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = obj.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = size;
            label.color = color;
            label.text = text;
            label.raycastTarget = false;
            return label;
        }

        void ShowLine()
        {
            StageDialogueLine current = chapter.lines[line];
            chat.SetPortraits(dialogueData.wordPortrait, chapter.speakerPortrait, current.wordSpeaking);
            chat.UpdateChatStream(current.wordSpeaking ? "워드" : chapter.speakerName, current.text);
        }

        void Update()
        {
            if (chat == null || finishing) return;
            if (Input.anyKeyDown) Advance();
        }

        public void Advance()
        {
            if (chat == null || finishing) return;
            if (chat.IsStreaming)
            {
                chat.CompleteStream();
                return;
            }
            if (++line < chapter.lines.Length) ShowLine();
            else
            {
                finishing = true;
                StartCoroutine(FinishAfterFrame());
            }
        }

        IEnumerator FinishAfterFrame()
        {
            // 마지막 입력이 클리어 화면이나 다음 씬으로 전달되지 않도록 소비한다.
            yield return null;
            ownsLock = false;
            InteractionLock.IsLocked = false;
            completed?.Invoke();
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (ownsLock) InteractionLock.IsLocked = false;
        }
    }
}
