using UnityEngine;

namespace Combat.Enemies
{
    // 2부 최종 보스 "폭주한 마력의 형상". 곡사 포격을 조준과 착탄으로 번갈아 쓰고, HP가 처음 절반
    // 아래로 떨어진 다음 적 턴에는 포격 대신 분열 슬라임(splitEnemyId) 두 마리를 부른다.
    // BossEnemy라서 빙결 대신 공격 약화를 받는다.
    public class SurgeBossEnemy : BossEnemy
    {
        const float SummonOffset = 1.2f;

        [SerializeField] Sprite markerSprite;
        [SerializeField] Sprite shellSprite;
        [SerializeField] int splitEnemyId;
        MortarStrike strike;
        bool summoned;

        MortarStrike Strike => strike ??= new MortarStrike(markerSprite, shellSprite);
        public override string AttackDescription => "곡사 슬라임처럼 표시를 놓고 다음 턴에 포탄을 쏜다. 체력이 처음 절반 아래로 떨어지면 분열 슬라임 두 마리를 부른다.";

        public override int SummonedEnemyId => splitEnemyId;

        public bool HasMarker => Strike.IsAimed;
        public bool HasSummoned => summoned;

        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (!summoned && Hp * 2 <= MaxHp)
            {
                summoned = true;
                Animator.RangeAttack();
                float x = transform.position.x;
                BattleWaveController.SpawnExtraEnemy(splitEnemyId, x - SummonOffset);
                BattleWaveController.SpawnExtraEnemy(splitEnemyId, x + SummonOffset);
                return;
            }

            if (!Strike.IsAimed)
            {
                Strike.Aim();
                return;
            }

            Animator.RangeAttack();
            Strike.Fire(transform.position, AttackDamage);
        }

        void OnDisable()
        {
            strike?.Clear();
        }
    }
}
