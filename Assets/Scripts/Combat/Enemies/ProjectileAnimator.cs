using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat.Enemies
{

    public class ProjectileAnimator : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        [SerializeField] List<Sprite> sprites = new List<Sprite>();

        // Idle은 발사체가 살아 있는 내내 돈다. 살아 있는 발사체 수만큼 대기 객체가
        // 계속 새로 만들어지지 않도록 공유한다.
        static readonly WaitForSeconds FrameHold = new WaitForSeconds(0.1f);

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            StartCoroutine(Idle());
        }

        IEnumerator Idle()
        {
            while (true)
            {
                spriteRenderer.sprite = sprites[0];
                yield return FrameHold;
                spriteRenderer.sprite = sprites[1];
                yield return FrameHold;
            }
        }

    }

}
