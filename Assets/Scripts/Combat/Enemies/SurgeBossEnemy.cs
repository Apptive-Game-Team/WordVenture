using System.Collections;
using System.Collections.Generic;
using Cards;
using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    public class SurgeBossEnemy : BossEnemy
    {
        [SerializeField] Sprite markerSprite;
        [SerializeField] Sprite shellSprite;
        public struct StoredMagic
        {
            public MagicType Spell, Element;
            public StoredMagic(MagicType spell, MagicType element) { Spell = spell; Element = element; }
        }
        readonly List<StoredMagic> stored = new List<StoredMagic>();
        readonly BossIntent[] memories = new BossIntent[3];
        readonly List<GameObject> visuals = new List<GameObject>();
        StoredMagic? pending;
        MortarStrike strike;
        int chargeTurns;
        bool casting;
        int castingIndex;
        public MagicType? RememberedSpell => pending?.Spell;
        public MagicType? RememberedElement => pending?.Element;
        public IReadOnlyList<StoredMagic> StoredSpells => stored;
        public int ChargeTurns => chargeTurns;
        public bool IsCasting => casting;
        public bool HasMarker => strike != null && strike.IsAimed;
        public bool IsEnraged => Hp * 2 <= MaxHp;
        public static bool IsCastingAny
        {
            get { foreach (var boss in FindObjectsOfType<SurgeBossEnemy>()) if (boss.casting) return true; return false; }
        }
        // 숫자와 규칙을 바꾸면 English.json도 함께 고친다.
        public override string AttackDescription => "턴마다 마지막으로 맞은 속성과 주문 형태를 하나 저장하고 3번째 턴에 순서대로 연속 시전한다. 각 마법은 공격력의 절반에 형태 배율을 적용하며 같은 원소 효과를 낸다. Spawn과 아군 공격은 저장하지 않는다. 주문이 없는 턴에도 저장분은 유지된다.";

        protected override void OnIncomingSpell(MagicType element, MagicType spell) => RememberElement(spell, element);
        public void Remember(MagicType spell) => RememberElement(spell, MagicType.Holy);
        public void RememberElement(MagicType spell, MagicType element)
        {
            if (casting || !IsAlive || (spell != MagicType.Shoot && spell != MagicType.Drop && spell != MagicType.Explode)) return;
            pending = new StoredMagic(spell, element);
            if (strike == null) strike = new MortarStrike(markerSprite, shellSprite);
            strike.Clear();
            if (spell == MagicType.Drop) strike.Aim();
            RefreshMemory();
        }
        protected override void Start() { base.Start(); RefreshMemory(); }
        void RefreshMemory()
        {
            int count = stored.Count + (pending.HasValue ? 1 : 0);
            for (int i = 0; i < 3; i++)
            {
                if (i >= Mathf.Max(1, count) || (casting && i < castingIndex)) { BossIntent.Hide(memories[i]); continue; }
                StoredMagic? magic = i < stored.Count ? stored[i] : pending;
                BossIntent.ActionIcon icon = !magic.HasValue ? BossIntent.ActionIcon.Front
                    : magic.Value.Spell == MagicType.Shoot ? BossIntent.ActionIcon.Shoot
                    : magic.Value.Spell == MagicType.Drop ? BossIntent.ActionIcon.Drop : BossIntent.ActionIcon.Explode;
                string message = !magic.HasValue ? "마법 축적 중" : magic.Value.Spell == MagicType.Shoot ? "모방 Shoot: 앞줄"
                    : magic.Value.Spell == MagicType.Drop ? "모방 Drop: 표시한 자리" : "모방 Explode: 아군 전체";
                BossIntent.Show(ref memories[i], transform, icon, message, new Color32(151, 255, 204, 255),
                    imminent: casting ? i == castingIndex : chargeTurns == 2, copied: magic.HasValue);
                memories[i].HorizontalOffset = (i - (Mathf.Max(1, count) - 1) * .5f) * 1.05f;
                memories[i].Element = magic?.Element;
                memories[i].Detail = Core.Localization.Translate("축적") + " " + chargeTurns + "/3";
            }
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (casting) return;
            if (pending.HasValue) { stored.Add(pending.Value); pending = null; }
            strike?.Clear();
            chargeTurns++;
            if (chargeTurns < 3) { Animator.Windup(); RefreshMemory(); return; }
            StartCoroutine(Release());
        }
        IEnumerator Release()
        {
            casting = true;
            castingIndex = 0;
            RefreshMemory();
            StoredMagic[] sequence = stored.ToArray();
            try
            {
                if (sequence.Length == 0)
                {
                    Animator.RangeAttack();
                    yield return Cast(new StoredMagic(MagicType.Shoot, MagicType.Holy), AttackDamage);
                }
                else foreach (StoredMagic magic in sequence)
                {
                    if (!IsAlive) yield break;
                    RefreshMemory();
                    Animator.RangeAttack();
                    yield return Cast(magic, Mathf.Max(1, AttackDamage / 2));
                    castingIndex++;
                    RefreshMemory();
                    yield return new WaitForSeconds(.18f);
                }
            }
            finally
            {
                casting = false;
                stored.Clear(); chargeTurns = 0;
                ClearVisuals();
                if (IsAlive) { Animator.EndWindup(); RefreshMemory(); }
            }
        }
        IEnumerator Cast(StoredMagic magic, int damage)
        {
            Vector3 from = EnemyMagicVisual.Center(this);
            var charge = EnemyMagicVisual.Spawn(MagicType.Explode, magic.Element, from, .35f);
            if (charge != null) visuals.Add(charge.gameObject);
            yield return new WaitForSeconds(.25f);
            if (!IsAlive) yield break;
            if (charge != null) Destroy(charge.gameObject);
            Player player = Player.PlayerInt();
            if (player == null) yield break;
            var targets = new List<Component>();
            var formation = AllyFormation.Current;
            if (magic.Spell == MagicType.Explode)
            {
                if (formation != null) foreach (var ally in formation.Allies) if (ally.IsAlive) targets.Add(ally);
                if (targets.Count == 0) targets.Add(player);
            }
            else if (magic.Spell == MagicType.Drop)
            {
                Component target = formation != null && formation.Allies.Count > 0 ? (Component)formation.Allies[formation.Allies.Count - 1] : player;
                targets.Add(target);
            }
            else targets.Add(formation != null && formation.Front != null && formation.Front.transform.position.x <= transform.position.x ? (Component)formation.Front : player);

            float duration = magic.Spell == MagicType.Shoot ? .48f : .55f;
            var effects = new List<EnemyMagicVisual>();
            var destinations = new List<Vector3>();
            var markers = new List<GameObject>();
            foreach (Component target in targets)
            {
                Vector3 destination = EnemyMagicVisual.Center(target);
                if (magic.Spell == MagicType.Drop)
                {
                    var marker = new GameObject("MimicDropMarker");
                    marker.transform.position = new Vector3(destination.x, -3.4f, 0);
                    var renderer = marker.AddComponent<SpriteRenderer>();
                    renderer.sprite = markerSprite; renderer.sortingOrder = 2;
                    visuals.Add(marker);
                    markers.Add(marker);
                }
                Vector3 start = magic.Spell == MagicType.Drop ? destination + Vector3.up * 6 : magic.Spell == MagicType.Explode ? destination : from;
                var effect = magic.Spell == MagicType.Explode ? null : EnemyMagicVisual.Spawn(magic.Spell, magic.Element, start);
                if (effect != null) { visuals.Add(effect.gameObject); if (magic.Spell == MagicType.Shoot) effect.transform.localScale = new Vector3(-1, 1, 1); }
                effects.Add(effect); destinations.Add(destination);
            }
            for (float time = 0; time < duration && IsAlive; time += Time.deltaTime)
            {
                for (int i = 0; i < effects.Count; i++) if (effects[i] != null)
                {
                    Vector3 start = magic.Spell == MagicType.Drop ? destinations[i] + Vector3.up * 6 : magic.Spell == MagicType.Explode ? destinations[i] : from;
                    effects[i].transform.position = Vector3.Lerp(start, destinations[i], Mathf.Clamp01(time / duration));
                }
                yield return null;
            }
            if (!IsAlive) yield break;
            foreach (var marker in markers) if (marker != null) Destroy(marker);
            int scaled = Mathf.Max(1, Mathf.FloorToInt(Combat.Spells.SpellObj.GetBaseDamage(damage, magic.Spell)));
            for (int i = 0; i < targets.Count; i++)
            {
                Component target = targets[i];
                if (target == null || (target is AllySlime ally && !ally.IsAlive)) continue;
                if (magic.Spell == MagicType.Drop && Mathf.Abs(EnemyMagicVisual.Center(target).x - destinations[i].x) > .7f) continue;
                if (magic.Spell == MagicType.Explode)
                {
                    effects[i] = EnemyMagicVisual.Spawn(magic.Spell, magic.Element, destinations[i]);
                    if (effects[i] != null) visuals.Add(effects[i].gameObject);
                }
                if (effects[i] != null) { effects[i].transform.position = destinations[i]; effects[i].Impact(); }
                FriendlyElementalStatus.Hit(target, magic.Element, magic.Spell, scaled);
            }
            HitImpact.Play(HitStrength.Weak);
            EnemyMagicVisual.PlayImpactSound(destinations[0]);
            yield return new WaitForSeconds(.3f);
        }
        void ClearVisuals() { foreach (var visual in visuals) if (visual != null) Destroy(visual); visuals.Clear(); }
        void OnDisable()
        {
            StopAllCoroutines(); casting = false; pending = null; stored.Clear(); chargeTurns = 0;
            strike?.Clear(); ClearVisuals();
            foreach (var memory in memories) BossIntent.Hide(memory);
        }
        protected override void Death() { OnDisable(); base.Death(); }
    }
}
