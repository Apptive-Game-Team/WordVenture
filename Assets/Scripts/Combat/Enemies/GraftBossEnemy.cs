using System.Collections;
using Combat.Allies;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Combat.Enemies
{
    public class GraftBossEnemy : BossEnemy
    {
        [SerializeField] Sprite markerSprite;
        [SerializeField] Sprite shellSprite;
        Enemy markedEnemy;
        TMP_Text tell;
        TMP_Text targetTell;
        MortarStrike strike;
        int absorbedEnemyId = -1;
        public Enemy MarkedEnemy => markedEnemy != null && markedEnemy.IsAlive ? markedEnemy : null;
        public int AbsorbedEnemyId => absorbedEnemyId;
        public bool HasMarker => strike != null && strike.IsAimed;
        public override string AttackDescription => "곡사 또는 돌진 슬라임 하나를 지목하고 다음 턴에 흡수한다. 그 다음 턴에 흡수한 적의 포격이나 돌진을 한 번 쓴다. 지목한 적을 먼저 쓰러뜨리면 흡수가 취소된다.";

        protected override void Start()
        {
            base.Start();
            RegionalBossTell.Show(ref tell, transform, HpText, "접목 준비", new Color(0.7f, 1f, 0.5f));
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (absorbedEnemyId >= 0)
            {
                int technique = absorbedEnemyId;
                absorbedEnemyId = -1;
                if (technique == 20)
                {
                    Animator.RangeAttack();
                    strike.Fire(transform.position, AttackDamage);
                }
                else StartCoroutine(Dash(distanceToFrontLine));
                RegionalBossTell.Show(ref tell, transform, HpText, "접목 준비", new Color(0.7f, 1f, 0.5f));
                return;
            }
            // 지목된 적이 죽었다면 이번 행동은 재지목한다. 살아 있으면 흡수만 하고 공격하지 않는다.
            if (MarkedEnemy != null)
            {
                absorbedEnemyId = markedEnemy.EnemyId;
                markedEnemy.gameObject.SetActive(false);
                markedEnemy = null;
                ClearTargetTell();
                Animator.RangeAttack();
                if (absorbedEnemyId == 20)
                {
                    if (strike == null) strike = new MortarStrike(markerSprite, shellSprite);
                    strike.Aim();
                    RegionalBossTell.Show(ref tell, transform, HpText, "다음 턴 접목 포격", Color.yellow);
                }
                else
                {
                    Animator.Windup();
                    RegionalBossTell.Show(ref tell, transform, HpText, "다음 턴 접목 돌진", Color.yellow);
                }
                return;
            }
            markedEnemy = null;
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
            if (markedEnemy != null)
            {
                RegionalBossTell.Show(ref targetTell, markedEnemy.transform, markedEnemy.GetComponentInChildren<TMP_Text>(),
                    "다음 턴 흡수", new Color(0.75f, 1f, 0.3f));
                RegionalBossTell.Show(ref tell, transform, HpText, "흡수 예고", new Color(0.75f, 1f, 0.3f));
                Animator.RangeAttack();
            }
            else
            {
                Animator.RangeAttack();
                AllyFormation.HitFrontLine(AttackDamage, transform.position.x);
                RegionalBossTell.Show(ref tell, transform, HpText, "접목 준비", new Color(0.7f, 1f, 0.5f));
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
            strike?.Clear();
            ClearTargetTell();
            base.Death();
        }
        protected override void StopMove() => Animator.MoveEnd();
        void OnDisable()
        {
            transform.DOKill();
            StopAllCoroutines();
            strike?.Clear();
            markedEnemy = null;
            absorbedEnemyId = -1;
            ClearTargetTell();
            RegionalBossTell.Hide(tell);
        }
    }
}
