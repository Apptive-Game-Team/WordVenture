using System.Collections;
using Combat.Allies;
using DG.Tweening;
using UnityEngine;

namespace Combat.Enemies
{
    public class ThunderBossEnemy : BossEnemy
    {
        enum Phase { Windup, Dash, Exhaust, Recover }
        Phase phase;
        WindupMotion windup;
        BossIntent tell;
        Vector3 home;
        bool hasHome;
        bool moving;
        public bool IsCharging => phase == Phase.Dash;
        public bool IsExhausted => phase == Phase.Exhaust || phase == Phase.Recover;
        public bool IsRecovering => moving && phase == Phase.Recover;
        public override string AttackDescription => "한 턴 예고한 뒤 앞줄의 한 대상에게 돌진한다. 돌진 다음 턴은 탈진하여 행동하지 않는다. 회복하면 뒤로 뛰어 다음 돌진을 준비한다.";
        protected override void Start()
        {
            base.Start();
            BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Charge, "돌진 준비", Color.yellow);
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (moving) return;
            if (!hasHome) { home = transform.position; hasHome = true; }
            if (phase == Phase.Windup) PrepareDash(0.35f);
            else if (phase == Phase.Dash)
            {
                phase = Phase.Exhaust;
                StartCoroutine(Dash(distanceToFrontLine));
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Exhaust, "다음 턴 탈진", new Color(0.8f, 0.8f, 1));
            }
            else if (phase == Phase.Exhaust)
            {
                phase = Phase.Recover;
                Animator.MoveEnd();
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Exhaust, "탈진 중 다음 턴 후방 도약", new Color(0.8f, 0.8f, 1));
            }
            else StartCoroutine(Recover());
        }

        void PrepareDash(float rearDistance)
        {
            phase = Phase.Dash;
            if (windup == null) windup = gameObject.AddComponent<WindupMotion>();
            windup.Play(rearDistance, 0.04f);
            Animator.Windup();
            BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Charge, "다음 턴 돌진", Color.yellow, imminent: true);
        }

        IEnumerator Recover()
        {
            moving = true;
            if (windup != null) windup.Stop();
            Animator.EndWindup();
            Animator.MoveStart();
            Vector3 destination = home;
            Camera camera = Camera.main;
            SpriteRenderer body = GetComponent<SpriteRenderer>();
            if (camera != null && body != null && body.sprite != null)
            {
                float depth = Mathf.Abs(camera.transform.position.z - transform.position.z);
                float left = camera.ViewportToWorldPoint(new Vector3(0, 0.5f, depth)).x;
                float right = camera.ViewportToWorldPoint(new Vector3(1, 0.5f, depth)).x;
                Bounds visible = ElementalStatusVfx.GetVisibleBodyBounds(body.sprite);
                float halfWidth = Mathf.Abs(visible.extents.x * transform.lossyScale.x);
                float centerOffset = body.transform.TransformPoint(new Vector3(body.flipX ? -visible.center.x : visible.center.x, visible.center.y, 0)).x - transform.position.x;
                if (right - left > halfWidth * 2 + 0.4f)
                    destination.x = Mathf.Clamp(destination.x, left + halfWidth + 0.2f - centerOffset, right - halfWidth - 0.2f - centerOffset);
            }
            // 한 번의 도약 안에 후퇴와 착지를 끝내고 다음 턴부터 바로 돌진한다.
            yield return transform.DOJump(destination, 1.15f, 1, 0.45f).SetEase(Ease.OutQuad).WaitForCompletion();
            moving = false;
            if (!IsAlive) yield break;
            FaceToDirection(-1);
            ThunderLandingVfx.Spawn(body);
            PrepareDash(0f);
        }

        IEnumerator Dash(float distance)
        {
            moving = true;
            float targetX = transform.position.x - Mathf.Max(0, distance - 1.2f);
            if (windup != null) windup.Stop();
            Animator.EndWindup();
            FaceToDirection(-1);
            Animator.MoveStart();
            yield return transform.DOMoveX(targetX, 0.3f).WaitForCompletion();
            moving = false;
            if (!IsAlive) yield break;
            Animator.Attack();
            AllyFormation.HitFrontLine(AttackDamage, transform.position.x);
        }
        protected override void StopMove() => Animator.MoveEnd();
        protected override void Death()
        {
            StopAllCoroutines();
            transform.DOKill();
            if (windup != null) windup.Stop();
            moving = false;
            BossIntent.Hide(tell);
            base.Death();
        }
        void OnDisable()
        {
            transform.DOKill();
            StopAllCoroutines();
            if (windup != null) windup.Stop();
            phase = Phase.Windup;
            moving = false;
            hasHome = false;
            BossIntent.Hide(tell);
        }
    }
}
