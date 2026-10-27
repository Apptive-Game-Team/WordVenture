using System.Collections;
using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    public class SnowballEnemy : Enemy
    {
        SnowBossEnemy owner;
        BossIntent tell;
        Vector3 initialScale;
        bool dismissed;
        public SnowBossEnemy Owner => owner;
        public int Growth { get; private set; } = 1;
        public int CounterDamage => Growth == 1 ? 8 : Growth == 2 ? 16 : 28;
        public override string AttackDescription => "공격하지 않는다. 설인 슬라임이 키우는 눈덩이로, 파괴하면 성장 단계에 따라 설인 슬라임에 8, 16, 28 피해를 준다.";

        public void Initialize(SnowBossEnemy boss)
        {
            owner = boss;
            Growth = 1;
            dismissed = false;
            initialScale = transform.localScale;
            ShowGrowth();
        }
        public void Grow()
        {
            Growth = Mathf.Min(3, Growth + 1);
            transform.localScale = initialScale * (1f + (Growth - 1) * 0.25f);
            ShowGrowth();
        }
        void ShowGrowth()
        {
            BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Snowball,
                Growth == 3 ? "다음 턴 눈덩이 투척" : "눈덩이 성장", Color.cyan, Growth, imminent: Growth == 3);
        }
        public void Throw(int damage)
        {
            SnowballFlight.Launch(GetComponent<SpriteRenderer>(), damage);
            Dismiss();
        }
        public void Dismiss()
        {
            dismissed = true;
            owner = null;
            gameObject.SetActive(false);
        }
        protected override void TakeTurnAction(float distanceToFrontLine) { }
        protected override void Death()
        {
            if (!dismissed && owner != null && owner.IsAlive) owner.TakeHit(CounterDamage);
            owner = null;
            base.Death();
        }
        void OnDisable() => BossIntent.Hide(tell);
    }
    internal sealed class SnowballFlight : MonoBehaviour
    {
        int damage;
        Vector3 start;
        Vector3 target;
        public static void Launch(SpriteRenderer source, int damage)
        {
            var go = new GameObject("ThrownSnowball");
            go.transform.position = source.transform.position;
            go.transform.localScale = source.transform.lossyScale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = source.sprite;
            renderer.color = source.color;
            renderer.sortingOrder = source.sortingOrder + 5;
            var flight = go.AddComponent<SnowballFlight>();
            flight.damage = damage;
            flight.start = source.transform.position;
            flight.target = new Vector3(AllyFormation.FrontLineX(Player.PlayerInt().transform.position.x), flight.start.y, flight.start.z);
            flight.StartCoroutine(flight.Fly());
        }
        IEnumerator Fly()
        {
            float elapsed = 0;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.4f);
                transform.position = Vector3.Lerp(start, target, t) + Vector3.up * (2f * 4f * t * (1f - t));
                yield return null;
            }
            AllyFormation.HitFrontLine(damage, start.x);
            Destroy(gameObject);
        }
    }
}
