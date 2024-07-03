using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> sprites = new List<Sprite>();
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(Idling());
    }


    public void MoveStart()
    {
        StopAllCoroutines();
        StartCoroutine(Moving());
    }

    public void MoveEnd()
    {
        StopAllCoroutines();
        StartCoroutine(Idling());
    }

    public void Attack()
    {
        StopAllCoroutines();
        StartCoroutine(Attacking());
    }

    public void Death()
    {
        StopAllCoroutines();
        spriteRenderer.sprite = sprites[5];
    }

    public void TakeHit()
    {
        StopAllCoroutines();
        StartCoroutine(TakeHitting());
    }

    public void RangeAttack()
    {
        StopAllCoroutines();
        StartCoroutine(RangeAttacking());
    }

    IEnumerator RangeAttacking()
    {
        spriteRenderer.sprite = sprites[6];
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.sprite = sprites[7];
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(Idling());
    }

    IEnumerator TakeHitting()
    {
        spriteRenderer.sprite = sprites[4];
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(Idling());
    }

    IEnumerator Attacking()
    {
        spriteRenderer.sprite = sprites[2];
        yield return new WaitForSeconds(0.25f);
        spriteRenderer.sprite = sprites[3];
        yield return new WaitForSeconds(0.15f);
        StartCoroutine(Idling());
    }

    IEnumerator Moving()
    {
        while (true)
        {
            spriteRenderer.sprite = sprites[2];
            yield return new WaitForSeconds(0.25f);
            spriteRenderer.sprite = sprites[3];
            yield return new WaitForSeconds(0.15f);
        }
    }



    IEnumerator Idling()
    {
        while (true)
        {
            spriteRenderer.sprite = sprites[0];
            yield return new WaitForSeconds(0.25f);
            spriteRenderer.sprite = sprites[1];
            yield return new WaitForSeconds(0.25f);
        }
    }

}
