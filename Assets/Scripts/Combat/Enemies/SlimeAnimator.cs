using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat.Enemies
{

    public class SlimeAnimator : MonoBehaviour
    {
        [SerializeField] List<Sprite> sprites = new List<Sprite>();
        SpriteRenderer spriteRenderer;

        // Idling과 Moving은 끝없이 돌기 때문에 대기 객체를 그때그때 만들면 살아 있는
        // 적 수만큼 GC 쓰레기가 계속 쌓인다. WaitForSeconds는 남은 시간을 들고 있지
        // 않아 인스턴스를 공유해도 안전하다.
        static readonly WaitForSeconds LongFrameHold = new WaitForSeconds(0.25f);
        static readonly WaitForSeconds ShortFrameHold = new WaitForSeconds(0.15f);

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
            yield return ShortFrameHold;
            spriteRenderer.sprite = sprites[7];
            yield return LongFrameHold;
            StartCoroutine(Idling());
        }

        IEnumerator TakeHitting()
        {
            spriteRenderer.sprite = sprites[4];
            yield return LongFrameHold;
            StartCoroutine(Idling());
        }

        IEnumerator Attacking()
        {
            spriteRenderer.sprite = sprites[2];
            yield return LongFrameHold;
            spriteRenderer.sprite = sprites[3];
            yield return ShortFrameHold;
            StartCoroutine(Idling());
        }

        IEnumerator Moving()
        {
            while (true)
            {
                spriteRenderer.sprite = sprites[2];
                yield return LongFrameHold;
                spriteRenderer.sprite = sprites[3];
                yield return ShortFrameHold;
            }
        }



        IEnumerator Idling()
        {
            while (true)
            {
                spriteRenderer.sprite = sprites[0];
                yield return LongFrameHold;
                spriteRenderer.sprite = sprites[1];
                yield return LongFrameHold;
            }
        }

    }

}
