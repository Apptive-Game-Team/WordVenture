using System.Collections;
using Combat.Allies;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Combat.Enemies
{
    public class ThunderBossEnemy : BossEnemy
    {
        enum Phase { Windup, Dash, Exhaust }
        Phase phase;
        WindupMotion windup;
        TMP_Text tell;
        public bool IsCharging => phase == Phase.Dash;
        public bool IsExhausted => phase == Phase.Exhaust;
        public override string AttackDescription => "한 턴 예고한 뒤 앞줄의 한 대상에게 돌진한다. 돌진 다음 턴은 탈진하여 행동하지 않는다.";
        protected override void Start()
        {
            base.Start();
            RegionalBossTell.Show(ref tell, transform, HpText, "돌진 준비", Color.yellow);
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (phase == Phase.Windup)
            {
                phase = Phase.Dash;
                if (windup == null) windup = gameObject.AddComponent<WindupMotion>();
                windup.Play(0.35f, 0.04f);
                Animator.Windup();
                RegionalBossTell.Show(ref tell, transform, HpText, "다음 턴 돌진", Color.yellow);
            }
            else if (phase == Phase.Dash)
            {
                phase = Phase.Exhaust;
                StartCoroutine(Dash(distanceToFrontLine));
                RegionalBossTell.Show(ref tell, transform, HpText, "다음 턴 탈진", new Color(0.8f, 0.8f, 1));
            }
            else
            {
                phase = Phase.Windup;
                Animator.MoveEnd();
                RegionalBossTell.Show(ref tell, transform, HpText, "돌진 준비", new Color(0.8f, 0.8f, 1));
            }
        }
        IEnumerator Dash(float distance)
        {
            float targetX = transform.position.x - Mathf.Max(0, distance - 1.2f);
            if (windup != null) windup.Stop();
            Animator.EndWindup();
            FaceToDirection(-1);
            Animator.MoveStart();
            yield return transform.DOMoveX(targetX, 0.3f).WaitForCompletion();
            if (!IsAlive) yield break;
            Animator.Attack();
            AllyFormation.HitFrontLine(AttackDamage, transform.position.x);
        }
        protected override void StopMove() => Animator.MoveEnd();
        void OnDisable()
        {
            transform.DOKill();
            StopAllCoroutines();
            if (windup != null) windup.Stop();
            phase = Phase.Windup;
            RegionalBossTell.Hide(tell);
        }
    }
}
