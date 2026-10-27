using System.Collections;
using System.Collections.Generic;
using Cards;
using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    public class FurnaceBossEnemy : BossEnemy
    {
        public const int MaxHeat = 3;
        public const int DamagePerHeat = 3;
        BossIntent tell;
        int shownDamage = -1;
        bool releasing;
        readonly List<GameObject> effects = new List<GameObject>();
        public bool IsReleasing => releasing;
        public static bool IsReleasingAny
        {
            get { foreach (var boss in FindObjectsOfType<FurnaceBossEnemy>()) if (boss.releasing) return true; return false; }
        }
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
            BossIntent.Show(ref tell, transform, BossIntent.ActionIcon.Heat, "열기 방출", new Color(1f, 0.6f, 0.25f), Heat, imminent: Heat == MaxHeat);
            shownDamage = NextAttackDamage;
            tell.DamagePreview = shownDamage;
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (releasing) return;
            int damage = Status.GetAttackDamage(EnemyRage.ApplyToDamage(Damage, RageLevel) + Heat * DamagePerHeat);
            int heat = Heat;
            Heat = 0;
            ShowHeat();
            StartCoroutine(ReleaseHeat(heat, damage));
        }
        IEnumerator ReleaseHeat(int heat, int damage)
        {
            releasing = true;
            Animator.Windup();
            Vector3 center = EnemyMagicVisual.Center(this);
            var embers = new EnemyMagicVisual[3];
            for (int i = 0; i < 3; i++)
            {
                embers[i] = EnemyMagicVisual.Spawn(MagicType.Shoot, MagicType.Fire, center, .35f);
                if (embers[i] != null) effects.Add(embers[i].gameObject);
            }
            for (float time = 0; time < .35f && IsAlive; time += Time.deltaTime)
            {
                for (int i = 0; i < 3; i++) if (embers[i] != null)
                {
                    float angle = (i * 120 + time * 400) * Mathf.Deg2Rad;
                    embers[i].transform.position = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * Mathf.Lerp(1.7f, .15f, time / .35f);
                }
                yield return null;
            }
            foreach (var ember in embers) if (ember != null) Destroy(ember.gameObject);
            if (!IsAlive) yield break;
            Animator.EndWindup(); Animator.RangeAttack();
            var flash = EnemyMagicVisual.Spawn(MagicType.Explode, MagicType.Fire, center, .45f + heat * .1f);
            if (flash != null) { effects.Add(flash.gameObject); flash.Impact(); }
            Player player = Player.PlayerInt();
            if (player == null) { releasing = false; yield break; }
            var formation = AllyFormation.Current;
            Component target = formation != null && formation.Front != null && formation.Front.transform.position.x <= transform.position.x ? (Component)formation.Front : player;
            Vector3 destination = EnemyMagicVisual.Center(target);
            var wave = EnemyMagicVisual.Spawn(MagicType.Shoot, MagicType.Fire, center, 1.2f + heat * .35f);
            if (wave != null) { effects.Add(wave.gameObject); wave.transform.localScale = new Vector3(-1, 1, 1); }
            for (float time = 0; time < .45f && IsAlive; time += Time.deltaTime)
            {
                if (wave != null) wave.transform.position = Vector3.Lerp(center, destination, time / .45f);
                yield return null;
            }
            if (!IsAlive) yield break;
            if (wave != null) { wave.transform.position = destination; wave.Impact(); }
            // 발사할 때 보인 대상에게 도착한 순간만 피해를 준다. 쓰러진 앞줄 뒤로 관통하지 않는다.
            if (target != null && (!(target is AllySlime ally) || ally.IsAlive))
            {
                if (target is AllySlime blocker) blocker.TakeHit(damage); else player.TakeHit(damage);
            }
            HitImpact.Play(heat == MaxHeat ? HitStrength.Reaction : HitStrength.Weak);
            EnemyMagicVisual.PlayImpactSound(destination, .45f + heat * .1f);
            yield return new WaitForSeconds(.35f);
            releasing = false;
            ClearEffects();
        }
        void ClearEffects() { foreach (var effect in effects) if (effect != null) Destroy(effect); effects.Clear(); }
        protected override void StopMove() => Animator.MoveEnd();
        void OnDisable()
        {
            StopAllCoroutines(); releasing = false; ClearEffects();
            Heat = 0;
            BossIntent.Hide(tell);
        }
        protected override void Death() { OnDisable(); base.Death(); }
    }
}
