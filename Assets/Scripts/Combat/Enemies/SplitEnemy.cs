using UnityEngine;

namespace Combat.Enemies
{
    // 쓰러지면 HP가 절반인 작은 슬라임(childEnemyId) 두 마리로 나뉜다.
    public class SplitEnemy : SwordEnemy
    {
        const float ChildOffset = 0.6f;

        [SerializeField] int childEnemyId;

        public override string AttackDescription => "근접 공격한다. 쓰러지면 작은 분열 슬라임 두 마리로 나뉜다.";

        public override int SummonedEnemyId => childEnemyId;

        protected override void Death()
        {
            base.Death();
            float x = transform.position.x;
            BattleWaveController.SpawnExtraEnemy(childEnemyId, x - ChildOffset);
            BattleWaveController.SpawnExtraEnemy(childEnemyId, x + ChildOffset);
        }
    }
}
