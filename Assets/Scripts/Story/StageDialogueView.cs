using System;
using System.Collections;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Story
{
    // 지역 완료 대화는 기존 타이핑 연출을 재사용하고 보상 화면 위에서 재생한다.
    public sealed class StageDialogueView : MonoBehaviour
    {
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

            Image backdrop = DialogueWindowPresentation.Picture("Backdrop", transform, Vector2.zero, Vector2.one, Color.white);
            backdrop.sprite = chapter.background;
            backdrop.raycastTarget = true;
            DialogueWindowPresentation.Picture("Shade", transform, Vector2.zero, Vector2.one,
                new Color(0.04f, 0.03f, 0.09f, 0.35f));
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
            chat.SetPortraits(dialogueData.wordPortrait, dialogueData.villagerPortrait, current.wordSpeaking);
            chat.UpdateChatStream(current.wordSpeaking ? "워드" : "마을 주민", current.text);
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
