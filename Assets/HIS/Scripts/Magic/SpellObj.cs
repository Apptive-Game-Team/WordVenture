using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SpellObj : MonoBehaviour
{
    Animator animator;
    Deck_Manage.MagicType spellType;
    Deck_Manage.MagicType magicType;
    SelectableObject target;
    MagicAAffinity.MagicAffinityTable magicAffinityTable;

    public void InitSpell(
        Deck_Manage.MagicType spellType,
        Deck_Manage.MagicType magicType,
        SelectableObject target,
        MagicAAffinity.MagicAffinityTable magicAffinityTable
        )
    {

        this.magicAffinityTable = magicAffinityTable;
        this.spellType = spellType;
        this.magicType = magicType;
        this.target = target;

        if (this.spellType == Deck_Manage.MagicType.Summon)
        {
            StartCoroutine(DestoryCounter());
            return;
        }

        if (this.spellType == Deck_Manage.MagicType.Drop)
        {
            moveVector = new Vector3(0, -1 * speed, 0);
        } else if (this.spellType == Deck_Manage.MagicType.Shoot)
        {
            moveVector = new Vector3(speed, 0, 0);
        }

        if (this.spellType == Deck_Manage.MagicType.Shoot || this.spellType == Deck_Manage.MagicType.Drop)
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
    int damage = 10 + 5 * (Map_scene.MapMove.StagePosition / 2);

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
                collision.GetComponent<Enemy.Enemy>().TakeHit(CalculateDamage(damage, collision.gameObject.GetComponent<Enemy.Enemy>().enemyType));
            } else
            {
                collision.GetComponent<Enemy.Player>().TakeHit(CalculateDamage(damage, Deck_Manage.MagicType.Holy));
            }
            
            StartCoroutine(DestoryCounter());
        }
    }
    IEnumerator DestoryCounter()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private int CalculateDamage(int damage, Deck_Manage.MagicType enemyMagicType)
    {
        float result = damage;
        if (spellType == Deck_Manage.MagicType.Drop)
        {
            result *= 0.7f;
        } else if(spellType == Deck_Manage.MagicType.Summon)
        {
            result *= 0.5f;
        }

        result *= magicAffinityTable.GetAffinity(magicType, enemyMagicType);

        return ((int)result);
    }

}
