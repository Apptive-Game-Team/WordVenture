using System.Collections;
using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Combat.Enemies
{
    // 죽은 나무 모양 슬라임에서 금빛 액체가 흘러나와 본래 슬라임으로 뭉친다.
    // 수명과 입력 잠금은 죽은 적이 아닌 웨이브 컨트롤러에 묶는다.
    public sealed class WorldTreeTransition : MonoBehaviour
    {
        public const float FluidDuration = 2.8f;
        internal const float GatherAt = 1.7f;
        internal const float GatherStartAt = 1.05f;
        internal const float CrackAt = .3f;
        static WorldTreeTransition current;
        public static bool IsPlaying => current != null && current.running || WorldTreeEntrance.IsPlaying;
        public static float FluidProgress => current != null ? Mathf.Clamp01(current.elapsed / FluidDuration) : 0;
        public static Enemy SpawnedCore => current != null ? current.core : null;
        bool running, restored, skip, previousLock, coreSpawned;
        float spawnX;
        Sprite standing, fallen;
        Image coreProxy;
        Vector2 coreSize;
        readonly Dictionary<Renderer, bool> coreVisibility = new Dictionary<Renderer, bool>();
        float elapsed;
        WorldTreeCinematicAudio sounds;
        int startFrame;
        Enemy core;
        Canvas canvas;
        RectTransform frame;
        Image corpse, puddle, waveA, waveB, puddleShine, glint;
        readonly List<Image> drops = new List<Image>();
        readonly List<Object> assets = new List<Object>();
        Sprite pixel;
        Vector2 corpsePoint, poolPoint;
        Vector2 corpseSize;
        static readonly Color Gold = new Color32(255, 203, 68, 255);
        static readonly Color BrightGold = new Color32(255, 247, 178, 255);

        public static IEnumerator Play(TreeShellBossEnemy shell)
        {
            if (shell == null || IsPlaying) yield break;
            var host = new GameObject("WorldTreeTransition");
            BattleWaveController controller = FindObjectOfType<BattleWaveController>();
            if (controller != null) host.transform.SetParent(controller.transform, false);
            var transition = host.AddComponent<WorldTreeTransition>();
            current = transition;
            transition.previousLock = InteractionLock.IsLocked;
            InteractionLock.IsLocked = true;
            transition.running = true;
            try
            {
                transition.Initialize(shell);
                while (transition != null && transition.running && !transition.skip && transition.elapsed < FluidDuration)
                    yield return null;
                if (transition == null || transition.restored) yield break;
                transition.SpawnCore();
                if (transition.canvas != null) transition.canvas.gameObject.SetActive(false);

            }
            finally
            {
                if (transition != null) transition.Restore();
                if (host != null) Destroy(host);
            }
        }
        public static void Skip()
        {
            if (InteractionLock.IsPaused) return;
            if (current != null) current.skip = true;
            if (WorldTreeEntrance.IsPlaying) WorldTreeEntrance.Skip();
        }
        void SpawnCore()
        {
            if (coreSpawned) return;
            coreSpawned = true;
            GameObject spawned = BattleWaveController.SpawnExtraEnemy(29, spawnX);
            if (spawned != null) core = spawned.GetComponent<Enemy>();
        }
        void Initialize(TreeShellBossEnemy shell)
        {
            startFrame = Time.frameCount;
            Camera camera = Camera.main;
            SpriteRenderer body = shell.GetComponent<SpriteRenderer>();
            if (camera == null || body == null || body.sprite == null) { skip = true; return; }
            float width = 1080f * Screen.width / Mathf.Max(1, Screen.height);
            Vector3 viewport = camera.WorldToViewportPoint(shell.transform.position);
            Vector3 ground = camera.WorldToViewportPoint(new Vector3(shell.transform.position.x, -3f, 0));
            spawnX = shell.transform.position.x;
            standing = shell.GetComponent<SlimeAnimator>().FirstFrame;
            fallen = shell.GetComponent<SlimeAnimator>().DeathFrame;
            corpsePoint = new Vector2((viewport.x - .5f) * width, (viewport.y - .5f) * 1080f);
            poolPoint = new Vector2((ground.x - .5f) * width, (ground.y - .5f) * 1080f);
            float pixelsPerUnit = 1080f / (camera.orthographicSize * 2);
            corpseSize = Vector2.Scale(body.sprite.bounds.size, new Vector2(Mathf.Abs(body.transform.lossyScale.x), Mathf.Abs(body.transform.lossyScale.y))) * pixelsPerUnit;
            var root = new GameObject("GoldLiquidCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 890;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1;
            frame = root.GetComponent<RectTransform>();
            pixel = MakeSprite(new[] { "x" });
            Image dim = Box("GoldDusk", Vector2.zero, new Vector2(width, 1080), new Color(.025f, .03f, .02f, .42f));
            dim.rectTransform.anchorMin = Vector2.zero;
            dim.rectTransform.anchorMax = Vector2.one;
            dim.rectTransform.anchoredPosition = Vector2.zero;
            dim.rectTransform.sizeDelta = Vector2.zero;
            dim.raycastTarget = true;
            corpse = Box("SlimeCorpse", corpsePoint, corpseSize, Color.white);
            // 정상 형체로 시작한 뒤 갈라진 나무의 사망 프레임으로 바꾼다.
            Sprite silhouette = standing;
            corpse.sprite = silhouette;
            corpse.rectTransform.pivot = silhouette.pivot / silhouette.rect.size;
            corpse.rectTransform.localScale = new Vector3(body.transform.lossyScale.x < 0 ? -1 : 1, 1, 1);
            // 전환용 시체만 남겨 일반 사망 프레임과 겹치지 않게 한다.
            body.enabled = false;
            puddle = Box("GoldPuddle", poolPoint, new Vector2(4, 4), Gold);
            puddle.sprite = MakeSprite(new[] { "..xxxxxxxxxxx..", ".xxxxxxxxxxxxx.", "xxxxxxxxxxxxxxx", ".xxxxxxxxxxxxx.", "..xxxxxxxxxxx.." });
            waveA = Box("GoldRippleA", poolPoint, new Vector2(32, 4), BrightGold);
            waveB = Box("GoldRippleB", poolPoint, new Vector2(24, 4), BrightGold);
            puddleShine = Box("GoldPuddleShine", poolPoint + new Vector2(-12, 4), new Vector2(4, 4), BrightGold);
            Sprite sparkle = MakeSprite(new[] { "...x...", "...x...", "...x...", "xxxxxxx", "...x...", "...x...", "...x..." });
            for (int i = 0; i < 24; i++)
            {
                Image drop = Box("GoldDrop" + i, corpsePoint, new Vector2(4, 4), Color.clear);
                if (i % 3 == 0) drop.sprite = sparkle;
                drops.Add(drop);
            }
            glint = Box("GoldCongealingGlint", poolPoint, new Vector2(4, 4), Color.clear);
            Image skipButton = Box("SkipGoldTransition", new Vector2(width / 2 - 148, -504), new Vector2(264, 56), new Color(.1f, .12f, .09f, 1));
            skipButton.rectTransform.anchorMin = skipButton.rectTransform.anchorMax = new Vector2(1, 0);
            skipButton.rectTransform.anchoredPosition = new Vector2(-148, 36);
            skipButton.raycastTarget = true;
            skipButton.gameObject.AddComponent<Button>().onClick.AddListener(Skip);
            var labelObject = new GameObject("SkipLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(skipButton.transform, false);
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            var existing = shell.GetComponentInChildren<TMP_Text>(true);
            if (existing != null) label.font = existing.font;
            label.text = Localization.Translate("건너뛰기");
            label.fontSize = 23;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.rectTransform.sizeDelta = new Vector2(250, 54);
            sounds = new WorldTreeCinematicAudio(transform);
            Render();
        }
        void Update()
        {
            if (!running || restored) return;
            bool paused = InteractionLock.IsPaused || Time.timeScale <= 0;
            sounds?.SetPaused(paused);
            if (paused) return;
            if (Time.frameCount > startFrame && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))) skip = true;
            elapsed += Time.deltaTime;
            if (!skip) sounds?.Transition(elapsed, Mathf.Clamp(poolPoint.x / 960f, -.65f, .65f));
            if (elapsed >= GatherAt && !coreSpawned)
            {
                SpawnCore();
                RevealCore();
            }
            Render();
        }
        void Render()
        {
            if (corpse == null) return;
            float t = Mathf.Clamp01(elapsed / FluidDuration);
            float flow = Mathf.Clamp01(t / .6f);
            float gather = Mathf.Clamp01((elapsed - GatherStartAt) / (GatherAt - GatherStartAt));
            // 나무가 갈라져 무너지는 그림으로 전환한다. 축을 눌러 짜지 않는다.
            if (elapsed > CrackAt && fallen != null) corpse.sprite = fallen;
            corpse.color = WithAlpha(Color.white, 1f - Mathf.Clamp01((elapsed - .65f) / .65f));
            corpse.rectTransform.anchoredPosition = Snap(corpsePoint - Vector2.up * Mathf.Min(24, elapsed * 18));
            if (coreProxy != null)
            {
                float emerge = Mathf.SmoothStep(0, 1, Mathf.Clamp01((elapsed - GatherAt) / .8f));
                coreProxy.rectTransform.sizeDelta = coreSize * Mathf.Lerp(.08f, 1, emerge);
                coreProxy.color = Color.Lerp(BrightGold, Color.white, emerge);
            }
            float spread = Mathf.Lerp(8, 210, flow) * (1 - gather) + 4 * gather;
            puddle.rectTransform.sizeDelta = new Vector2(Snap(spread), Snap(Mathf.Lerp(4, 28, flow) * (1 - gather) + 4 * gather));
            puddle.color = Color.Lerp(Gold, BrightGold, gather);
            waveA.rectTransform.anchoredPosition = Snap(poolPoint + new Vector2(Mathf.Sin(t * 18) * spread * .19f, 4));
            waveB.rectTransform.anchoredPosition = Snap(poolPoint + new Vector2(Mathf.Cos(t * 16) * spread * .22f, -4));
            waveA.rectTransform.sizeDelta = new Vector2(Mathf.Max(4, Snap(spread * .18f)), 4);
            waveB.rectTransform.sizeDelta = new Vector2(Mathf.Max(4, Snap(spread * .13f)), 4);
            waveA.color = waveB.color = WithAlpha(BrightGold, .6f * flow * (1 - gather));
            puddleShine.rectTransform.sizeDelta = new Vector2(Mathf.Max(4, Snap(spread * .42f)), 4);
            puddleShine.rectTransform.anchoredPosition = Snap(poolPoint + new Vector2(-spread * .1f, 4 * (1 - gather)));
            puddleShine.color = WithAlpha(BrightGold, .85f * (1 - gather));
            for (int i = 0; i < drops.Count; i++)
            {
                float cycle = Mathf.Repeat(t * 2.8f + i * .137f, 1);
                float side = Mathf.Sin(i * 2.4f) * 84;
                Vector2 top = poolPoint + new Vector2(side, 22);
                Vector2 bottom = poolPoint + new Vector2(side * .1f, 0);
                Vector2 point = Vector2.Lerp(top, bottom, cycle);
                point.y += Mathf.Sin(cycle * Mathf.PI) * (12 + i % 5 * 5);
                drops[i].rectTransform.anchoredPosition = Snap(point);
                drops[i].rectTransform.sizeDelta = i % 3 == 0 ? new Vector2(20, 20) : new Vector2(4, 8);
                drops[i].color = WithAlpha(i % 3 == 0 ? BrightGold : Gold, Mathf.Clamp01(t * 6) * (1 - gather) * (i % 3 == 0 ? .35f + .65f * Mathf.Abs(Mathf.Sin(elapsed * 17 + i)) : 1));
            }
            glint.rectTransform.sizeDelta = Vector2.one * Snap(Mathf.Lerp(4, 24, gather));
            glint.color = WithAlpha(BrightGold, gather * (1 - Mathf.Clamp01((elapsed - GatherAt) / .25f)));
            if (elapsed > GatherAt) puddle.color = WithAlpha(Gold, 1 - Mathf.Clamp01((elapsed - GatherAt) / .7f));
        }
        void RevealCore()
        {
            if (core == null) return;
            var body = core.GetComponent<SpriteRenderer>();
            Sprite sprite = core.GetComponent<SlimeAnimator>().FirstFrame;
            coreSize = (Vector2)sprite.bounds.size * Mathf.Abs(core.transform.lossyScale.x) * 108f;
            coreProxy = Box("CoreEmergingHere", poolPoint, coreSize, BrightGold);
            coreProxy.sprite = sprite;
            coreProxy.rectTransform.pivot = sprite.pivot / sprite.rect.size;
            coreProxy.rectTransform.localScale = new Vector3(body.transform.lossyScale.x < 0 ? -1 : 1, 1, 1);
            HideCore();
        }
        void LateUpdate()
        {
            if (running && core != null && coreProxy != null) HideCore();
        }
        void HideCore()
        {
            foreach (Renderer renderer in core.GetComponentsInChildren<Renderer>(true))
            {
                if (!coreVisibility.ContainsKey(renderer)) coreVisibility[renderer] = renderer.enabled;
                renderer.enabled = false;
            }
        }
        Image Box(string name, Vector2 position, Vector2 size, Color tint)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(frame, false);
            var image = go.GetComponent<Image>();
            image.sprite = pixel;
            image.color = tint;
            image.raycastTarget = false;
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.one * .5f;
            image.rectTransform.anchoredPosition = Snap(position);
            image.rectTransform.sizeDelta = size;
            return image;
        }
        Sprite MakeSprite(string[] rows)
        {
            var texture = new Texture2D(rows[0].Length, rows.Length, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var colors = new Color[texture.width * texture.height];
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < rows[y].Length; x++)
                    colors[(rows.Length - 1 - y) * texture.width + x] = rows[y][x] == 'x' ? Color.white : Color.clear;
            texture.SetPixels(colors);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f, 27);
            assets.Add(sprite);
            assets.Add(texture);
            return sprite;
        }
        static float Snap(float value) => Mathf.Round(value / 4) * 4;
        static Vector2 Snap(Vector2 value) => new Vector2(Snap(value.x), Snap(value.y));
        static Color WithAlpha(Color color, float alpha) { color.a = alpha; return color; }
        void Restore()
        {
            if (restored) return;
            restored = true;
            running = false;
            sounds?.Stop();
            foreach (var pair in coreVisibility)
                if (pair.Key != null) pair.Key.enabled = pair.Value;
            coreVisibility.Clear();
            InteractionLock.IsLocked = previousLock;
            if (current == this) current = null;
            if (canvas != null) canvas.gameObject.SetActive(false);
            foreach (Object asset in assets) if (asset != null) Destroy(asset);
            assets.Clear();
        }
        void OnDisable() => Restore();
        void OnDestroy() => Restore();
    }
}
