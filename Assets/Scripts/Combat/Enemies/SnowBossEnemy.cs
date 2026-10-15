using UnityEngine;

namespace Combat.Enemies
{
    public class SnowBossEnemy : BossEnemy
    {
        SnowballEnemy snowball;
        BossIntent tell;
        public SnowballEnemy Snowball => snowball != null && snowball.IsAlive ? snowball : null;
        public override int SummonedEnemyId => 34;
        // 수치를 바꾸면 English.json 도 고친다.
        public override string AttackDescription => "눈덩이를 만들고 3단계까지 키운 다음 턴에 던진다. 눈덩이를 파괴하면 크기에 따라 본체가 8, 16, 28 피해를 받는다.";

        protected override void Start()
        {
            base.Start();
            BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Snowball, "눈덩이 준비", new Color(0.65f, 0.9f, 1f));
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (Snowball == null)
            {
                GameObject created = BattleWaveController.SpawnExtraEnemy(SummonedEnemyId, transform.position.x - 2f);
                if (created != null)
                {
                    snowball = created.GetComponent<SnowballEnemy>();
                    snowball.Initialize(this);
                }
                Animator.RangeAttack();
            }
            else if (snowball.Growth < 3)
            {
                snowball.Grow();
                Animator.RangeAttack();
            }
            else
            {
                snowball.Throw(AttackDamage);
                snowball = null;
                Animator.RangeAttack();

            }
            BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Snowball,
                Snowball == null ? "눈덩이 준비" : snowball.Growth == 3 ? "다음 턴 눈덩이 투척" : "눈덩이 성장", new Color(0.65f, 0.9f, 1f),
                Snowball != null ? snowball.Growth : 0, imminent: Snowball != null && snowball.Growth == 3);
        }

        void LateUpdate()
        {
            if (tell != null && tell.Level > 0 && Snowball == null)
                BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Snowball, "눈덩이 준비", new Color(0.65f, 0.9f, 1f));
        }
        protected override void Death()
        {
            if (snowball != null && snowball.IsAlive) snowball.Dismiss();
            snowball = null;
            base.Death();
        }
        void OnDisable()
        {
            if (snowball != null && snowball.IsAlive) snowball.Dismiss();
            snowball = null;
            BossIntent.Hide(tell);
        }
        protected override void StopMove() => Animator.MoveEnd();
    }

}
