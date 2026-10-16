using System.Collections;
using System.Collections.Generic;
using Cards;
using Combat.Allies;
using Combat.Enemies;
using Core;
using UnityEngine;

namespace Combat.Spells
{


    public class SpellObj : MonoBehaviour
    {
        Animator animator;
        MagicType spellType;
        MagicType magicType;
        SelectableObject target;
        MagicAffinityTable magicAffinityTable;
        readonly HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
        string targetTag;
        bool hitPlayer;
        readonly HashSet<AllySlime> hitAllies = new HashSet<AllySlime>();
        const string AllyTag = "Ally";
        bool initialized;
        static int activeSpells;
        public static bool HasActiveSpells => activeSpells > 0;

        public void InitSpell(
            MagicType spellType,
            MagicType magicType,
            SelectableObject target,
            MagicAffinityTable magicAffinityTable
            )
        {

            this.magicAffinityTable = magicAffinityTable;
            this.spellType = spellType;
            this.magicType = magicType;
            this.target = target;
            targetTag = target.gameObject.tag;
            initialized = true;
            FriendlyElementalStatus.ConsumeAttack(Player.PlayerInt());
            activeSpells++;

            if (this.spellType == MagicType.Explode)
            {
                StartCoroutine(DestoryCounter());
                return;
            }

            if (this.spellType == MagicType.Drop)
            {
                moveVector = new Vector3(0, -1 * speed, 0);
            } else if (this.spellType == MagicType.Shoot)
            {
                moveVector = new Vector3(speed, 0, 0);
                // 아군은 몸 위치로, 거대 나무형 보스는 큰 충돌체의 중심으로 조준한다.
                if (targetTag == AllyTag || target.GetComponent<TreeShellBossEnemy>() != null)
                {
                    Vector3 toTarget = targetTag == AllyTag ? target.transform.position : target.GetComponent<Collider2D>().bounds.center;
                    toTarget -= transform.position;
                    toTarget.z = 0;
                    moveVector = toTarget.normalized * speed;
                }
            }

            if (this.spellType == MagicType.Shoot || this.spellType == MagicType.Drop)
            {
                StartCoroutine(ShootAction());
            }
        }
        float maxTime = 5;


        // 0.01초 간격으로 0.01초 분량만 움직이는 루프였다. 0.01초는 프레임 간격보다
        // 짧아 실제로는 한 프레임에 한 번 돌면서 이동량은 프레임 시간이 아닌 0.01을
        // 썼다. 그래서 발사체가 speed(초당 10)보다 느리게 날고, maxTime 5초도 60fps에서
        // 8초가 넘게 걸렸다. 느린 기기일수록 더 느려진다.
        IEnumerator ShootAction()
        {
            for (float elapsed = 0; elapsed < maxTime; elapsed += Time.deltaTime)
            {
                transform.position += moveVector * Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
        float speed = 10;
        Vector3 moveVector;
        int damage;

        // PlayerPrefs 는 MonoBehaviour 생성자(필드 초기화)에서 읽을 수 없다. InitProjectileDamage 는 Awake 뒤에 불린다.
        void Awake()
        {
            damage = CurrentPlayerDamage;
        }

        public void InitProjectileDamage(int damage)
        {
            this.damage = damage;
        }

        void Start()
        {

            animator = GetComponent<Animator>();
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 폭발은 누구를 골랐든 범위에 닿은 적과 아군을 모두 맞힌다. 신성 폭발은 양쪽을 모두 회복한다.
            if (initialized && spellType == MagicType.Explode)
            {
                if (collision.CompareTag("Enemy")) HitEnemy(collision.GetComponentInParent<Enemy>());
                else if (collision.CompareTag(AllyTag)) HitAlly(collision.GetComponentInParent<AllySlime>());
                return;
            }
            if (initialized && targetTag == AllyTag && collision.CompareTag(AllyTag))
            {
                AllySlime ally = collision.GetComponentInParent<AllySlime>();
                if (ally == null || ally.gameObject != target.gameObject) return;
                moveVector = Vector3.zero;
                if (HitAlly(ally)) StartCoroutine(DestoryCounter());
                return;
            }
            if (initialized && collision.CompareTag(targetTag))
            {
                Enemy enemy = collision.GetComponentInParent<Enemy>();
                if (collision.CompareTag("Enemy"))
                {
                    if (enemy == null || !enemy.IsAlive || !hitEnemies.Add(enemy)) return;
                }
                else
                {
                    if (hitPlayer) return;
                    hitPlayer = true;
                }
                moveVector = Vector3.zero;
                if (animator != null) animator.SetTrigger("Hit");
                if (collision.CompareTag("Enemy"))
                {
                    ApplyToEnemy(enemy);
                } else
                {
                    collision.GetComponent<Player>().TakeHit(CalculateDamage(damage, MagicType.Holy));
                }

                StartCoroutine(DestoryCounter());
            }
        }
        void HitEnemy(Enemy enemy)
        {
            if (enemy == null || !enemy.IsAlive || !hitEnemies.Add(enemy)) return;
            if (animator != null) animator.SetTrigger("Hit");
            ApplyToEnemy(enemy);
        }

        void ApplyToEnemy(Enemy enemy)
        {
            enemy.TakeSpellHit(magicType, spellType, GetBaseDamage(damage, spellType),
                magicAffinityTable.GetAffinity(magicType, enemy.enemyType));
        }

        // 신성은 아군을 회복하고(최대 HP까지), 나머지 속성은 아군에게 실제 피해를 준다.
        // 아군에는 상성표가 없으므로 상성 1배를 쓴다. 한 주문은 같은 아군에게 한 번만 들어간다.
        bool HitAlly(AllySlime ally)
        {
            if (ally == null || !ally.IsAlive || !hitAllies.Add(ally)) return false;
            if (animator != null) animator.SetTrigger("Hit");
            int amount = Mathf.FloorToInt(GetScaledDamage(damage, spellType));
            if (magicType == MagicType.Holy) ally.Heal(amount);
            else ally.TakeSpellDamage(amount);
            return true;
        }

        IEnumerator DestoryCounter()
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (initialized) activeSpells = Mathf.Max(0, activeSpells - 1);
        }

        private int CalculateDamage(int damage, MagicType enemyMagicType)
        {
            return (int)(GetScaledDamage(damage, spellType)
                * magicAffinityTable.GetAffinity(magicType, enemyMagicType));
        }

        static float GetScaledDamage(int damage, MagicType spellType)
        {
            float result = damage;
            if (spellType == MagicType.Drop)
            {
                result *= 0.8f;
            } else if(spellType == MagicType.Explode)
            {
                result *= 0.67f;
            }

            return result;
        }

        public static float GetBaseDamage(int damage, MagicType spellType) => GetScaledDamage(damage, spellType);

        static int CurrentPlayerDamage => Mathf.FloorToInt(PlayerUpgrades.SpellBaseDamage
            * (Player.PlayerInt() != null ? FriendlyElementalStatus.AttackMultiplier(Player.PlayerInt()) : 1f));
        public static float GetCurrentBaseDamage(MagicType spellType) => GetBaseDamage(CurrentPlayerDamage, spellType);

    }

}
