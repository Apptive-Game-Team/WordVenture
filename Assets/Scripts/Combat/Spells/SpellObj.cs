using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;

namespace WordVenture.Combat.Spells
{


    public class SpellObj : MonoBehaviour
    {
        Animator animator;
        WordVenture.Cards.MagicType spellType;
        WordVenture.Cards.MagicType magicType;
        SelectableObject target;
        WordVenture.Combat.MagicAffinityTable magicAffinityTable;

        public void InitSpell(
            WordVenture.Cards.MagicType spellType,
            WordVenture.Cards.MagicType magicType,
            SelectableObject target,
            WordVenture.Combat.MagicAffinityTable magicAffinityTable
            )
        {

            this.magicAffinityTable = magicAffinityTable;
            this.spellType = spellType;
            this.magicType = magicType;
            this.target = target;

            if (this.spellType == WordVenture.Cards.MagicType.Summon)
            {
                StartCoroutine(DestoryCounter());
                return;
            }

            if (this.spellType == WordVenture.Cards.MagicType.Drop)
            {
                moveVector = new Vector3(0, -1 * speed, 0);
            } else if (this.spellType == WordVenture.Cards.MagicType.Shoot)
            {
                moveVector = new Vector3(speed, 0, 0);
            }

            if (this.spellType == WordVenture.Cards.MagicType.Shoot || this.spellType == WordVenture.Cards.MagicType.Drop)
            {
                StartCoroutine(ShootAction());
            }
        }
        float maxTime = 5;


        IEnumerator ShootAction()
        {
            for (float i = 0; i < maxTime;)
            {
                i += 0.01f;
                transform.position += moveVector * 0.01f;
                yield return new WaitForSeconds(0.01f);

            }

            Destroy(gameObject);
        }
        float speed = 10;
        Vector3 moveVector;
        int damage = 10 + 5 * (WordVenture.Map.MapMove.StagePosition / 2);

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
            if (collision.CompareTag(target.gameObject.tag))
            {
                moveVector = Vector3.zero;
                print(collision.gameObject.tag);
                animator.SetTrigger("Hit");
                if (collision.CompareTag("Enemy"))
                {
                    collision.GetComponent<WordVenture.Combat.Enemies.Enemy>().TakeHit(CalculateDamage(damage, collision.gameObject.GetComponent<WordVenture.Combat.Enemies.Enemy>().enemyType));
                } else
                {
                    collision.GetComponent<WordVenture.Combat.Enemies.Player>().TakeHit(CalculateDamage(damage, WordVenture.Cards.MagicType.Holy));
                }

                StartCoroutine(DestoryCounter());
            }
        }
        IEnumerator DestoryCounter()
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }

        private int CalculateDamage(int damage, WordVenture.Cards.MagicType enemyMagicType)
        {
            float result = damage;
            if (spellType == WordVenture.Cards.MagicType.Drop)
            {
                result *= 0.8f;
            } else if(spellType == WordVenture.Cards.MagicType.Summon)
            {
                result *= 0.67f;
            }

            result *= magicAffinityTable.GetAffinity(magicType, enemyMagicType);

            return ((int)result);
        }

    }

}
