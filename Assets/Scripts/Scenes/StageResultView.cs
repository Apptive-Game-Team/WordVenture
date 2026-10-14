using System.Collections;
using System.Collections.Generic;
using Combat.Stage;
using Core;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    // 클리어 대화가 끝난 뒤 점수와 등급, 받은 마력 조각을 차례로 보여 주는 결과 화면. 씬에 두지 않고 Canvas 를 코드로 만든다.
    // 씬이 바뀌면 함께 사라진다. 튜토리얼 Canvas(100)보다 아래에 둔다.
    public sealed class StageResultView : MonoBehaviour
    {
        const int SortingOrder = 20;
        const float RowInterval = 0.25f;
        const float CountUpSeconds = 0.2f;
        const float PunchSeconds = 0.3f;
        static TMP_FontAsset Font => Resources.Load<DialogueWindowStyle>("Story/DialogueWindowStyle").font;
        static string T(string korean) => Localization.Translate(korean);

        struct Row
        {
            public GameObject Root;
            public TextMeshProUGUI Points;
            public int Target;
        }

        readonly List<Row> rows = new List<Row>();
        GameObject totalRoot;
        TextMeshProUGUI gradeLabel;
        GameObject rewardRoot;
        Coroutine reveal;

        // 모든 줄이 나타나 더 건너뛸 것이 없으면 true.
        public bool IsRevealed { get; private set; }

        public static StageResultView Show(StageDataSingleton stage)
        {
            if (stage == null || !stage.hasResult) return null;
            var view = new GameObject("StageResultView", typeof(RectTransform)).AddComponent<StageResultView>();
            view.Build(stage);
            view.reveal = view.StartCoroutine(view.Reveal());
            return view;
        }

        // 연출을 건너뛰고 모든 줄을 최종 값으로 바로 보여 준다.
        public void Skip()
        {
            if (IsRevealed) return;
            if (reveal != null) StopCoroutine(reveal);
            foreach (Row row in rows)
            {
                row.Root.SetActive(true);
                row.Points.text = row.Target.ToString();
            }
            totalRoot.SetActive(true);
            gradeLabel.transform.localScale = Vector3.one;
            rewardRoot.SetActive(true);
            IsRevealed = true;
        }

        void Build(StageDataSingleton stage)
        {
            StageScore score = stage.lastScore;
            StageRewardResult reward = stage.lastReward;
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            DialogueWindowPresentation.ConfigureCanvas(canvas);

            Image frame = DialogueWindowPresentation.Picture("Frame", transform, new Vector2(0.2f, 0.05f),
                new Vector2(0.8f, 0.95f), UiPalette.Border);
            Image panel = DialogueWindowPresentation.Picture("Background", frame.transform, Vector2.zero, Vector2.one, UiPalette.Panel);
            panel.rectTransform.offsetMin = new Vector2(6, 6);
            panel.rectTransform.offsetMax = new Vector2(-6, -6);
            Transform root = panel.transform;

            Label("Title", root, new Vector2(0.03f, 0.88f), new Vector2(0.97f, 0.98f), T("결과"), 60,
                TextAlignmentOptions.Center);

            AddRow(root, 0, T("걸린 턴"),
                string.Format(T("{0}턴 (목표 {1})"), score.Turns, StageReward.TargetTurns(stage.stagePosition)), score.TurnPoints);
            AddRow(root, 1, T("쓴 카드"), string.Format(T("{0}장"), score.CardsUsed), score.CardPoints);
            AddRow(root, 2, T("깎인 HP"), score.DamageTaken.ToString(), score.DamagePoints);
            AddRow(root, 3, T("원소 연계"), string.Format(T("{0}회"), score.Combos), score.ComboPoints);

            int max = StageReward.MaxTurnPoints + StageReward.MaxCardPoints + StageReward.MaxDamagePoints + StageReward.MaxComboPoints;
            totalRoot = Group("Total", root, new Vector2(0.03f, 0.27f), new Vector2(0.97f, 0.40f));
            Label("TotalText", totalRoot.transform, new Vector2(0f, 0f), new Vector2(0.65f, 1f),
                string.Format(T("총점 {0} / {1}"), score.Total, max), 46, TextAlignmentOptions.Right);
            gradeLabel = Label("Grade", totalRoot.transform, new Vector2(0.68f, -0.3f), new Vector2(1f, 1.3f),
                score.Grade.ToString(), 120, TextAlignmentOptions.Center);
            gradeLabel.color = GradeColor(score.Grade);
            gradeLabel.fontStyle = FontStyles.Bold;
            totalRoot.SetActive(false);

            rewardRoot = Group("Reward", root, new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.26f));
            var lines = new List<(string text, int size)>
            {
                (string.Format(T("마력 조각 +{0}"), reward.BaseShards), 44),
            };
            if (reward.IsFirstClear)
                lines.Add((string.Format(T("등급 보너스 +{0}"), reward.GradeBonus), 38));
            else
                lines.Add((string.Format(T("다시 깬 스테이지: 기본의 {0}%"), Mathf.RoundToInt(StageReward.ReplayRate * 100)), 34));
            lines.Add((string.Format(T("보유 마력 조각 {0}"), PlayerUpgrades.Shards), 38));
            float step = 1f / lines.Count;
            for (int i = 0; i < lines.Count; i++)
            {
                float top = 1f - step * i;
                Label("Line" + i, rewardRoot.transform, new Vector2(0f, top - step), new Vector2(1f, top),
                    lines[i].text, lines[i].size, TextAlignmentOptions.Center);
            }
            rewardRoot.SetActive(false);
        }

        void AddRow(Transform parent, int index, string label, string value, int points)
        {
            float top = 0.86f - 0.115f * index;
            GameObject row = Group("Row" + index, parent, new Vector2(0.03f, top - 0.105f), new Vector2(0.97f, top));
            Image background = DialogueWindowPresentation.Picture("Cell", row.transform, Vector2.zero, Vector2.one, UiPalette.Cell);
            Label("Label", background.transform, new Vector2(0.03f, 0f), new Vector2(0.3f, 1f), label, 38, TextAlignmentOptions.Left);
            Label("Value", background.transform, new Vector2(0.3f, 0f), new Vector2(0.78f, 1f), value, 38, TextAlignmentOptions.Center);
            TextMeshProUGUI pointsLabel = Label("Points", background.transform, new Vector2(0.78f, 0f), new Vector2(0.97f, 1f),
                "0", 42, TextAlignmentOptions.Right);
            row.SetActive(false);
            rows.Add(new Row { Root = row, Points = pointsLabel, Target = points });
        }

        IEnumerator Reveal()
        {
            foreach (Row row in rows)
            {
                row.Root.SetActive(true);
                for (float t = 0f; t < CountUpSeconds; t += Time.unscaledDeltaTime)
                {
                    row.Points.text = Mathf.RoundToInt(row.Target * (t / CountUpSeconds)).ToString();
                    yield return null;
                }
                row.Points.text = row.Target.ToString();
                yield return Wait(RowInterval - CountUpSeconds);
            }
            yield return Wait(0.2f);

            totalRoot.SetActive(true);
            Transform grade = gradeLabel.transform;
            for (float t = 0f; t < PunchSeconds; t += Time.unscaledDeltaTime)
            {
                float k = 1f - t / PunchSeconds;
                grade.localScale = Vector3.one * (1f + 1.2f * k * k);
                yield return null;
            }
            grade.localScale = Vector3.one;
            yield return Wait(0.4f);

            rewardRoot.SetActive(true);
            yield return Wait(0.2f);
            IsRevealed = true;
        }

        static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime) yield return null;
        }

        static Color GradeColor(StageGrade grade)
        {
            switch (grade)
            {
                case StageGrade.S: return new Color(1f, 0.82f, 0.25f);
                case StageGrade.A: return new Color(0.55f, 0.9f, 0.45f);
                case StageGrade.B: return new Color(0.5f, 0.75f, 0.95f);
                default: return new Color(0.7f, 0.7f, 0.7f);
            }
        }

        static GameObject Group(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return obj;
        }

        static TextMeshProUGUI Label(string name, Transform parent, Vector2 min, Vector2 max, string text, int size,
            TextAlignmentOptions alignment)
        {
            GameObject obj = Group(name, parent, min, max);
            var label = obj.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.fontSize = size;
            label.color = UiPalette.Text;
            label.alignment = alignment;
            label.enableAutoSizing = true;
            label.fontSizeMax = size;
            label.fontSizeMin = 18;
            label.text = text;
            label.raycastTarget = false;
            return label;
        }
    }
}
