using System;
using Cards;
using Combat.Enemies;
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
        public const int MaxHp = 8;
        public const int AttackDamage = 4;
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
            hp = MaxHp;
            if (bodyRenderer != null) bodyRenderer.color = BodyColorOf(element);
            UpdateHpText();
        }

        public void Attack(Enemy enemy)
        {
            if (!IsAlive || enemy == null || !enemy.IsAlive) return;
            slimeAnimator.Attack();
            // 신성은 언데드를 뺀 모든 속성에 음수 상성이라 공격하면 적이 회복된다. 신성 주문이
            // 워드를 회복하는 데 쓰이는 것처럼, 신성 슬라임은 적을 공격하지 않고 워드를 회복한다.
            if (Element == MagicType.Holy)
            {
                Player.PlayerInt().TakeHit(-AttackDamage);
                return;
            }
            enemy.TakeSpellHit(Element, MagicType.Spawn, AttackDamage,
                affinityTable.GetAffinity(Element, enemy.enemyType));
        }

        public void TakeHit(int damage)
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
