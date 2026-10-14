using Combat.Stage;
using Core;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    // 클리어 화면에서 받은 마력 조각을 보여 주는 패널. 씬에 두지 않고 Canvas 를 코드로 만든다.
    // 씬이 바뀌면 함께 사라진다. 튜토리얼 Canvas(100)보다 아래에 둔다.
    public sealed class StageRewardView : MonoBehaviour
    {
        const int SortingOrder = 20;
        static TMP_FontAsset Font => Resources.Load<DialogueWindowStyle>("Story/DialogueWindowStyle").font;
        static string T(string korean) => Localization.Translate(korean);

        public static StageRewardView Show(StageDataSingleton stage)
        {
            if (stage == null || stage.lastReward.Total <= 0) return null;
            var view = new GameObject("StageRewardView", typeof(RectTransform)).AddComponent<StageRewardView>();
            view.Build(stage);
            return view;
        }

        void Build(StageDataSingleton stage)
        {
            StageRewardResult reward = stage.lastReward;
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            DialogueWindowPresentation.ConfigureCanvas(canvas);

            Image frame = DialogueWindowPresentation.Picture("Frame", transform, new Vector2(0.3f, 0.66f),
                new Vector2(0.7f, 0.96f), UiPalette.Border);
            Image panel = DialogueWindowPresentation.Picture("Background", frame.transform, Vector2.zero, Vector2.one, UiPalette.Panel);
            panel.rectTransform.offsetMin = new Vector2(6, 6);
            panel.rectTransform.offsetMax = new Vector2(-6, -6);

            var lines = new System.Collections.Generic.List<(string text, int size)>
            {
                (string.Format(T("마력 조각 +{0}"), reward.BaseShards), 56),
            };
            if (reward.IsFastClear)
                lines.Add((string.Format(T("빠른 클리어 보너스 +{0} ({1}턴 이내)"), reward.FastClearBonus,
                    StageReward.FastClearTurns(stage.stagePosition)), 38));
            if (reward.IsFirstClear)
                lines.Add((string.Format(T("걸린 턴 {0}"), stage.lastClearTurns), 32));
            else
                lines.Add((string.Format(T("다시 깬 스테이지: 기본의 {0}%"), Mathf.RoundToInt(StageReward.ReplayRate * 100)), 34));
            lines.Add((string.Format(T("보유 마력 조각 {0}"), PlayerUpgrades.Shards), 38));

            float step = 1f / lines.Count;
            for (int i = 0; i < lines.Count; i++)
            {
                float top = 1f - step * i;
                Label("Line" + i, panel.transform, new Vector2(0.03f, top - step), new Vector2(0.97f, top),
                    lines[i].text, lines[i].size);
            }
        }

        static void Label(string name, Transform parent, Vector2 min, Vector2 max, string text, int size)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = obj.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.fontSize = size;
            label.color = UiPalette.Text;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMax = size;
            label.fontSizeMin = 18;
            label.text = text;
            label.raycastTarget = false;
        }
    }
}
