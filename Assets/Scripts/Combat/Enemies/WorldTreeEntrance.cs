using System.Collections;
using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Combat.Enemies
{
    // 먼 세계수의 작은 반짝임에서 거대한 나무형 슬라임이 날아온다. 실제 적과 카메라는 움직이지 않고
    // 같은 스프라이트의 연출용 사본을 쓴다. 중단돼도 전투 객체의 위치와 애니메이션은 그대로다.
    public sealed class WorldTreeEntrance : MonoBehaviour
    {
        public const float Duration = 3.5f;
        internal const float FlashAt = 0.85f;
        internal const float FlightAt = 1.05f;
        internal const float LandAt = 2.15f;
        const float TitleAt = 2.65f;
        static WorldTreeEntrance current;
        public static bool IsPlaying => current != null && current.running;
        public static float Progress => IsPlaying ? Mathf.Clamp01(current.elapsed / Duration) : 0f;

        Enemy boss;
        SpriteRenderer body;
        readonly Dictionary<Renderer, bool> visibility = new Dictionary<Renderer, bool>();
        readonly List<Object> ownedAssets = new List<Object>();
        readonly List<Image> leaves = new List<Image>();
        Canvas canvas;
        RectTransform frame;
        Image dim, topBar, bottomBar, proxy, star, crossH, crossV, rune, impact;
        TMP_Text title, subtitle;
        CanvasGroup caption;
        Sprite pixel, leaf;
        Vector2 origin, destination, bodySize;
        float floorOffset;
        Vector3 bodyScale;
        float elapsed;
        int startFrame;
        bool running, skip, restored;
        WorldTreeCinematicAudio sounds;
        static readonly Color Gold = new Color32(255, 239, 166, 255);
        static readonly Color Green = new Color32(166, 227, 140, 255);

        public static IEnumerator Play(Enemy enemy) => PlayFrom(enemy, new Vector2(.5f, .73f));

        // 1페이즈의 액체가 모인 위치를 화면 정규 좌표로 받는다. 배경 세계수와 분리된 슬라임의 마력이다.
        public static IEnumerator PlayFrom(Enemy enemy, Vector2 viewportOrigin)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy || IsPlaying) yield break;
            var host = new GameObject("WorldTreeEntrance");
            var entrance = host.AddComponent<WorldTreeEntrance>();
            current = entrance;
            try
            {
                if (!entrance.Initialize(enemy, viewportOrigin)) yield break;
                while (entrance != null && entrance.running)
                    yield return null;
            }
            finally
            {
                if (entrance != null)
                {
                    entrance.Restore();
                    Destroy(host);
                }
            }
        }

        public static void Skip()
        {
            if (IsPlaying && !InteractionLock.IsPaused) current.skip = true;
        }

        bool Initialize(Enemy enemy, Vector2 viewportOrigin)
        {
            boss = enemy;
            body = enemy.GetComponent<SpriteRenderer>();
            var camera = Camera.main;
            if (body == null || body.sprite == null || camera == null) return false;
            // Spawn 직후에는 SlimeAnimator.Start 전이라 prefab의 임시 그림일 수 있다.
            var animator = enemy.GetComponent<SlimeAnimator>();
            Sprite displayedSprite = animator != null && animator.FirstFrame != null ? animator.FirstFrame : body.sprite;

            var root = new GameObject("CinematicCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900; // 설정 메뉴(1000)는 연출보다 앞에 둔다.
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1;
            Canvas.ForceUpdateCanvases();
            frame = root.GetComponent<RectTransform>();
            float width = 1080f * Screen.width / Mathf.Max(1, Screen.height);
            Vector3 viewport = camera.WorldToViewportPoint(body.transform.position);
            destination = new Vector2((viewport.x - .5f) * width, (viewport.y - .5f) * 1080f);
            origin = new Vector2((viewportOrigin.x - .5f) * width, (viewportOrigin.y - .5f) * 1080f);
            float pixelsPerUnit = 1080f / (camera.orthographicSize * 2);
            Vector3 size = displayedSprite.bounds.size;
            bodyScale = body.transform.lossyScale;
            bodySize = new Vector2(size.x * Mathf.Abs(bodyScale.x), size.y * Mathf.Abs(bodyScale.y)) * pixelsPerUnit;
            floorOffset = bodySize.y * displayedSprite.pivot.y / displayedSprite.rect.height;

            pixel = MakeSprite(new[] { "x" });
            leaf = MakeSprite(new[] { "..xx", ".xxx", "xxx.", "xx.." });
            dim = Box("Dusk", Vector2.zero, new Vector2(width, 1080), new Color(.025f, .07f, .07f, 0));
            dim.rectTransform.anchorMin = Vector2.zero;
            dim.rectTransform.anchorMax = Vector2.one;
            dim.rectTransform.sizeDelta = Vector2.zero;
            dim.raycastTarget = true;
            topBar = Box("TopLetterbox", new Vector2(0, 504), new Vector2(width, 72), Color.black);
            bottomBar = Box("BottomLetterbox", new Vector2(0, -504), new Vector2(width, 72), Color.black);
            foreach (Image bar in new[] { topBar, bottomBar })
            {
                bar.rectTransform.anchorMin = new Vector2(0, .5f);
                bar.rectTransform.anchorMax = new Vector2(1, .5f);
                bar.rectTransform.sizeDelta = new Vector2(0, 72);
            }

            for (int i = 0; i < 28; i++)
            {
                Image particle = Box("LeafTrail" + i, origin, new Vector2(16, 16), Color.clear);
                particle.sprite = leaf;
                leaves.Add(particle);
            }
            proxy = Box("WorldTreeFlight", origin, bodySize, Color.clear);
            proxy.sprite = displayedSprite;
            proxy.rectTransform.pivot = displayedSprite.pivot / displayedSprite.rect.size;
            proxy.rectTransform.localScale = new Vector3(bodyScale.x < 0 ? -1 : 1, bodyScale.y < 0 ? -1 : 1, 1);
            star = Box("DistantPinprick", origin, new Vector2(28, 28), Gold);
            star.sprite = MakeSprite(new[] { "...x...", "...x...", "...x...", "xxxxxxx", "...x...", "...x...", "...x..." });
            crossH = Box("DistantFlashHorizontal", origin, new Vector2(4, 4), Color.clear);
            crossV = Box("DistantFlashVertical", origin, new Vector2(4, 4), Color.clear);
            impact = Box("LandingLine", destination, new Vector2(bodySize.x, 8), Color.clear);
            rune = Box("MemoryRune", destination, new Vector2(52, 52), Color.clear);
            rune.sprite = MakeSprite(new[] { "...x...", "..x.x..", ".x.x.x.", "x.xxx.x", ".x.x.x.", "..x.x..", "...x..." });

            var captionObject = new GameObject("BossCaption", typeof(RectTransform), typeof(CanvasGroup));
            captionObject.transform.SetParent(frame, false);
            caption = captionObject.GetComponent<CanvasGroup>();
            caption.alpha = 0;
            title = Label(captionObject.transform, "BossName", "세계수 슬라임", 58, new Vector2(0, -310), new Vector2(width - 100, 80));
            title.color = Gold;
            subtitle = Label(captionObject.transform, "BossSubtitle", "슬라임을 낳는 사악한 마력의 근원", 24, new Vector2(0, -366), new Vector2(width - 100, 50));
            subtitle.color = Green;
            Image line = Box("NameUnderline", new Vector2(0, -410), new Vector2(300, 4), Gold);
            line.transform.SetParent(captionObject.transform, false);

            Image skipBackground = Box("SkipButton", new Vector2(width / 2 - 148, -504), new Vector2(264, 56), new Color(.1f, .16f, .14f, 1));
            skipBackground.raycastTarget = true;
            skipBackground.rectTransform.anchorMin = skipBackground.rectTransform.anchorMax = new Vector2(1, 0);
            skipBackground.rectTransform.anchoredPosition = new Vector2(-148, 36);
            Button button = skipBackground.gameObject.AddComponent<Button>();
            button.onClick.AddListener(Skip);
            Label(skipBackground.transform, "SkipLabel", "건너뛰기", 23, Vector2.zero, new Vector2(250, 54));
            running = true;
            startFrame = Time.frameCount;
            CaptureAndHide();
            sounds = new WorldTreeCinematicAudio(transform);
            Render(0);
            return true;
        }

        void Update()
        {
            if (!running) return;
            if (boss == null || !boss.gameObject.activeInHierarchy) { Finish(); return; }
            bool paused = InteractionLock.IsPaused || Time.timeScale <= 0;
            sounds?.SetPaused(paused);
            if (paused) return;
            // Esc는 설정 메뉴가 먼저 받는다. 메뉴가 없는 씬에서는 Esc도 건너뛰기다.
            if (skip || Time.frameCount > startFrame && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)))
            { Finish(); return; }
            elapsed += Time.deltaTime;
            sounds?.Entrance(elapsed, Mathf.Clamp(destination.x / 960f, -.65f, .65f));
            Render(elapsed);
            if (elapsed >= Duration) Finish();
        }

        void LateUpdate()
        {
            if (running) CaptureAndHide();
        }

        void CaptureAndHide()
        {
            // HP·분노·조준 표식도 감춘다. 연출 중 추가된 자식은 원래 상태를 따로 기억한다.
            foreach (Renderer renderer in boss.GetComponentsInChildren<Renderer>(true))
            {
                if (!visibility.ContainsKey(renderer)) visibility.Add(renderer, renderer.enabled);
                renderer.enabled = false;
            }
        }

        // 깊이감은 마지막 1/3에서 급격히 커지는 원근과 가속으로 만든다.
        public static Vector2 EvaluateFlight(Vector2 start, Vector2 end, float progress)
        {
            float t = Mathf.Clamp01(progress);
            float travel = t * t * t;
            Vector2 point = Vector2.Lerp(start, end, travel);
            point.x -= Mathf.Sin(t * Mathf.PI) * 90f;
            point.y += Mathf.Sin(t * Mathf.PI) * 64f;
            return point;
        }

        void Render(float time)
        {
            float enter = Mathf.Clamp01(time / .3f);
            float exit = Mathf.Clamp01((Duration - time) / .25f);
            float framing = Mathf.Min(enter, exit);
            dim.color = new Color(.025f, .07f, .07f, .55f * framing);
            topBar.color = bottomBar.color = new Color(0, 0, 0, .94f * framing);
            float flight = Mathf.Clamp01((time - FlightAt) / (LandAt - FlightAt));
            float flash = Mathf.Clamp01(1 - Mathf.Abs(time - .96f) / .16f);
            float twinkle = time < FlashAt ? .55f + .45f * Mathf.Sin(time * 18f) : 0;
            star.color = WithAlpha(Gold, Mathf.Max(twinkle, flash));
            float starSize = time < FlashAt ? 12 + 16 * Mathf.Pow(time / FlashAt, 4) : 28 + 56 * flash;
            star.rectTransform.sizeDelta = Vector2.one * Snap(starSize);
            crossH.color = crossV.color = WithAlpha(Gold, flash);
            crossH.rectTransform.sizeDelta = new Vector2(Snap(220 * flash), 4);
            crossV.rectTransform.sizeDelta = new Vector2(4, Snap(100 * flash));

            Vector2 position = EvaluateFlight(origin, destination, flight);
            float scale = Mathf.Lerp(.016f, 1, Mathf.Pow(flight, 4.5f));
            proxy.color = time < FlightAt ? Color.clear : Color.Lerp(new Color(.16f, .29f, .2f), Color.white, Mathf.Pow(flight, 3));
            Vector2 dimensions = bodySize * scale;
            if (time >= LandAt)
            {
                float land = time - LandAt;
                float squash = Mathf.Sin(Mathf.Clamp01(land / .16f) * Mathf.PI) * .28f;
                float rebound = land < .16f ? 0 : Mathf.Sin(Mathf.Clamp01((land - .16f) / .32f) * Mathf.PI);
                dimensions = bodySize * new Vector2(1 + squash - rebound * .09f, 1 - squash + rebound * .12f);
                position.y += rebound * 28;
                // 전장이나 카메라 위치를 바꾸지 않고 사본만 착지 충격에 떨게 한다.
                position.x += Mathf.Sin(land * 90) * 10 * Mathf.Clamp01(1 - land / .28f);
            }
            proxy.rectTransform.sizeDelta = new Vector2(Mathf.Max(4, Snap(dimensions.x)), Mathf.Max(4, Snap(dimensions.y)));
            proxy.rectTransform.anchoredPosition = Snap(position);
            proxy.rectTransform.localRotation = Quaternion.Euler(0, 0, time < LandAt ? Mathf.Sin(flight * Mathf.PI) * 12 : 0);

            for (int i = 0; i < leaves.Count; i++)
            {
                Image particle = leaves[i];
                float lag = (i % 14) * .024f;
                if (time < LandAt)
                {
                    float trailing = Mathf.Clamp01(flight - lag);
                    Vector2 trail = EvaluateFlight(origin, destination, trailing);
                    trail += new Vector2(Mathf.Sin(i * 2.7f + time * 3) * 14, Mathf.Cos(i * 1.7f) * 16) * trailing;
                    particle.rectTransform.anchoredPosition = Snap(trail);
                    particle.rectTransform.sizeDelta = Vector2.one * (4 + Snap(12 * trailing));
                    particle.color = WithAlpha(i % 3 == 0 ? Gold : Green, time > FlightAt + lag ? .65f * flight : 0);
                }
                else
                {
                    float age = time - LandAt;
                    float side = i % 2 == 0 ? -1 : 1;
                    float speed = 100 + (i % 7) * 30;
                    Vector2 ground = destination - Vector2.up * floorOffset;
                    particle.rectTransform.anchoredPosition = Snap(ground + new Vector2(side * age * speed, Mathf.Sin(i) * 14 + age * 110 - age * age * 190));
                    particle.rectTransform.sizeDelta = new Vector2(12 + i % 3 * 4, 8);
                    particle.color = WithAlpha(i % 3 == 0 ? Gold : Green, .8f * Mathf.Clamp01(1 - age / .75f));
                }
                particle.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Floor((time * 100 + i * 40) / 45) * 45);
            }
            float impactAge = time - LandAt;
            impact.rectTransform.anchoredPosition = Snap(destination - Vector2.up * floorOffset);
            impact.rectTransform.sizeDelta = new Vector2(bodySize.x * (1 + Mathf.Max(0, impactAge) * 2), 4);
            impact.color = WithAlpha(Gold, impactAge < 0 ? 0 : Mathf.Clamp01(1 - impactAge / .25f));
            float reveal = Mathf.Clamp01((time - TitleAt) / .18f);
            caption.alpha = reveal * exit;
            rune.rectTransform.anchoredPosition = Snap(destination + Vector2.up * bodySize.y * .14f);
            rune.color = WithAlpha(Gold, reveal * (.75f + .25f * Mathf.Sin(time * 12)));
        }

        Image Box(string name, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(frame, false);
            Image image = go.GetComponent<Image>();
            image.sprite = pixel;
            image.color = color;
            image.raycastTarget = false;
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(.5f, .5f);
            image.rectTransform.anchoredPosition = position;
            image.rectTransform.sizeDelta = size;
            return image;
        }

        TMP_Text Label(Transform parent, string name, string korean, float size, Vector2 position, Vector2 bounds)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TMP_Text label = go.GetComponent<TextMeshProUGUI>();
            TMP_Text existing = boss.GetComponentInChildren<TMP_Text>(true);
            label.font = existing != null ? existing.font : TMP_Settings.defaultFontAsset;
            label.text = Localization.Translate(korean);
            label.fontSize = size;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.rectTransform.anchoredPosition = position;
            label.rectTransform.sizeDelta = bounds;
            return label;
        }

        Sprite MakeSprite(string[] rows)
        {
            var texture = new Texture2D(rows[0].Length, rows.Length, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            var colors = new Color[texture.width * texture.height];
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < rows[y].Length; x++)
                    colors[(rows.Length - 1 - y) * texture.width + x] = rows[y][x] == 'x' ? Color.white : Color.clear;
            texture.SetPixels(colors);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 27);
            ownedAssets.Add(sprite);
            ownedAssets.Add(texture);
            return sprite;
        }

        static float Snap(float value) => Mathf.Round(value / 4) * 4;
        static Vector2 Snap(Vector2 point) => new Vector2(Snap(point.x), Snap(point.y));
        static Color WithAlpha(Color color, float alpha) { color.a = alpha; return color; }

        void Finish()
        {
            Restore();
            if (canvas != null) canvas.gameObject.SetActive(false);
            Destroy(gameObject);
        }

        void Restore()
        {
            if (restored) return;
            restored = true;
            running = false;
            sounds?.Stop();
            foreach (var pair in visibility)
                if (pair.Key != null) pair.Key.enabled = pair.Value;
            if (current == this) current = null;
            if (canvas != null) canvas.gameObject.SetActive(false);
            foreach (Object asset in ownedAssets)
                if (asset != null) Destroy(asset);
            ownedAssets.Clear();
        }

        void OnDisable() => Restore();
        void OnDestroy() => Restore();
    }
}
