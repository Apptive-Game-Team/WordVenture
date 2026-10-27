using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Cards;
using Combat.Enemies;
using Combat.Stage;
using Core;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Combat.UI
{
    // 맵 화면에서 여는 슬라임 도감. 전투에서 만난 슬라임(SaveLoadController.HasSeenSlime)의 수치, 공격 방식, 마법 상성을 보여 준다.
    // 씬에 두지 않고 열 때마다 Canvas 를 코드로 만들고, 닫으면 지운다.
    public sealed class SlimeEncyclopediaView : MonoBehaviour
    {
        const int Columns = 5;
        const string Unknown = "???";
        // MagicAffinityTable 의 행 순서. 플레이어가 쓰는 원소 마법이다.
        static readonly MagicType[] SpellElements =
            { MagicType.Fire, MagicType.Ice, MagicType.Rock, MagicType.Lightning, MagicType.Holy };
        static readonly Color Silhouette = new Color(0f, 0f, 0f, 0.85f);

        public static SlimeEncyclopediaView Current { get; private set; }

        readonly List<EnemyData> entries = new List<EnemyData>();
        readonly List<Image> cells = new List<Image>();
        MagicAffinityTable affinity;
        ScrollRect list;
        GridLayoutGroup grid;
        float fittedWidth;
        Image portrait;
        TMP_Text nameLabel;
        TMP_Text elementLabel;
        TMP_Text statsLabel;
        TMP_Text movementLabel;
        TMP_Text detailLabel;
        int selected = -1;
        int openedFrame;
        bool ownsLock;

        const string WoodButtonPath = "UI/WoodButtonSmall";
        static TMP_FontAsset Font => Resources.Load<DialogueWindowStyle>("Story/DialogueWindowStyle").font;
        static string T(string korean) => Localization.Translate(korean);

        // 대화나 튜토리얼이 입력을 잡고 있으면 열지 않는다.
        public static SlimeEncyclopediaView Open(EnemyDataContainer enemies, MagicAffinityTable affinityTable)
        {
            if (Current != null || InteractionLock.IsLocked || enemies == null || affinityTable == null) return Current;
            var view = new GameObject("SlimeEncyclopedia", typeof(RectTransform)).AddComponent<SlimeEncyclopediaView>();
            view.Build(enemies, affinityTable);
            return view;
        }

        // 맵 오른쪽 위에 도감을 여는 버튼을 만든다. 도감 창이 이 버튼보다 위에 그려진다.
        public static Button CreateOpenButton(UnityAction open)
        {
            var obj = new GameObject("SlimeEncyclopediaButton", typeof(RectTransform));
            SetUpCanvas(obj, 10);
            Button button = WoodButton(obj.transform, T("슬라임 도감 (E)"), 40);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-40, -40);
            rect.sizeDelta = new Vector2(400, 90);
            button.onClick.AddListener(open);
            return button;
        }

        // 도감 기록을 넣기 전에 진행한 세이브도 채우도록, 이미 깬 스테이지(stageID < stagePosition)의 적을 기록한다.
        // 스테이지를 깼다면 모든 웨이브의 적과, 그 적이 웨이브 도중에 부른 적을 만났다. 이미 기록한 적은 다시 저장하지 않는다.
        public static void RecordClearedStages(IEnumerable<StageData> stages, EnemyDataContainer enemies, int stagePosition)
        {
            if (stages == null || enemies == null) return;
            var pending = new Stack<int>();
            foreach (StageData stage in stages)
            {
                if (stage == null || stage.stageID >= stagePosition || stage.waveData?.enemyWaves == null
                    || stage.waveData.enemyWaves.Length == 0 || stage.waveData.enemyWaves[0] == null) continue;
                // StageManager 는 enemyWaves[0] 만 전투에 쓴다.
                foreach (BattleWaveData wave in stage.waveData.enemyWaves[0].GetBattleWaveDatas())
                    foreach (EnemySpawnData spawn in wave.enemySpawnDatasInWave)
                        pending.Push(spawn.enemyId);
            }

            var recorded = new HashSet<int>();
            while (pending.Count > 0)
            {
                int id = pending.Pop();
                if (id < 0 || id >= enemies.GetGearNum() || !recorded.Add(id)) continue;
                SaveLoadController.MarkSlimeSeen(id);
                GameObject prefab = enemies.GetGearData(id).prefab;
                Enemy enemy = prefab != null ? prefab.GetComponent<Enemy>() : null;
                if (enemy != null) pending.Push(enemy.SummonedEnemyId);
            }
        }

        public void Close() => Destroy(gameObject);

        void Build(EnemyDataContainer enemies, MagicAffinityTable affinityTable)
        {
            Current = this;
            affinity = affinityTable;
            openedFrame = Time.frameCount;
            ownsLock = true;
            InteractionLock.IsLocked = true;
            int seen = 0;
            foreach (EnemyData data in enemies.All)
            {
                if (string.IsNullOrEmpty(data.displayName) || data.prefab == null) continue;
                entries.Add(data);
                if (SaveLoadController.HasSeenSlime(data.id)) seen++;
            }

            SetUpCanvas(gameObject, 150);
            Image shade = DialogueWindowPresentation.Picture("Shade", transform, Vector2.zero, Vector2.one,
                new Color(0.04f, 0.03f, 0.09f, 0.6f));
            // 도감 뒤의 맵 지점과 버튼이 클릭되지 않게 막는다.
            shade.raycastTarget = true;
            Image frame = DialogueWindowPresentation.Picture("Window", transform, new Vector2(0.05f, 0.06f),
                new Vector2(0.95f, 0.94f), UiPalette.Border);
            Image window = DialogueWindowPresentation.Picture("Background", frame.transform, Vector2.zero, Vector2.one, UiPalette.Panel);
            Inset(window.rectTransform, 6);

            Label("Title", window.transform, new Vector2(0.025f, 0.87f), new Vector2(0.4f, 0.98f), T("슬라임 도감"), 52, UiPalette.Text)
                .alignment = TextAlignmentOptions.MidlineLeft;
            TMP_Text count = Label("Count", window.transform, new Vector2(0.4f, 0.87f), new Vector2(0.75f, 0.98f),
                T("발견") + " " + seen + " / " + entries.Count, 36, UiPalette.Text);
            count.alignment = TextAlignmentOptions.MidlineRight;
            Button close = WoodButton(window.transform, T("닫기 (Esc)"), 34);
            Place((RectTransform)close.transform, new Vector2(0.78f, 0.885f), new Vector2(0.975f, 0.975f));
            close.onClick.AddListener(Close);

            BuildList(window.transform, new Vector2(0.025f, 0.035f), new Vector2(0.5f, 0.86f));
            BuildDetail(window.transform, new Vector2(0.52f, 0.035f), new Vector2(0.975f, 0.86f));
            Select(Mathf.Max(0, entries.FindIndex(data => SaveLoadController.HasSeenSlime(data.id))));
        }

        void BuildList(Transform parent, Vector2 min, Vector2 max)
        {
            Image area = DialogueWindowPresentation.Picture("List", parent, min, max, new Color(0f, 0f, 0f, 0.25f));
            // 마우스 휠과 끌기를 받는다.
            area.raycastTarget = true;
            area.gameObject.AddComponent<RectMask2D>();

            var content = (RectTransform)new GameObject("Content", typeof(RectTransform)).transform;
            content.SetParent(area.transform, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = content.offsetMax = Vector2.zero;
            grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(14, 14, 14, 14);
            grid.spacing = new Vector2(10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            list = area.gameObject.AddComponent<ScrollRect>();
            list.content = content;
            list.viewport = area.rectTransform;
            list.horizontal = false;
            list.movementType = ScrollRect.MovementType.Clamped;
            list.scrollSensitivity = 40;
            for (int i = 0; i < entries.Count; i++) cells.Add(BuildCell(content, i));
        }

        Image BuildCell(Transform parent, int index)
        {
            EnemyData data = entries[index];
            bool seen = SaveLoadController.HasSeenSlime(data.id);
            var obj = new GameObject("Slime" + data.id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            var background = obj.GetComponent<Image>();
            background.color = UiPalette.Cell;
            Image picture = DialogueWindowPresentation.Picture("Picture", obj.transform, new Vector2(0.06f, 0.3f),
                new Vector2(0.94f, 0.96f), seen ? BodyColor(data) : Silhouette);
            picture.sprite = Portrait(data);
            picture.preserveAspect = true;
            TMP_Text label = Label("Name", obj.transform, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.3f),
                seen ? T(data.displayName) : Unknown, 22, UiPalette.Text);
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12;
            label.fontSizeMax = 22;
            var button = obj.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(() => Select(index));
            return background;
        }

        void BuildDetail(Transform parent, Vector2 min, Vector2 max)
        {
            Image panel = DialogueWindowPresentation.Picture("Detail", parent, min, max, new Color(0f, 0f, 0f, 0.25f));
            Transform detail = panel.transform;
            portrait = DialogueWindowPresentation.Picture("Portrait", detail, new Vector2(0.03f, 0.64f),
                new Vector2(0.36f, 0.97f), Color.white);
            portrait.preserveAspect = true;
            nameLabel = Label("Name", detail, new Vector2(0.4f, 0.86f), new Vector2(0.97f, 0.97f), "", 44, UiPalette.Text);
            nameLabel.enableAutoSizing = true;
            nameLabel.fontSizeMin = 24;
            nameLabel.fontSizeMax = 44;
            elementLabel = Label("Element", detail, new Vector2(0.4f, 0.77f), new Vector2(0.97f, 0.86f), "", 32, Color.white);
            statsLabel = StatLabel("Stats", detail, new Vector2(0.4f, 0.62f), new Vector2(0.62f, 0.77f));
            movementLabel = StatLabel("Movement", detail, new Vector2(0.62f, 0.62f), new Vector2(0.97f, 0.77f));
            detailLabel = Label("Description", detail, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.6f), "", 28, Color.white);
            detailLabel.alignment = TextAlignmentOptions.TopLeft;
            detailLabel.enableAutoSizing = true;
            detailLabel.fontSizeMin = 18;
            detailLabel.fontSizeMax = 28;
        }

        public void Select(int index)
        {
            if (index < 0 || index >= entries.Count) return;
            if (selected >= 0) cells[selected].color = UiPalette.Cell;
            selected = index;
            cells[index].color = UiPalette.SelectedCell;

            EnemyData data = entries[index];
            bool seen = SaveLoadController.HasSeenSlime(data.id);
            portrait.sprite = Portrait(data);
            portrait.color = seen ? BodyColor(data) : Silhouette;
            nameLabel.text = seen ? T(data.displayName) : Unknown;
            elementLabel.text = seen ? T("속성") + ": " + ElementName(data.type) : "";
            statsLabel.text = seen ? T("체력") + " " + data.maxHp + "\n" + T("공격력") + " " + data.damage : "";
            movementLabel.text = seen ? Movement(data) : "";
            detailLabel.text = seen ? Details(data) : T("아직 만나지 않은 슬라임이다. 전투에서 만나면 도감에 기록된다.");
        }

        void Update()
        {
            FitCells();
            // 도감을 연 E 키 입력으로 바로 닫히지 않게 한다.
            if (Time.frameCount == openedFrame) return;
            // 설정 메뉴가 위에 열려 있으면 Esc 키와 방향키는 메뉴가 받는다.
            if (InteractionLock.IsPaused) return;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) Close();
            else if (Input.GetKeyDown(KeyCode.RightArrow)) Step(1);
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) Step(-1);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) Step(Columns);
            else if (Input.GetKeyDown(KeyCode.UpArrow)) Step(-Columns);
        }

        // 화면 비율에 따라 목록 너비가 달라지므로 한 줄에 Columns 칸이 들어가게 칸 크기를 맞춘다.
        void FitCells()
        {
            float width = list.viewport.rect.width;
            if (Mathf.Approximately(width, fittedWidth)) return;
            fittedWidth = width;
            float cell = (width - grid.padding.horizontal - grid.spacing.x * (Columns - 1)) / Columns;
            grid.cellSize = new Vector2(cell, cell * 1.15f);
        }

        void Step(int step)
        {
            int next = Mathf.Clamp(selected + step, 0, entries.Count - 1);
            if (next == selected) return;
            Select(next);
            int rows = (entries.Count + Columns - 1) / Columns;
            if (rows > 1) list.verticalNormalizedPosition = 1f - (float)(next / Columns) / (rows - 1);
        }

        // 맵이 같은 프레임에 E 키를 다시 읽어 도감을 또 열지 않도록 잠금은 오브젝트가 지워질 때 푼다.
        void OnDestroy()
        {
            if (Current == this) Current = null;
            if (ownsLock) InteractionLock.IsLocked = false;
        }

        static string Movement(EnemyData data)
        {
            string move = data.moveDistance <= 0f ? T("없음") : Number(data.moveDistance);
            // 곡사·치유 슬라임은 사거리 99 로 어디서든 행동한다.
            string range = data.attackRange >= 99f ? T("화면 전체") : Number(data.attackRange);
            return T("이동 거리") + " " + move + "\n" + T("사거리") + " " + range;
        }

        string Details(EnemyData data)
        {
            Enemy enemy = data.prefab.GetComponent<Enemy>();
            var text = new StringBuilder();
            text.Append(Heading("공격 방식"));
            if (enemy != null) text.Append(T(enemy.AttackDescription));
            // ElementalStatus.Hit 은 보스를 얼리지 않고 2턴 동안 공격 약화를 건다.
            if (enemy is BossEnemy) text.Append(' ').Append(T("보스라서 빙결되지 않고, 냉기가 다 쌓이면 대신 공격력이 30% 줄어든다."));

            var weak = new List<string>();
            var resisted = new List<string>();
            var healed = new List<string>();
            foreach (MagicType element in SpellElements)
            {
                float multiplier = affinity.GetAffinity(element, data.type);
                if (multiplier > 1f) weak.Add(ElementName(element) + " " + Multiplier(multiplier));
                else if (multiplier <= 0f) healed.Add(ElementName(element));
                else if (multiplier < 1f) resisted.Add(ElementName(element) + " " + Multiplier(multiplier));
            }
            text.Append("\n\n").Append(Heading("마법 상성"));
            text.Append(T("약점")).Append(": ").Append(Join(weak)).Append('\n');
            text.Append(T("저항")).Append(": ").Append(Join(resisted));
            if (healed.Count > 0) text.Append('\n').Append(T("맞으면 회복")).Append(": ").Append(Join(healed));
            string resistance = new ElementalStatus(data.type).ResistanceDescription;
            if (resistance != "") text.Append('\n').Append(T(resistance));
            return text.ToString();
        }

        static string Heading(string korean) => "<color=#F5D58C>" + T(korean) + "</color>\n";
        static string Join(List<string> items) => items.Count > 0 ? string.Join(", ", items) : T("없음");
        static string Number(float value) => value.ToString("0.#", CultureInfo.InvariantCulture);

        public static string Multiplier(float value) => Localization.Current == Language.Korean
            ? Number(value) + "배"
            : "x" + Number(value);

        public static string ElementName(MagicType type)
        {
            switch (type)
            {
                case MagicType.Fire: return "<color=#FF9955>" + T("불") + "</color>";
                case MagicType.Ice: return "<color=#88DDFF>" + T("얼음") + "</color>";
                case MagicType.Rock: return "<color=#DDBB88>" + T("바위") + "</color>";
                case MagicType.Lightning: return "<color=#FFFF77>" + T("번개") + "</color>";
                case MagicType.Holy: return "<color=#FFF4C8>" + T("신성") + "</color>";
                case MagicType.Undead: return "<color=#CCAAFF>" + T("언데드") + "</color>";
                case MagicType.Water: return "<color=#66AAFF>" + T("물") + "</color>";
                case MagicType.Plant: return "<color=#88DD77>" + T("풀") + "</color>";
                default: return type.ToString();
            }
        }

        // 여러 prefab 의 SpriteRenderer 에는 임시 그림이 있으므로 SlimeAnimator 의 첫 프레임을 먼저 쓴다.
        static Sprite Portrait(EnemyData data)
        {
            var animator = data.prefab.GetComponent<SlimeAnimator>();
            if (animator != null && animator.FirstFrame != null) return animator.FirstFrame;
            var body = data.prefab.GetComponent<SpriteRenderer>();
            return body != null ? body.sprite : null;
        }

        static Color BodyColor(EnemyData data)
        {
            var body = data.prefab.GetComponent<SpriteRenderer>();
            return body != null ? body.color : Color.white;
        }

        static void SetUpCanvas(GameObject obj, int sortingOrder)
        {
            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            DialogueWindowPresentation.ConfigureCanvas(canvas);
            obj.AddComponent<GraphicRaycaster>();
        }

        // 밝은 나무색 버튼 모양(스프라이트, 상태별 스프라이트, 라벨)은 Resources 의 template prefab 에 둔다.
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

        static TMP_Text StatLabel(string name, Transform parent, Vector2 min, Vector2 max)
        {
            TMP_Text label = Label(name, parent, min, max, "", 28, Color.white);
            label.enableAutoSizing = true;
            label.fontSizeMin = 18;
            label.fontSizeMax = 28;
            label.enableWordWrapping = false;
            return label;
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
