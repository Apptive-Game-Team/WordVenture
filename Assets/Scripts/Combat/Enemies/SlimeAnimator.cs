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

        // 돌진·폭발 슬라임만 대기 8장 뒤에 예고 프레임 2장을 더 가진다.
        const int WindupFrame = 8;
        bool windingUp;

        // 소환된 프레임에 바로 공격이나 피격 애니메이션을 부를 수 있으므로 참조는 Awake에서 잡는다.
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
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

        // 예고하는 동안에는 대기 대신 예고 프레임 2장을 번갈아 보여 준다. 피격 애니메이션이 끝나도 예고로 돌아온다.
        public void Windup()
        {
            if (sprites.Count < WindupFrame + 2) return;
            windingUp = true;
            StopAllCoroutines();
            StartCoroutine(Idling());
        }

        public void EndWindup()
        {
            windingUp = false;
        }

        public void Death()
        {
            windingUp = false;
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
                if (windingUp)
                {
                    spriteRenderer.sprite = sprites[WindupFrame];
                    yield return ShortFrameHold;
                    spriteRenderer.sprite = sprites[WindupFrame + 1];
                    yield return ShortFrameHold;
                    continue;
                }
                spriteRenderer.sprite = sprites[0];
                yield return LongFrameHold;
                spriteRenderer.sprite = sprites[1];
                yield return LongFrameHold;
            }
        }

    }

}
