using System.Collections.Generic;
using Core;
using DG.Tweening;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Map
{
    // 맵 화면에서 여는 업그레이드 창. 스테이지를 깨고 받은 마력 조각으로 PlayerUpgrades 의 능력치를 올린다.
    // 씬에 두지 않고 열 때마다 Canvas 를 코드로 만들고, 닫으면 지운다.
    public sealed class UpgradeView : MonoBehaviour
    {
        const string WoodButtonPath = "UI/WoodButtonSmall";
        static readonly Color PipFilled = new Color(0.96f, 0.84f, 0.55f);
        static readonly Color PipEmpty = new Color(0f, 0f, 0f, 0.4f);

        sealed class Row
        {
            public UpgradeKind kind;
            public TMP_Text values;
            public Image[] pips;
            public Button button;
            public TMP_Text buttonLabel;
        }

        public static UpgradeView Current { get; private set; }

        readonly List<Row> rows = new List<Row>();
        TMP_Text shardsLabel;
        int openedFrame;
        bool ownsLock;

        static TMP_FontAsset Font => Resources.Load<DialogueWindowStyle>("Story/DialogueWindowStyle").font;
        static string T(string korean) => Localization.Translate(korean);

        // 대화나 튜토리얼이 입력을 잡고 있으면 열지 않는다.
        public static UpgradeView Open()
        {
            if (Current != null || InteractionLock.IsLocked) return Current;
            var view = new GameObject("UpgradeView", typeof(RectTransform)).AddComponent<UpgradeView>();
            view.Build();
            return view;
        }

        // 맵 오른쪽 위, 슬라임 도감 버튼(오른쪽 끝 400 폭) 바로 왼쪽에 업그레이드 창을 여는 버튼을 만든다.
        public static Button CreateOpenButton(UnityAction open)
        {
            var obj = new GameObject("UpgradeButton", typeof(RectTransform));
            SetUpCanvas(obj, 10);
            Button button = WoodButton(obj.transform, T("업그레이드 (U)"), 40);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-460, -40);
            rect.sizeDelta = new Vector2(400, 90);
            button.onClick.AddListener(open);
            return button;
        }

        public void Close() => Destroy(gameObject);

        void Build()
        {
            Current = this;
            openedFrame = Time.frameCount;
            ownsLock = true;
            InteractionLock.IsLocked = true;

            SetUpCanvas(gameObject, 150);
            Image shade = DialogueWindowPresentation.Picture("Shade", transform, Vector2.zero, Vector2.one,
                new Color(0.04f, 0.03f, 0.09f, 0.6f));
            // 창 뒤의 맵 지점과 버튼이 클릭되지 않게 막는다.
            shade.raycastTarget = true;
            Image frame = DialogueWindowPresentation.Picture("Window", transform, new Vector2(0.12f, 0.08f),
                new Vector2(0.88f, 0.92f), UiPalette.Border);
            Image window = DialogueWindowPresentation.Picture("Background", frame.transform, Vector2.zero, Vector2.one, UiPalette.Panel);
            Inset(window.rectTransform, 6);

            Label("Title", window.transform, new Vector2(0.025f, 0.87f), new Vector2(0.4f, 0.98f), T("업그레이드"), 52, UiPalette.Text)
                .alignment = TextAlignmentOptions.MidlineLeft;
            shardsLabel = Label("Shards", window.transform, new Vector2(0.4f, 0.87f), new Vector2(0.75f, 0.98f), "", 36, UiPalette.Text);
            shardsLabel.alignment = TextAlignmentOptions.MidlineRight;
            Button close = WoodButton(window.transform, T("닫기 (Esc)"), 34);
            Place((RectTransform)close.transform, new Vector2(0.78f, 0.885f), new Vector2(0.975f, 0.975f));
            close.onClick.AddListener(Close);

            Image list = DialogueWindowPresentation.Picture("List", window.transform, new Vector2(0.025f, 0.17f),
                new Vector2(0.975f, 0.86f), new Color(0f, 0f, 0f, 0.25f));
            int count = PlayerUpgrades.Kinds.Length;
            for (int i = 0; i < count; i++)
            {
                float top = 1f - (float)i / count;
                rows.Add(BuildRow(list.transform, PlayerUpgrades.Kinds[i], top - 1f / count + 0.01f, top - 0.01f));
            }

            TMP_Text hint = Label("Hint", window.transform, new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.15f),
                T("스테이지를 깨면 마력 조각을 얻습니다. 결과 등급이 높을수록 더 받습니다."), 30, Color.white);
            hint.alignment = TextAlignmentOptions.MidlineLeft;
            hint.enableAutoSizing = true;
            hint.fontSizeMin = 18;
            hint.fontSizeMax = 30;
            Refresh();
        }

        Row BuildRow(Transform parent, UpgradeKind kind, float yMin, float yMax)
        {
            Image background = DialogueWindowPresentation.Picture(kind.ToString(), parent, new Vector2(0.01f, yMin),
                new Vector2(0.99f, yMax), UiPalette.Cell);
            Transform row = background.transform;
            var result = new Row { kind = kind };

            TMP_Text name = Label("Name", row, new Vector2(0.02f, 0f), new Vector2(0.21f, 1f), T(KindName(kind)), 40, UiPalette.Text);
            name.alignment = TextAlignmentOptions.MidlineLeft;
            name.enableAutoSizing = true;
            name.fontSizeMin = 24;
            name.fontSizeMax = 40;

            result.values = Label("Values", row, new Vector2(0.22f, 0f), new Vector2(0.58f, 1f), "", 32, Color.white);
            result.values.alignment = TextAlignmentOptions.MidlineLeft;
            result.values.enableAutoSizing = true;
            result.values.fontSizeMin = 18;
            result.values.fontSizeMax = 32;

            // 단계 표시는 글리프 대신 작은 사각형으로 그려 폰트에 ●○ 가 없어도 보인다.
            var pipRow = (RectTransform)new GameObject("Pips", typeof(RectTransform)).transform;
            pipRow.SetParent(row, false);
            Place(pipRow, new Vector2(0.59f, 0.3f), new Vector2(0.77f, 0.7f));
            var layout = pipRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            result.pips = new Image[PlayerUpgrades.MaxLevel(kind)];
            for (int i = 0; i < result.pips.Length; i++)
            {
                Image pip = DialogueWindowPresentation.Picture("Pip" + i, pipRow, Vector2.zero, Vector2.one, PipEmpty);
                var element = pip.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = element.preferredHeight = 22;
                result.pips[i] = pip;
            }

            result.button = WoodButton(row, "", 34);
            Place((RectTransform)result.button.transform, new Vector2(0.79f, 0.12f), new Vector2(0.98f, 0.88f));
            result.buttonLabel = result.button.GetComponentInChildren<TMP_Text>();
            UpgradeKind captured = kind;
            result.button.onClick.AddListener(() => Upgrade(captured));
            return result;
        }

        void Upgrade(UpgradeKind kind)
        {
            if (!PlayerUpgrades.TryUpgrade(kind)) return;
            Refresh();
            foreach (Row row in rows)
            {
                if (row.kind != kind) continue;
                row.button.transform.DOKill(true);
                row.button.transform.DOPunchScale(Vector3.one * 0.12f, 0.2f).SetLink(row.button.gameObject);
            }
        }

        void Refresh()
        {
            shardsLabel.text = string.Format(T("보유 마력 조각 {0}"), PlayerUpgrades.Shards);
            foreach (Row row in rows)
            {
                UpgradeKind kind = row.kind;
                bool locked = !PlayerUpgrades.IsUnlocked(kind);
                int level = PlayerUpgrades.GetLevel(kind);
                row.values.text = locked ? T("Spawn 주문을 얻으면 열립니다") : Describe(kind, level);
                for (int i = 0; i < row.pips.Length; i++) row.pips[i].color = !locked && i < level ? PipFilled : PipEmpty;
                int cost = PlayerUpgrades.NextCost(kind);
                if (locked) row.buttonLabel.text = T("잠김");
                else row.buttonLabel.text = cost < 0 ? T("최대") : string.Format(T("{0} 조각"), cost);
                row.button.interactable = !locked && PlayerUpgrades.CanUpgrade(kind);
            }
        }

        static string KindName(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Attack: return "공격력";
                case UpgradeKind.MaxHp: return "최대 HP";
                case UpgradeKind.HandLimit: return "손패 상한";
                default: return "아군 슬라임";
            }
        }

        // 현재 값 → 한 단계 올린 값. 최대 단계면 현재 값만 보인다.
        static string Describe(UpgradeKind kind, int level)
        {
            bool maxed = level >= PlayerUpgrades.MaxLevel(kind);
            switch (kind)
            {
                case UpgradeKind.Attack:
                {
                    int now = PlayerUpgrades.SpellBaseDamage;
                    return maxed ? string.Format(T("주문 피해 {0}"), now)
                        : string.Format(T("주문 피해 {0} > {1}"), now, now + PlayerUpgrades.SpellDamagePerLevel);
                }
                case UpgradeKind.MaxHp:
                {
                    int now = PlayerUpgrades.PlayerMaxHp;
                    return maxed ? string.Format(T("HP {0}"), now)
                        : string.Format(T("HP {0} > {1}"), now, now + PlayerUpgrades.PlayerMaxHpPerLevel);
                }
                case UpgradeKind.HandLimit:
                {
                    int now = PlayerUpgrades.HandLimit;
                    return maxed ? string.Format(T("손패 {0}장"), now)
                        : string.Format(T("손패 {0}장 > {1}장"), now, now + PlayerUpgrades.HandLimitPerLevel);
                }
                default:
                {
                    int hp = PlayerUpgrades.AllyMaxHp;
                    int damage = PlayerUpgrades.AllyAttackDamage;
                    return maxed ? string.Format(T("슬라임 HP {0}, 공격력 {1}"), hp, damage)
                        : string.Format(T("슬라임 HP {0} > {1}, 공격력 {2} > {3}"), hp, hp + PlayerUpgrades.AllyMaxHpPerLevel,
                            damage, damage + PlayerUpgrades.AllyAttackDamagePerLevel);
                }
            }
        }

        void Update()
        {
            // 창을 연 U 키 입력으로 바로 닫히지 않게 한다.
            if (Time.frameCount == openedFrame) return;
            // 설정 메뉴가 위에 열려 있으면 Esc 키는 메뉴가 받는다.
            if (InteractionLock.IsPaused) return;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.U)) Close();
        }

        // 맵이 같은 프레임에 U 키를 다시 읽어 창을 또 열지 않도록 잠금은 오브젝트가 지워질 때 푼다.
        void OnDestroy()
        {
            if (Current == this) Current = null;
            if (ownsLock) InteractionLock.IsLocked = false;
        }

        static void SetUpCanvas(GameObject obj, int sortingOrder)
        {
            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            DialogueWindowPresentation.ConfigureCanvas(canvas);
            obj.AddComponent<GraphicRaycaster>();
        }

        static Button WoodButton(Transform parent, string text, int size)
        {
            Button button = Instantiate(Resources.Load<Button>(WoodButtonPath), parent, false);
            button.name = "Button";
            Place((RectTransform)button.transform, Vector2.zero, Vector2.one);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            label.text = text;
            label.fontSize = size;
            return button;
        }

        static void Inset(RectTransform rect, float border)
        {
            rect.offsetMin = new Vector2(border, border);
            rect.offsetMax = new Vector2(-border, -border);
        }

        static void Place(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static TMP_Text Label(string name, Transform parent, Vector2 min, Vector2 max, string text, int size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            Place((RectTransform)obj.transform, min, max);
            var label = obj.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.fontSize = size;
            label.color = color;
            label.text = text;
            label.raycastTarget = false;
            return label;
        }
    }
}
