using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Stage;
using WordVenture.Combat.UI;

namespace WordVenture.Combat.Enemies
{

    public class ProjectileAnimator : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        [SerializeField] List<Sprite> sprites = new List<Sprite>();
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
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.sprite = sprites[1];
                yield return new WaitForSeconds(0.1f);
            }
        }

    }

}
