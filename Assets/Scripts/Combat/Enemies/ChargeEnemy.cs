using System.Collections;
using Combat.Allies;
using DG.Tweening;
using UnityEngine;

namespace Combat.Enemies
{
    // 돌진 거리 안에 들어오면 한 적 턴 동안 뒤로 물러나 눈썹을 찌푸리고 웅크리며 떨고, 다음 적 턴에 맨 앞 아군 앞까지
    // 달려와 들이받는다. 아군 슬라임이 막으면 두 배 피해를 준다.
    public class ChargeEnemy : Enemy
    {
        const float ChargeRange = 6f;
        const float StopDistance = 1.2f;
        const float DashSeconds = 0.3f;
        const int AllyDamageMultiplier = 2;
        const float WindupRearDistance = 0.4f;
        const float WindupTrembleDistance = 0.04f;

        bool charging;
        WindupMotion windup;

        public bool IsCharging => charging;

        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (charging)
            {
                StartCoroutine(Dash(distanceToFrontLine));
                return;
            }

            if (distanceToFrontLine <= ChargeRange)
            {
                charging = true;
                Windup.Play(WindupRearDistance, WindupTrembleDistance);
                Animator.Windup();
                return;
            }

            base.TakeTurnAction(distanceToFrontLine);
        }

        IEnumerator Dash(float distanceToFrontLine)
        {
            charging = false;
            // distanceToFrontLine은 떨고 있던 자리에서 잰 거리이므로 동작을 멈추기 전에 목표를 정한다.
            float targetX = transform.position.x - Mathf.Max(0f, distanceToFrontLine - StopDistance);
            Windup.Stop();
            Animator.EndWindup();
            FaceToDirection(-1);
            Animator.MoveStart();
            yield return transform.DOMoveX(targetX, DashSeconds).WaitForCompletion();
            if (!IsAlive) yield break;
            Animator.Attack();
            AllyFormation.HitFrontLine(AttackDamage, transform.position.x, AllyDamageMultiplier);
        }

        WindupMotion Windup => windup != null ? windup : windup = gameObject.AddComponent<WindupMotion>();

        void OnDisable()
        {
            transform.DOKill();
            charging = false;
        }
    }
}
