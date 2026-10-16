using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    // 실제 세계수와 다른, 나무를 닮은 슬라임 외피다. 쓰러지면 안에 있던 금빛 본체가 드러난다.
    public sealed class TreeShellBossEnemy : BossEnemy
    {
        public const float GroundY = -2.5f;
        BossIntent tell;
        bool rootsReady;
        bool transitionStarted;
        public bool RootsReady => rootsReady;
        public override int SummonedEnemyId => 29;
        public override string AttackDescription => "한 턴 동안 뿌리 공격을 예고하고 다음 턴에 앞줄을 공격한다. 쓰러지면 금빛 액체가 모여 세계수 슬라임의 본체가 나타난다.";
        protected override void Start()
        {
            base.Start();
            Vector3 position = transform.position;
            position.y = GroundY;
            transform.position = position;
            BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Roots, "뿌리 공격 준비", new Color(1f, 0.85f, 0.4f));
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (GetComponent<EnemyFrontAttack>() != null) return;
            if (!rootsReady)
            {
                rootsReady = true;
                Animator.Windup();
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Roots, "다음 턴 뿌리 공격", new Color(1f, 0.85f, 0.4f), imminent: true);
                return;
            }
            rootsReady = false;
            Animator.EndWindup();
            Animator.RangeAttack();
            EnemyFrontAttack.Begin(this, AttackDamage);
            BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Roots, "뿌리 공격 준비", new Color(1f, 0.85f, 0.4f));
        }
        protected override void Death()
        {
            if (transitionStarted) return;
            transitionStarted = true;
            BossIntent.Hide(tell);
            base.Death();
            BattleWaveController.BeginWorldTreeTransition(this);
        }
        protected override void StopMove() => Animator.MoveEnd();
    }
}
