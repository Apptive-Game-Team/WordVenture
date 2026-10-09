using System.Collections;
using Combat.Allies;
using DG.Tweening;
using UnityEngine;

namespace Combat.Enemies
{
    // 돌진 거리 안에 들어오면 한 적 턴 동안 "돌진 준비"를 표시하고, 다음 적 턴에 맨 앞 아군 앞까지
    // 달려와 들이받는다. 아군 슬라임이 막으면 두 배 피해를 준다.
    public class ChargeEnemy : Enemy
    {
        const float ChargeRange = 6f;
        const float StopDistance = 1.2f;
        const float DashSeconds = 0.3f;
        const int AllyDamageMultiplier = 2;

        bool charging;

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
                SetIntent("돌진 준비");
                return;
            }

            base.TakeTurnAction(distanceToFrontLine);
        }

        IEnumerator Dash(float distanceToFrontLine)
        {
            charging = false;
            SetIntent(string.Empty);
            float targetX = transform.position.x - Mathf.Max(0f, distanceToFrontLine - StopDistance);
            FaceToDirection(-1);
            Animator.MoveStart();
            yield return transform.DOMoveX(targetX, DashSeconds).WaitForCompletion();
            if (!IsAlive) yield break;
            Animator.Attack();
            AllyFormation.HitFrontLine(AttackDamage, transform.position.x, AllyDamageMultiplier);
        }

        void OnDisable()
        {
            transform.DOKill();
            charging = false;
        }
    }
}
