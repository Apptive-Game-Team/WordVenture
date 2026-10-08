using System.Collections;
using System.Collections.Generic;
using Cards;
using Combat.Enemies;
using Map;
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
        int damage = 10 + 5 * (MapMove.StagePosition / 2);

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
                    enemy.TakeSpellHit(magicType, spellType, GetBaseDamage(damage, spellType),
                        magicAffinityTable.GetAffinity(magicType, enemy.enemyType));
                } else
                {
                    collision.GetComponent<Player>().TakeHit(CalculateDamage(damage, MagicType.Holy));
                }

                StartCoroutine(DestoryCounter());
            }
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

        public static float GetCurrentBaseDamage(MagicType spellType) => GetBaseDamage(10 + 5 * (MapMove.StagePosition / 2), spellType);

    }

}
