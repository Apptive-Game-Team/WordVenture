using System.Collections;
using Cards;
using Combat.Allies;
using DG.Tweening;
using UnityEngine;

namespace Combat.Enemies
{
    public class GraftBossEnemy : BossEnemy
    {
        [SerializeField] Sprite markerSprite;
        [SerializeField] Sprite shellSprite;
        Enemy markedEnemy;
        AllySlime markedAlly;
        MagicType? absorbedElement;
        BossIntent tell;
        BossIntent targetTell;
        MortarStrike strike;
        int absorbedEnemyId = -1;
        GraftAbsorptionVfx absorption;
        public bool IsAbsorbing => absorption != null;
        public static bool IsAbsorbingAny
        {
            get { foreach (var boss in FindObjectsOfType<GraftBossEnemy>()) if (boss.IsAbsorbing) return true; return false; }
        }
        public Enemy MarkedEnemy => markedEnemy != null && markedEnemy.IsAlive ? markedEnemy : null;
        public AllySlime MarkedAlly => markedAlly != null && markedAlly.IsAlive ? markedAlly : null;
        public MagicType? AbsorbedElement => absorbedElement;
        public int AbsorbedEnemyId => absorbedEnemyId;
        public bool HasMarker => strike != null && strike.IsAimed;
        public override string AttackDescription => "곡사나 돌진 슬라임을 우선 지목하고, 없으면 워드의 맨 앞 소환 아군을 지목해 다음 턴에 흡수한다. 다음 턴에 훔친 기술이나 아군의 속성 마법을 한 번 쓰며, 대상이 먼저 죽으면 흡수를 취소하고 대상이 없으면 뿌리로 앞줄을 공격한다.";

        protected override void Start()
        {
            base.Start();
            BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Absorb, "접목 준비", new Color(0.7f, 1f, 0.5f));
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (IsAbsorbing || GetComponent<EnemyFrontAttack>() != null) return;
            if (tell != null) tell.Element = null;
            if (absorbedEnemyId >= 0 || absorbedElement.HasValue)
            {
                int technique = absorbedEnemyId;
                absorbedEnemyId = -1;
                if (absorbedElement.HasValue)
                {
                    Animator.RangeAttack();
                    EnemyFrontAttack.Begin(this, AttackDamage, absorbedElement.Value);
                    absorbedElement = null;
                }
                else if (technique == 20)
                {
                    Animator.RangeAttack();
                    strike.Fire(transform.position, AttackDamage);
                }
                else StartCoroutine(Dash(distanceToFrontLine));
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Absorb, "접목 준비", new Color(0.7f, 1f, 0.5f));
                return;
            }
            // 지목된 적이 죽었다면 이번 행동은 재지목한다. 살아 있으면 흡수만 하고 공격하지 않는다.
            if (MarkedEnemy != null || MarkedAlly != null)
            {
                if (MarkedEnemy != null)
                {
                    absorbedEnemyId = markedEnemy.EnemyId;
                    absorption = GraftAbsorptionVfx.Begin(this, markedEnemy);
                    markedEnemy.gameObject.SetActive(false);
                }
                else
                {
                    absorbedElement = markedAlly.Element;
                    absorption = GraftAbsorptionVfx.Begin(this, markedAlly);
                    markedAlly.Absorb();
                }
                markedEnemy = null;
                markedAlly = null;
                ClearTargetTell();
                Animator.RangeAttack();
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Absorb, "흡수 중", new Color(.75f, 1f, .3f), imminent: true);
                StartCoroutine(FinishAbsorption());
                return;
            }
            markedEnemy = null;
            markedAlly = null;
            ClearTargetTell();
            float closest = float.PositiveInfinity;
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                if (!enemy.IsAlive || enemy is BossEnemy || (enemy.EnemyId != 20 && enemy.EnemyId != 21)) continue;
                float distance = Mathf.Abs(enemy.transform.position.x - transform.position.x);
                if (distance >= closest) continue;
                closest = distance;
                markedEnemy = enemy;
            }
            if (markedEnemy == null && AllyFormation.Current != null) markedAlly = AllyFormation.Current.Front;
            Component target = markedEnemy != null ? (Component)markedEnemy : markedAlly;
            if (target != null)
            {
                BossIntent.Show(ref targetTell, target.transform, BossIntent.ActionIcon.Absorb,
                    "다음 턴 흡수", new Color(0.75f, 1f, 0.3f), imminent: true);
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Absorb, "흡수 예고", new Color(0.75f, 1f, 0.3f), imminent: true);
                Animator.RangeAttack();
            }
            else
            {
                Animator.RangeAttack();
                EnemyFrontAttack.Begin(this, AttackDamage);
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Absorb, "접목 준비", new Color(0.7f, 1f, 0.5f));
            }
        }
        IEnumerator FinishAbsorption()
        {
            while (absorption != null) yield return null;
            if (!IsAlive) yield break;
            if (absorbedElement.HasValue)
            {
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Shoot, "다음 턴 접목 마법", Color.yellow, imminent: true);
                tell.Element = absorbedElement;
            }
            else if (absorbedEnemyId == 20)
            {
                if (strike == null) strike = new MortarStrike(markerSprite, shellSprite);
                strike.Aim();
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Drop, "다음 턴 접목 포격", Color.yellow, imminent: true);
            }
            else
            {
                Animator.Windup();
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Charge, "다음 턴 접목 돌진", Color.yellow, imminent: true);
            }
        }
        void LateUpdate()
        {
            if ((markedEnemy != null && !markedEnemy.IsAlive) || (targetTell != null && MarkedEnemy == null && MarkedAlly == null))
            {
                markedEnemy = null;
                markedAlly = null;
                ClearTargetTell();
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Absorb, "접목 준비", new Color(0.7f, 1f, 0.5f));
            }
        }
        IEnumerator Dash(float distance)
        {
            Animator.EndWindup();
            FaceToDirection(-1);
            float targetX = transform.position.x - Mathf.Max(0, distance - 1.2f);
            Animator.MoveStart();
            yield return transform.DOMoveX(targetX, 0.3f).WaitForCompletion();
            if (!IsAlive) yield break;
            Animator.Attack();
            AllyFormation.HitFrontLine(AttackDamage, transform.position.x);
        }
        void ClearTargetTell()
        {
            if (targetTell != null) Destroy(targetTell.gameObject);
            targetTell = null;
        }
        protected override void Death()
        {
            StopAllCoroutines();
            ClearAbsorption();
            strike?.Clear();
            ClearTargetTell();
            base.Death();
        }
        protected override void StopMove() => Animator.MoveEnd();
        void OnDisable()
        {
            transform.DOKill();
            StopAllCoroutines();
            ClearAbsorption();
            strike?.Clear();
            markedEnemy = null;
            markedAlly = null;
            absorbedElement = null;
            absorbedEnemyId = -1;
            ClearTargetTell();
            BossIntent.Hide(tell);
        }
        void ClearAbsorption()
        {
            if (absorption != null) Destroy(absorption.gameObject);
            absorption = null;
        }
    }
}
