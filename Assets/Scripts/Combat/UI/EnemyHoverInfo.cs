using System.Collections.Generic;
using System.Text;
using Cards;
using Combat.Enemies;
using Core;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Combat.UI
{
    // 전투에서 마우스를 올린 적 슬라임 옆에 이름, 속성, 체력, 공격력, 상태, 마법 상성을 보여 주는 정보 창.
    // 적마다 OnMouseEnter 를 받지 않고 매 프레임 마우스 아래를 직접 확인한다. 죽거나 꺼진 적은 OnMouseExit 를 받지 못하기 때문이다.
    public sealed class EnemyHoverInfo : MonoBehaviour
    {
        // MagicAffinityTable 의 행 순서. 도감과 같은 목록을 쓴다.
        static readonly MagicType[] SpellElements =
            { MagicType.Fire, MagicType.Ice, MagicType.Rock, MagicType.Lightning, MagicType.Holy };
        const int FontSize = 30;
        const float Padding = 28f;
        const float Gap = 16f;
        const float Margin = 12f;

        public static EnemyHoverInfo Current { get; private set; }

        public Enemy Target { get; private set; }
        public BossIntent IntentTarget { get; private set; }
        public bool IsVisible => panel != null && panel.gameObject.activeSelf;
        public string Text => label != null ? label.text : "";

        Canvas canvas;
        RectTransform panel;
        TMP_Text label;
        Enemy underMouse;
        BossIntent underIntent;
        string shownKey = "";
        readonly List<string> lines = new List<string>();
        readonly List<string> items = new List<string>();
        readonly StringBuilder builder = new StringBuilder();

        static TMP_FontAsset Font => Resources.Load<DialogueWindowStyle>("Story/DialogueWindowStyle").font;
        static string T(string korean) => Localization.Translate(korean);

        public static EnemyHoverInfo EnsureExists()
        {
            if (Current != null) return Current;
            var view = new GameObject("EnemyHoverInfo", typeof(RectTransform)).AddComponent<EnemyHoverInfo>();
            view.Build();
            return view;
        }

        public void Show(Enemy enemy)
        {
            Target = enemy;
            IntentTarget = null;
            shownKey = "";
        }

        public void ShowIntent(BossIntent intent)
        {
            Show(intent != null ? intent.GetComponentInParent<Enemy>() : null);
            IntentTarget = intent;
        }
        public void Hide() { Target = null; IntentTarget = null; }

        void Build()
        {
            Current = this;
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // 전투 UI(0~1)보다 위, 대화·튜토리얼(200)과 설정 메뉴보다 아래. GraphicRaycaster 를 두지 않아 클릭을 받지 않는다.
            canvas.sortingOrder = 20;
            DialogueWindowPresentation.ConfigureCanvas(canvas);

            Image frame = DialogueWindowPresentation.Picture("Panel", transform, Vector2.zero, Vector2.zero, UiPalette.Border);
            panel = frame.rectTransform;
            panel.pivot = Vector2.zero;
            Image background = DialogueWindowPresentation.Picture("Background", panel, Vector2.zero, Vector2.one,
                new Color(UiPalette.Panel.r, UiPalette.Panel.g, UiPalette.Panel.b, 0.94f));
            background.rectTransform.offsetMin = new Vector2(4, 4);
            background.rectTransform.offsetMax = new Vector2(-4, -4);

            var obj = new GameObject("Text", typeof(RectTransform));
            obj.transform.SetParent(background.transform, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(Padding - 4, Padding - 4);
            rect.offsetMax = new Vector2(-(Padding - 4), -(Padding - 4));
            label = obj.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.fontSize = FontSize;
            label.color = UiPalette.Text;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            panel.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        void Update()
        {
            // 카드를 끌거나 대상을 누르는 동안에는 마우스 아래에 적이 없는 것으로 본다.
            BossIntent foundIntent = null;
            Enemy found = Input.GetMouseButton(0) ? null : FindEnemyUnderMouse(out foundIntent);
            // 마우스 아래 대상이 바뀐 프레임에만 Show/Hide 를 부른다. 직접 Show 를 부른 경우를 덮어쓰지 않기 위해서다.
            if (found != underMouse || foundIntent != underIntent)
            {
                underMouse = found;
                underIntent = foundIntent;
                if (foundIntent != null) ShowIntent(foundIntent);
                else if (found != null) Show(found); else Hide();
            }

            bool visible = Target != null && Target.IsAlive && !InteractionLock.IsLocked
                && (IntentTarget == null || IntentTarget.gameObject.activeInHierarchy);
            if (panel.gameObject.activeSelf != visible) panel.gameObject.SetActive(visible);
            if (!visible) return;
            Refresh();
            Place();
        }

        static Enemy FindEnemyUnderMouse(out BossIntent intent)
        {
            intent = null;
            Camera camera = Camera.main;
            if (camera == null) return null;
            Vector2 point = camera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(point);
            foreach (Collider2D hit in hits)
            {
                var icon = hit.GetComponent<BossIntent>();
                var enemy = hit.GetComponentInParent<Enemy>();
                if (icon != null && enemy != null && enemy.IsAlive) { intent = icon; return enemy; }
            }
            foreach (Collider2D hit in hits)
            {
                var enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null && enemy.IsAlive) return enemy;
            }
            return null;
        }

        // 체력과 상태가 바뀐 프레임에만 글자를 다시 만든다.
        void Refresh()
        {
            ElementalStatus s = Target.Status;
            string intents = "";
            foreach (BossIntent intent in Target.GetComponentsInChildren<BossIntent>()) intents += intent.Description + "/";
            string key = Target.CurrentHp + "/" + Target.MaximumHp + "/" + Target.CurrentAttackDamage + "/" + s.BurnTurns + "/"
                + s.Chill + "/" + s.Frozen + "/" + s.ShockTurns + "/" + s.FractureTurns + "/" + s.WeakenTurns
                + "/" + Localization.Current + "/" + intents + "/" + (IntentTarget != null ? IntentTarget.GetInstanceID() : 0);
            if (key == shownKey) return;
            shownKey = key;
            label.text = IntentTarget != null ? IntentTarget.Description
                : Describe(Target, CombineZone.Instance != null ? CombineZone.Instance.AffinityTable : null);
            Vector2 size = label.GetPreferredValues(label.text);
            panel.sizeDelta = new Vector2(size.x + Padding * 2, size.y + Padding * 2);
        }

        string Describe(Enemy enemy, MagicAffinityTable affinity)
        {
            lines.Clear();
            lines.Add("<color=#F5D58C>" + T(enemy.DisplayName) + "</color>  " + SlimeEncyclopediaView.ElementName(enemy.enemyType));
            lines.Add(T("체력") + " " + enemy.CurrentHp + " / " + enemy.MaximumHp + "   " + T("공격력") + " " + enemy.CurrentAttackDamage);

            foreach (BossIntent intent in enemy.GetComponentsInChildren<BossIntent>())
                lines.Add(T("다음 행동") + ": " + intent.Description);

            ElementalStatus s = enemy.Status;
            items.Clear();
            if (s.BurnTurns > 0) items.Add(T("화상") + " " + Turns(s.BurnTurns));
            if (s.Frozen) items.Add(T("빙결"));
            else if (s.Chill > 0) items.Add(T("냉기") + " " + s.Chill);
            if (s.ShockTurns > 0) items.Add(T("감전"));
            if (s.FractureTurns > 0) items.Add(T("균열"));
            if (s.Weakened) items.Add(T("약화") + " " + Turns(s.WeakenTurns));
            if (items.Count > 0) lines.Add(T("상태") + ": " + string.Join(", ", items));

            if (affinity != null)
            {
                AddAffinityLine(affinity, enemy.enemyType, "약점", m => m > 1f, true);
                AddAffinityLine(affinity, enemy.enemyType, "저항", m => m > 0f && m < 1f, true);
                AddAffinityLine(affinity, enemy.enemyType, "회복", m => m <= 0f, false);
            }
            builder.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                if (i > 0) builder.Append('\n');
                builder.Append(lines[i]);
            }
            return builder.ToString();
        }

        void AddAffinityLine(MagicAffinityTable affinity, MagicType defense, string heading,
            System.Func<float, bool> matches, bool withMultiplier)
        {
            items.Clear();
            foreach (MagicType spell in SpellElements)
            {
                float multiplier = affinity.GetAffinity(spell, defense);
                if (!matches(multiplier)) continue;
                string name = SlimeEncyclopediaView.ElementName(spell);
                items.Add(withMultiplier ? name + " " + SlimeEncyclopediaView.Multiplier(multiplier) : name);
            }
            if (items.Count > 0) lines.Add(T(heading) + ": " + string.Join(", ", items));
        }

        static string Turns(int count) => Localization.Current == Language.Korean ? count + "턴" : count + (count == 1 ? " turn" : " turns");

        // 슬라임의 왼쪽 위에 놓고, 왼쪽에 자리가 없으면 오른쪽 위에 놓는다. 화면 밖으로 나가지 않게 맞춘다.
        void Place()
        {
            Camera camera = Camera.main;
            if (camera == null) return;
            float scale = canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
            Vector2 size = panel.sizeDelta;
            Component anchor = IntentTarget != null ? (Component)IntentTarget : Target;
            Bounds bounds = anchor.TryGetComponent(out Collider2D collider)
                ? collider.bounds : new Bounds(Target.transform.position, Vector3.zero);
            Vector2 left = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.center.z)) / scale;
            Vector2 right = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.center.z)) / scale;
            float x = left.x - size.x - Gap;
            if (x < Margin) x = right.x + Gap;
            float screenWidth = Screen.width / scale, screenHeight = Screen.height / scale;
            x = Mathf.Clamp(x, Margin, Mathf.Max(Margin, screenWidth - size.x - Margin));
            float y = Mathf.Clamp(left.y + Gap, Margin, Mathf.Max(Margin, screenHeight - size.y - Margin));
            panel.anchoredPosition = new Vector2(x, y);
        }
    }
}
