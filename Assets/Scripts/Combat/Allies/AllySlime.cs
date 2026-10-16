using System;
using System.Collections.Generic;
using Cards;
using Combat.Enemies;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Combat.Allies
{
    // Spawn 주문으로 소환하는 아군 슬라임. 적 공격을 한두 번 대신 받고 작은 피해를 보태는
    // 보조 역할이라서 적 슬라임(HP 10~30, 공격력 10)보다 약하게 둔다.
    // 적과 구별되도록 prefab에서 오른쪽을 보게 하고, 크기를 줄이고, HP 표시를 초록색으로 칠했다.
    public class AllySlime : MonoBehaviour
    {
        public static int MaxHp => PlayerUpgrades.AllyMaxHp;
        public static int AttackDamage => PlayerUpgrades.AllyAttackDamage;
        const float MoveSeconds = 0.3f;
        const float DeathSeconds = 0.25f;

        SlimeAnimator slimeAnimator;
        SpriteRenderer bodyRenderer;
        TMP_Text hpText;
        MagicAffinityTable affinityTable;
        int hp;

        public MagicType Element { get; private set; }
        public bool IsAlive => hp > 0;
        public event Action<AllySlime> Died;

        void Awake()
        {
            slimeAnimator = GetComponent<SlimeAnimator>();
            bodyRenderer = GetComponent<SpriteRenderer>();
            hpText = GetComponentInChildren<TMP_Text>();
        }

        public void Initialize(MagicType element, MagicAffinityTable affinityTable)
        {
            Element = element;
            this.affinityTable = affinityTable;
            hp = MaxHealth;
            if (bodyRenderer != null) bodyRenderer.color = BodyColorOf(element);
            UpdateHpText();
        }

        // 속성별 역할. 배율은 업그레이드로 늘어난 HP와 공격력에 곱한다.
        public const float RockHpMultiplier = 2f;
        public const float RockDamageTakenMultiplier = 0.7f;
        public const float LightningSecondTargetMultiplier = 0.75f;

        public int MaxHealth => Mathf.RoundToInt(MaxHp * (Element == MagicType.Rock ? RockHpMultiplier : 1f));
        public int CurrentHealth => hp;
        public int MissingHealth => MaxHealth - hp;
        // 신성은 공격하지 않으므로 이 값이 회복량이다.
        public int AttackValue => Mathf.Max(1, Mathf.FloorToInt(AttackDamage * AttackMultiplierOf(Element) * FriendlyElementalStatus.AttackMultiplier(this) + 0.5f));

        static float AttackMultiplierOf(MagicType element)
        {
            switch (element)
            {
                case MagicType.Rock: return 0.5f;
                case MagicType.Fire: return 1.5f;
                case MagicType.Ice: return 0.75f;
                default: return 1f;
            }
        }

        // 공격과 회복은 투사체로 날아가고, 피해와 회복은 투사체가 도착할 때 들어간다.
        // front는 가장 앞의 적, behind는 그 바로 뒤의 적이다(번개만 쓴다).
        public void Act(Enemy front, Enemy behind, IReadOnlyList<AllySlime> allies)
        {
            if (!IsAlive) return;
            var status = GetComponent<FriendlyElementalStatus>();
            if (status != null && status.BeginAction()) return;
            FriendlyElementalStatus.ConsumeAttack(this);
            slimeAnimator.RangeAttack();
            switch (Element)
            {
                case MagicType.Holy:
                    HealWordAndAlly(allies);
                    break;
                case MagicType.Lightning:
                    LaunchAt(front, 1f);
                    LaunchAt(behind, LightningSecondTargetMultiplier);
                    break;
                default:
                    LaunchAt(front, 1f);
                    break;
            }
        }

        void LaunchAt(Enemy enemy, float damageMultiplier)
        {
            if (enemy == null || !enemy.IsAlive) return;
            AllyProjectile.Fire(transform.position, enemy.transform, Element, () =>
            {
                if (enemy == null) return;
                // 불은 화상, 얼음은 냉기, 번개는 감전이 ElementalStatus에서 붙는다.
                enemy.TakeSpellHit(Element, MagicType.Spawn, AttackValue * damageMultiplier,
                    affinityTable.GetAffinity(Element, enemy.enemyType));
            });
        }

        // 워드와, 가장 많이 다친 아군을 함께 회복한다. 다친 아군이 신성 자신뿐일 때만 자신을 회복한다.
        void HealWordAndAlly(IReadOnlyList<AllySlime> allies)
        {
            int amount = AttackValue;
            Player player = Player.PlayerInt();
            if (player != null)
            {
                AllyProjectile.Fire(transform.position, player.transform, Element, () =>
                {
                    if (player != null) player.TakeHit(-amount);
                });
            }
            AllySlime patient = MostDamagedAlly(allies);
            if (patient == null) return;
            AllyProjectile.Fire(transform.position, patient.transform, Element, () =>
            {
                if (patient != null) patient.Heal(amount);
            });
        }

        AllySlime MostDamagedAlly(IReadOnlyList<AllySlime> allies)
        {
            AllySlime best = null;
            foreach (AllySlime ally in allies)
            {
                if (ally == this || ally == null || !ally.IsAlive || ally.MissingHealth <= 0) continue;
                if (best == null || ally.MissingHealth > best.MissingHealth) best = ally;
            }
            if (best == null && MissingHealth > 0) return this;
            return best;
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            hp = Mathf.Min(MaxHealth, hp + amount);
            UpdateHpText();
        }

        // 적의 공격을 받을 때. 바위는 대신 맞는 피해를 30% 덜 받는다.
        public void TakeHit(int damage)
        {
            if (Element == MagicType.Rock && damage > 0)
                damage = Mathf.Max(1, Mathf.RoundToInt(damage * RockDamageTakenMultiplier));
            ApplyDamage(damage);
        }

        // 워드의 주문이 입히는 피해. 적의 공격이 아니므로 바위의 피해 감소를 받지 않는다.
        public void TakeSpellDamage(int damage)
        {
            ApplyDamage(damage);
        }

        void ApplyDamage(int damage)
        {
            if (!IsAlive) return;
            hp = Mathf.Max(0, hp - damage);
            UpdateHpText();
            if (hp > 0)
            {
                slimeAnimator.TakeHit();
                return;
            }

            slimeAnimator.Death();
            Died?.Invoke(this);
            Destroy(gameObject, DeathSeconds);
        }

        public void MoveTo(Vector3 position)
        {
            transform.DOKill();
            transform.DOMove(position, MoveSeconds);
        }

        // 흡수 그림은 별도 사본으로 이동한다. 실제 아군은 즉시 대열에서 빼서 방어하거나 공격하지 않는다.
        public void Absorb()
        {
            if (!IsAlive) return;
            hp = 0;
            transform.DOKill();
            Died?.Invoke(this);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            transform.DOKill();
        }

        void UpdateHpText()
        {
            if (hpText != null) hpText.SetText(hp.ToString());
        }

        static Color BodyColorOf(MagicType element)
        {
            switch (element)
            {
                case MagicType.Fire: return new Color(1f, 0.55f, 0.4f);
                case MagicType.Ice: return new Color(0.55f, 0.85f, 1f);
                case MagicType.Rock: return new Color(0.8f, 0.65f, 0.45f);
                case MagicType.Lightning: return new Color(1f, 1f, 0.45f);
                default: return Color.white;
            }
        }
    }
}
