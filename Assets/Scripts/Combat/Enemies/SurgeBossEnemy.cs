using System.Collections;
using Cards;
using Combat.Allies;
using Core;
using TMPro;
using UnityEngine;

namespace Combat.Enemies
{
    // 주문 확정 경로만 기억을 바꾼다. 아군 자동 공격과 Spawn은 기억을 덮어쓰지 않는다.
    public class SurgeBossEnemy : BossEnemy
    {
        [SerializeField] Sprite markerSprite;
        [SerializeField] Sprite shellSprite;
        MortarStrike strike;
        TMP_Text memoryLabel;
        SpriteRenderer surgeGlow;
        SpriteRenderer body;
        MagicType? rememberedSpell;

        MortarStrike Strike => strike ??= new MortarStrike(markerSprite, shellSprite);
        // 문장을 바꾸면 English.json도 함께 고친다.
        public override string AttackDescription => "마지막으로 사용한 Shoot, Drop, Explode를 다음 턴에 따라 한다. Spawn은 기억하지 않고, 공격 주문을 쓰지 않은 턴에는 앞줄을 공격한다.";
        public MagicType? RememberedSpell => rememberedSpell;
        public bool HasMarker => strike != null && strike.IsAimed;
        public bool IsEnraged => Hp * 2 <= MaxHp;

        public static void ObservePlayerSpell(MagicType spell)
        {
            if (spell != MagicType.Shoot && spell != MagicType.Drop && spell != MagicType.Explode) return;
            foreach (SurgeBossEnemy boss in FindObjectsOfType<SurgeBossEnemy>())
                if (boss.IsAlive) boss.Remember(spell);
        }

        public void Remember(MagicType spell)
        {
            if (spell != MagicType.Shoot && spell != MagicType.Drop && spell != MagicType.Explode) return;
            rememberedSpell = spell;
            Strike.Clear();
            if (spell == MagicType.Drop) Strike.Aim();
            RefreshMemory();
        }

        protected override void Start()
        {
            base.Start();
            var label = new GameObject("RememberedSpell");
            label.transform.SetParent(transform, false);
            memoryLabel = label.AddComponent<TextMeshPro>();
            if (HpText != null) memoryLabel.font = HpText.font;
            memoryLabel.fontSize = 3.2f;
            memoryLabel.alignment = TextAlignmentOptions.Center;
            memoryLabel.rectTransform.sizeDelta = new Vector2(16, 2);
            memoryLabel.GetComponent<MeshRenderer>().sortingOrder = 55;
            body = GetComponent<SpriteRenderer>();
            var glow = new GameObject("WorldTreeSurgeGlow");
            glow.transform.SetParent(transform, false);
            surgeGlow = glow.AddComponent<SpriteRenderer>();
            surgeGlow.sortingLayerID = body.sortingLayerID;
            surgeGlow.sortingOrder = body.sortingOrder + 1;
            surgeGlow.enabled = false;
            RefreshMemory();
        }

        void LateUpdate()
        {
            if (memoryLabel == null) return;
            float top = Animator != null && Animator.FirstFrame != null
                ? transform.TransformPoint(new Vector3(0, Animator.FirstFrame.bounds.max.y, 0)).y
                : transform.position.y + 1.8f;
            memoryLabel.transform.position = new Vector3(transform.position.x, top + .9f, transform.position.z);
            memoryLabel.transform.rotation = Quaternion.identity;
            Vector3 scale = transform.lossyScale;
            memoryLabel.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
            memoryLabel.color = IsEnraged ? new Color32(255, 221, 125, 255) : new Color32(151, 255, 204, 255);
            surgeGlow.enabled = IsAlive && IsEnraged;
            if (surgeGlow.enabled)
            {
                surgeGlow.sprite = body.sprite;
                surgeGlow.flipX = body.flipX;
                surgeGlow.flipY = body.flipY;
                surgeGlow.color = new Color(1f, .85f, .35f, .13f + .07f * Mathf.Sin(Time.time * 8));
            }
        }

        void RefreshMemory()
        {
            if (memoryLabel == null) return;
            string text = !rememberedSpell.HasValue ? "앞줄 공격"
                : rememberedSpell == MagicType.Shoot ? "모방 Shoot: 앞줄"
                : rememberedSpell == MagicType.Drop ? "모방 Drop: 표시한 자리" : "모방 Explode: 아군 전체";
            memoryLabel.text = Localization.Translate(text);
        }

        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            MagicType? spell = rememberedSpell;
            rememberedSpell = null;
            Animator.RangeAttack();
            if (spell == MagicType.Drop)
                Strike.Fire(transform.position, AttackDamage);
            else if (spell == MagicType.Explode)
            {
                Strike.Clear();
                StartCoroutine(ExplodeEcho());
            }
            else
            {
                Strike.Clear();
                StartCoroutine(ShootEcho());
            }
            RefreshMemory();
        }

        IEnumerator ShootEcho()
        {
            var echo = CreateEcho(transform.position, shellSprite, new Color32(171, 255, 199, 255));
            echo.transform.localScale = Vector3.one * 0.25f;
            Vector3 from = transform.position;
            float playerX = Player.PlayerInt().transform.position.x;
            Vector3 to = new Vector3(AllyFormation.FrontLineX(playerX), from.y, 0);
            float elapsed = 0;
            while (elapsed < 0.4f && IsAlive)
            {
                elapsed += Time.deltaTime;
                echo.transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / 0.4f));
                yield return null;
            }
            Destroy(echo);
            if (IsAlive) AllyFormation.HitFrontLine(AttackDamage, transform.position.x);
        }

        IEnumerator ExplodeEcho()
        {
            var echoes = new System.Collections.Generic.List<GameObject>();
            float playerX = Player.PlayerInt().transform.position.x;
            var formation = AllyFormation.Current;
            if (formation != null && formation.Allies.Count > 0)
                foreach (var ally in formation.Allies) echoes.Add(CreateEcho(ally.transform.position, markerSprite, new Color32(255, 225, 138, 255)));
            else echoes.Add(CreateEcho(new Vector3(playerX, -3f, 0), markerSprite, new Color32(255, 225, 138, 255)));
            float elapsed = 0;
            while (elapsed < 0.35f && IsAlive)
            {
                elapsed += Time.deltaTime;
                foreach (GameObject echo in echoes)
                    echo.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 0.45f, elapsed / 0.35f);
                yield return null;
            }
            foreach (GameObject echo in echoes) Destroy(echo);
            // 전체 공격은 단일 공격의 절반. 공격 약화와 분노를 적용한 뒤 나눈다.
            if (IsAlive) AllyFormation.HitAllAllies(Mathf.Max(1, AttackDamage / 2));
        }

        GameObject CreateEcho(Vector3 position, Sprite sprite, Color tint)
        {
            var echo = new GameObject("WorldTreeEcho");
            echo.transform.SetParent(transform, true);
            echo.transform.position = position;
            SpriteRenderer renderer = echo.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingOrder = 30;
            return echo;
        }

        void OnDisable()
        {
            strike?.Clear();
            rememberedSpell = null;
        }

        protected override void Death()
        {
            strike?.Clear();
            if (surgeGlow != null) surgeGlow.enabled = false;
            base.Death();
        }
    }
}
