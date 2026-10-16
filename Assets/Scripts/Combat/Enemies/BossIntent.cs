using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Combat.Enemies
{
    // 1 texel = 화면 4px. 문장을 계속 띄우지 않고 다음 행동을 같은 크기의 문양으로 표시한다.
    public sealed class BossIntent : MonoBehaviour
    {
        public enum ActionIcon { Snowball, Charge, Exhaust, Heat, Absorb, Roots, Shoot, Drop, Explode, Front }
        public ActionIcon Icon { get; private set; }
        public int Level { get; private set; }
        public bool IsImminent { get; private set; }
        public bool IsCopied { get; private set; }
        public Color Tint { get; set; }
        public int DamagePreview { get; set; } = -1;
        public Sprite Glyph => glyph != null ? glyph.sprite : null;
        public float HorizontalOffset { get; set; }
        public Cards.MagicType? Element { get; set; }
        public string Detail { get; set; }
        public string Description
        {
            get
            {
                string text = Localization.Translate(message);
                if (Icon == ActionIcon.Snowball || Icon == ActionIcon.Heat) text += " " + Level + "/3";
                if (DamagePreview >= 0) text += "  " + Localization.Translate("피해") + " " + DamagePreview;
                if (Element.HasValue) text = Combat.UI.SlimeEncyclopediaView.ElementName(Element.Value) + " " + text;
                if (!string.IsNullOrEmpty(Detail)) text += "  " + Detail;
                return text;
            }
        }

        static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        static BossIntentArt art;
        Enemy owner;
        string message;
        SpriteRenderer glyph, border, copiedBorder, elementGlyph;
        readonly SpriteRenderer[] pips = new SpriteRenderer[3];

        public static void Show(ref BossIntent view, Transform owner, ActionIcon icon, string description,
            Color tint, int level = 0, bool imminent = false, bool copied = false)
        {
            if (view == null)
            {
                var go = new GameObject("BossIntent");
                go.transform.SetParent(owner, false);
                view = go.AddComponent<BossIntent>();
                view.owner = owner.GetComponent<Enemy>();
                view.Build();
            }
            view.Icon = icon;
            view.message = description;
            view.Level = Mathf.Clamp(level, 0, 3);
            view.Tint = tint;
            view.IsImminent = imminent;
            view.IsCopied = copied;
            view.DamagePreview = -1;
            view.glyph.sprite = IconSprite(icon, view.Level);
            view.copiedBorder.gameObject.SetActive(copied);
            for (int i = 0; i < 3; i++)
            {
                view.pips[i].gameObject.SetActive(icon == ActionIcon.Snowball || icon == ActionIcon.Heat);
                view.pips[i].sprite = icon == ActionIcon.Heat ? Pattern("ember", new[] {".x..", ".xx.", "xxxx", ".xx."}) : Pattern("pip", new[] {".xx.", "xxxx", "xxxx", ".xx."});
            }
            view.gameObject.SetActive(true);
            view.Refresh();
        }
        public static void Hide(BossIntent view) { if (view != null) view.gameObject.SetActive(false); }

        void Build()
        {
            Sprite frame = Frame();
            var back = Part("Backdrop", Pattern("back", Fill(22, 26)), 54);
            back.color = new Color32(41, 37, 47, 245);
            copiedBorder = Part("CopiedFrame", frame, 54);
            copiedBorder.transform.localPosition = new Vector3(-2f / 27, 2f / 27, 0);
            copiedBorder.color = new Color32(189, 133, 236, 255);
            border = Part("Border", frame, 55);
            glyph = Part("Glyph", null, 56);
            glyph.transform.localPosition = new Vector3(0, 2f / 27, 0);
            elementGlyph = Part("Element", null, 57);
            elementGlyph.transform.localPosition = new Vector3(0, -7f / 27, 0);
            elementGlyph.transform.localScale = Vector3.one * .45f;
            for (int i = 0; i < 3; i++)
            {
                pips[i] = Part("Stage" + (i + 1), null, 56);
                pips[i].transform.localPosition = new Vector3((i - 1) * 6f / 27, -9f / 27, 0);
            }
            // 글자는 기존 적 정보창에서만 표시한다. 문양에도 마우스를 올릴 수 있다.
            var hit = gameObject.AddComponent<BoxCollider2D>();
            hit.size = new Vector2(22f / 27, 26f / 27);
            hit.isTrigger = true;
        }
        SpriteRenderer Part(string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }
        void LateUpdate()
        {
            if (owner == null || !owner.IsAlive) { Hide(this); return; }
            Refresh();
        }
        void Refresh()
        {
            Transform parent = transform.parent;
            var animator = parent.GetComponent<SlimeAnimator>();
            float top = animator != null && animator.FirstFrame != null
                ? parent.TransformPoint(new Vector3(0, animator.FirstFrame.bounds.max.y, 0)).y : parent.position.y + 1.8f;
            float y = top + 1.32f;
            Camera camera = Camera.main;
            if (camera != null) y = Mathf.Min(y, camera.transform.position.y + camera.orthographicSize - .58f);
            transform.position = new Vector3(parent.position.x + HorizontalOffset, y, parent.position.z);
            transform.rotation = Quaternion.identity;
            Vector3 scale = parent.lossyScale;
            transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
            float pulse = IsImminent ? .68f + .32f * Mathf.Abs(Mathf.Sin(Time.time * 6)) : 1;
            border.color = new Color(Tint.r, Tint.g, Tint.b, pulse);
            bool card = Icon == ActionIcon.Shoot || Icon == ActionIcon.Drop || Icon == ActionIcon.Explode;
            Color symbol = card ? Color.white : Tint;
            symbol.a = pulse;
            glyph.color = symbol;
            elementGlyph.enabled = Element.HasValue;
            if (Element.HasValue)
            {
                elementGlyph.sprite = CardGlyph(EnemyMagicVisual.Catalog.Element(Element.Value));
                elementGlyph.color = new Color(1, 1, 1, pulse);
            }
            glyph.transform.localRotation = Quaternion.Euler(0, 0, Icon == ActionIcon.Exhaust ? Mathf.Floor(Time.time * 5) * 45 : 0);
            for (int i = 0; i < 3; i++) pips[i].color = i < Level ? Tint : new Color32(83, 78, 89, 255);
        }

        static Sprite IconSprite(ActionIcon icon, int level)
        {
            string key = icon + "/" + (icon == ActionIcon.Snowball ? level : 0);
            if (Sprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;
            if (icon == ActionIcon.Shoot || icon == ActionIcon.Drop || icon == ActionIcon.Explode)
            {
                if (art == null) art = Resources.Load<BossIntentArt>("Combat/BossIntentArt");
                Sprite card = icon == ActionIcon.Shoot ? art.shoot : icon == ActionIcon.Drop ? art.drop : art.explode;
                Sprite crop = CardGlyph(card);
                Sprites[key] = crop;
                return crop;
            }
            if (icon == ActionIcon.Snowball)
            {
                string[] rows = new string[18];
                float radius = 3 + level * 1.5f;
                for (int y = 0; y < 18; y++)
                {
                    char[] row = new string('.', 18).ToCharArray();
                    for (int x = 0; x < 18; x++) if (new Vector2(x - 8.5f, y - 8.5f).sqrMagnitude <= radius * radius) row[x] = 'x';
                    rows[y] = new string(row);
                }
                return Pattern(key, rows);
            }
            string[] pattern;
            switch (icon)
            {
                case ActionIcon.Heat: pattern = new[] {"........x.......", ".......xx.......", ".......xxx......", "......xxxx......", "..x...xxxxx.....", "..xx.xxxxxxx....", ".xxxxxxxxxxx....", ".xxxxxxxxxxxx...", "xxxxxxxxxxxxxx..", "xxxxxxxxxxxxxx..", "xxxxxxxxxxxxxx..", ".xxxxxxxxxxxx...", "..xxxxxxxxxx....", "...xxxxxxxx.....", "....xxxxxx......", "................"}; break;
                case ActionIcon.Exhaust: pattern = new[] {"..x.............", ".xxx............", "xxxxx...........", ".xxx.......x....", "..x.......xxx...", ".........xxxxx..", "..........xxx...", "...........x....", "................", "......x.........", ".....xxx........", "....xxxxx.......", ".....xxx........", "......x........."}; break;
                case ActionIcon.Roots: pattern = new[] {"......xx........", "......xx........", "......xx........", ".....xxxx.......", "....xx..xx......", "...xx....xx.....", "..xx.xx...xx....", ".xx..xx....xx...", "xx...xx.....xx..", ".....xx.........", "....xx.xx.......", "...xx...xx......", "..xx.....xx.....", ".xx.......xx....", "................", "................"}; break;
                case ActionIcon.Absorb: pattern = new[] {".xx........xx...", "..xx......xx....", "...xx....xx.....", "....xx..xx......", ".....xxxx.......", "..xxxxxxxxxx....", "....xxxxxx......", ".....xxxx.......", "......xx........", ".....xxxx.......", "....xx..xx......", "...xx....xx.....", "..xx......xx....", ".xx........xx...", "xx..........xx..", "................"}; break;
                default: pattern = new[] {"................", "......xx........", ".....xxx........", "....xxxx........", "...xxxx.........", "..xxxxxxxxxxxx..", ".xxxxxxxxxxxxx..", "xxxxxxxxxxxxxx..", ".xxxxxxxxxxxxx..", "..xxxxxxxxxxxx..", "...xxxx.........", "....xxxx........", ".....xxx........", "......xx........", "................", "................"}; break;
            }
            return Pattern(key, pattern);
        }
        public static Sprite CardGlyph(Sprite card)
        {
            if (card == null) return null;
            string key = "card/" + card.GetInstanceID();
            if (Sprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;
            Rect r = card.rect;
            float side = Mathf.Round(r.width * 20f / 34);
            var rect = new Rect(r.x + Mathf.Round((r.width - side) / 2), r.y + Mathf.Round((r.height - side) / 2), side, side);
            Sprite crop = Sprite.Create(card.texture, rect, Vector2.one * .5f, 27);
            crop.name = "BossIntent." + card.name;
            return Sprites[key] = crop;
        }
        static Sprite Frame()
        {
            string[] rows = new string[26];
            for (int y = 0; y < 26; y++) rows[y] = y == 0 || y == 25 ? new string('x', 22) : "x" + new string('.', 20) + "x";
            return Pattern("frame", rows);
        }
        static string[] Fill(int width, int height)
        {
            string[] rows = new string[height];
            for (int i = 0; i < height; i++) rows[i] = new string('x', width);
            return rows;
        }
        static Sprite Pattern(string key, string[] rows)
        {
            if (Sprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;
            var texture = new Texture2D(rows[0].Length, rows.Length, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var colors = new Color[texture.width * texture.height];
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < rows[y].Length; x++) colors[(rows.Length - 1 - y) * texture.width + x] = rows[y][x] == 'x' ? Color.white : Color.clear;
            texture.SetPixels(colors);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f, 27);
            sprite.name = "BossIntent." + key;
            Sprites[key] = sprite;
            return sprite;
        }
    }
}
