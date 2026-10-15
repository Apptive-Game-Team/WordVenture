using Combat.Allies;
using TMPro;
using UnityEngine;

namespace Combat.Enemies
{
    public class FurnaceBossEnemy : BossEnemy
    {
        public const int MaxHeat = 3;
        public const int DamagePerHeat = 3;
        TMP_Text tell;
        int shownDamage = -1;
        public int Heat { get; private set; }
        public int NextAttackDamage => Mathf.FloorToInt((EnemyRage.ApplyToDamage(Damage, RageLevel) + Heat * DamagePerHeat) * Status.AttackMultiplier);
        public override string AttackDescription => "주문이나 아군 슬라임의 직접 타격마다 열기가 1 쌓이고 최대 3까지 오른다. 다음 공격은 열기마다 3 추가 피해를 주고 열기를 모두 소모한다. 화상은 열기를 쌓지 않는다.";
        protected override void Start()
        {
            base.Start();
            ShowHeat();
        }
        protected override void OnDirectDamage(int damage)
        {
            Heat = Mathf.Min(MaxHeat, Heat + 1);
            ShowHeat();
        }
        void LateUpdate()
        {
            if (IsAlive && NextAttackDamage != shownDamage) ShowHeat();
        }
        void ShowHeat()
        {
            RegionalBossTell.Show(ref tell, transform, HpText, "열기 방출", new Color(1f, 0.6f, 0.25f));
            shownDamage = NextAttackDamage;
            tell.text += " " + Heat + "/" + MaxHeat + "  " + shownDamage;
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            int damage = Status.GetAttackDamage(EnemyRage.ApplyToDamage(Damage, RageLevel) + Heat * DamagePerHeat);
            Heat = 0;
            Animator.RangeAttack();
            AllyFormation.HitFrontLine(damage, transform.position.x);
            ShowHeat();
        }
        protected override void StopMove() => Animator.MoveEnd();
        void OnDisable()
        {
            Heat = 0;
            RegionalBossTell.Hide(tell);
        }
    }
}
