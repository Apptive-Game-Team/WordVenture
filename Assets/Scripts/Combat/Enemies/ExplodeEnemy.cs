using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    // 맨 앞까지 다가와 한 적 턴 동안 심지 불꽃을 키우고 몸을 번쩍이며 떨고, 다음 적 턴에 터진다. 터지면 아군 슬라임
    // 전체(아군이 없으면 워드)가 피해를 받고 자기도 쓰러진다. 터지기 전에 쓰러뜨리면 주변 적이 피해를 받는다.
    public class ExplodeEnemy : Enemy
    {
        const float BlastRadius = 2.5f;
        const float WindupTrembleDistance = 0.05f;
        // 피해 범위는 그대로 두고 폭발 그림은 보이는 몸 폭의 두 배 이상으로 키운다.
        const float BlastDiameter = BlastRadius * 2f;
        // 폭발 그림(FireExplode) 한 칸은 256px, PPU 256 이라 1 unit 이다. 여백을 잘라낸 스프라이트라 bounds 로는 알 수 없다.
        const float BlastFrameWidth = 1f;

        [SerializeField] Sprite[] blastFrames;

        bool primed;
        bool exploding;
        WindupMotion windup;

        public override string AttackDescription => "맨 앞까지 다가와 한 턴 동안 심지를 태우고, 다음 턴에 터져 아군 슬라임 전체(없으면 워드)에게 피해를 준다. 터지기 전에 쓰러뜨리면 주변의 적이 피해를 받는다.";

        public bool IsPrimed => primed;

        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (primed)
            {
                exploding = true;
                PlayBlast();
                Animator.RangeAttack();
                AllyFormation.HitAllAllies(AttackDamage);
                Kill();
                return;
            }

            if (IsInAttackRange(distanceToFrontLine))
            {
                primed = true;
                Windup.Play(0f, WindupTrembleDistance);
                Animator.Windup();
                return;
            }

            base.TakeTurnAction(distanceToFrontLine);
        }

        protected override void Death()
        {
            bool blastEnemies = primed && !exploding;
            primed = false;
            Windup.Stop();
            if (blastEnemies)
            {
                PlayBlast();
                DamageNearbyEnemies();
            }
            base.Death();
        }

        WindupMotion Windup => windup != null ? windup : windup = gameObject.AddComponent<WindupMotion>();

        // 슬라임은 곧 꺼지므로 폭발 그림은 슬라임과 따로 재생한다.
        void PlayBlast()
        {
            SpriteRenderer body = GetComponent<SpriteRenderer>();
            Vector3 center = transform.position;
            float diameter = BlastDiameter;
            if (body != null && body.sprite != null)
            {
                Bounds visible = ElementalStatusVfx.GetVisibleBodyBounds(body.sprite);
                Vector3 localCenter = visible.center;
                if (body.flipX) localCenter.x = -localCenter.x;
                if (body.flipY) localCenter.y = -localCenter.y;
                center = body.transform.TransformPoint(localCenter);
                diameter = Mathf.Max(diameter, visible.size.x * Mathf.Abs(body.transform.lossyScale.x) * 2f);
            }
            BlastVfx.Spawn(center, blastFrames, diameter, BlastFrameWidth, body);
            HitImpact.Play(HitStrength.Normal);
            EnemyMagicVisual.PlayImpactSound(center, .7f);
        }

        void DamageNearbyEnemies()
        {
            float x = transform.position.x;
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                if (enemy == this || !enemy.IsAlive) continue;
                if (Mathf.Abs(enemy.transform.position.x - x) <= BlastRadius) enemy.TakeHit(Damage);
            }
        }
    }
}
