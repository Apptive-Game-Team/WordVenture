using System.Collections;
using UnityEngine;

namespace Combat.Enemies
{
    // 주문에 맞은 적을 하얗게 번쩍이고 워드 반대쪽으로 잠깐 밀었다가 되돌린다. 세기는 HitImpact.Classify가 정한다.
    // SpriteRenderer.color로는 스프라이트를 밝힐 수 없어서 Resources/Combat/HitFlash.mat으로 몸 모양을 흰색으로 덧그린다.
    public sealed class HitFlashVfx : MonoBehaviour
    {
        // ReactionBurstVfx(+10)보다 아래에 그린다.
        const int SortingOffset = 5;
        const float NormalFlashSeconds = 0.1f;
        const float StrongFlashSeconds = 0.14f;
        const float ResistedFlashSeconds = 0.06f;
        const float ResistedFlashAlpha = 0.5f;
        const float KnockbackSeconds = 0.12f;
        // 밀리는 거리는 월드 단위다.
        const float ResistedKnockback = 0.05f;
        const float NormalKnockback = 0.12f;
        const float WeakKnockback = 0.25f;
        const float ReactionKnockback = 0.3f;

        static Material flashMaterial;
        SpriteRenderer body;
        SpriteRenderer flash;
        Coroutine flashing;
        Coroutine knockback;
        // 지금 transform에 더해 둔 x 이동량. Enemy.MoveDistance도 위치를 조금씩 옮기므로 더한 만큼만 뺀다.
        float knockbackOffset;

        public HitStrength? LastStrength { get; private set; }

        public void Initialize(SpriteRenderer renderer)
        {
            body = renderer;
            if (body == null) { enabled = false; return; }
            if (flashMaterial == null) flashMaterial = Resources.Load<Material>("Combat/HitFlash");
            // 재질이 없으면 번쩍임만 끄고 밀림은 그대로 쓴다.
            if (flashMaterial == null) return;
            var child = new GameObject("HitFlash");
            child.transform.SetParent(transform, false);
            flash = child.AddComponent<SpriteRenderer>();
            flash.sharedMaterial = flashMaterial;
            flash.enabled = false;
        }

        public void Play(HitStrength strength)
        {
            LastStrength = strength;
            if (strength == HitStrength.None) return;
            if (!enabled || body == null || !gameObject.activeInHierarchy) return;
            if (flash != null)
            {
                if (flashing != null) StopCoroutine(flashing);
                flashing = StartCoroutine(Flashing(strength));
            }
            if (knockback != null) StopCoroutine(knockback);
            knockback = StartCoroutine(Knockback(GetKnockbackDistance(strength)));
        }

        static float GetFlashSeconds(HitStrength strength)
        {
            switch (strength)
            {
                case HitStrength.Resisted: return ResistedFlashSeconds;
                case HitStrength.Weak:
                case HitStrength.Reaction: return StrongFlashSeconds;
                default: return NormalFlashSeconds;
            }
        }

        static float GetKnockbackDistance(HitStrength strength)
        {
            switch (strength)
            {
                case HitStrength.Resisted: return ResistedKnockback;
                case HitStrength.Normal: return NormalKnockback;
                case HitStrength.Weak: return WeakKnockback;
                case HitStrength.Reaction: return ReactionKnockback;
                default: return 0f;
            }
        }

        // 히트 스톱(timeScale 0) 중에도 보이도록 unscaled 시간으로 사라진다.
        IEnumerator Flashing(HitStrength strength)
        {
            float seconds = GetFlashSeconds(strength);
            float startAlpha = strength == HitStrength.Resisted ? ResistedFlashAlpha : 1f;
            float startTime = Time.unscaledTime;
            flash.enabled = true;
            MirrorBody();
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                flash.color = new Color(1f, 1f, 1f, startAlpha * (1f - elapsed / seconds));
                yield return null;
                elapsed = Time.unscaledTime - startTime;
            }
            flash.enabled = false;
            flashing = null;
        }

        // 피격 애니메이션이 같은 프레임에 스프라이트를 바꾸므로 그리기 직전에 몸을 따라간다.
        void LateUpdate()
        {
            if (flash != null && flash.enabled) MirrorBody();
        }

        void MirrorBody()
        {
            flash.sprite = body.sprite;
            flash.flipX = body.flipX;
            flash.flipY = body.flipY;
            flash.sortingLayerID = body.sortingLayerID;
            flash.sortingOrder = body.sortingOrder + SortingOffset;
        }

        // 적은 오른쪽에서 왼쪽으로 걸어오므로 +x가 워드 반대쪽이다. 히트 스톱 동안에는 멈춰 있다가 풀리면 밀린다.
        IEnumerator Knockback(float distance)
        {
            // 앞선 밀림이 남아 있으면 그 자리에서 이어 가서 튀지 않게 한다.
            float startOffset = knockbackOffset;
            float elapsed = 0f;
            while (elapsed < KnockbackSeconds)
            {
                yield return null;
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / KnockbackSeconds);
                SetKnockbackOffset(startOffset * (1f - progress) + distance * Mathf.Sin(Mathf.PI * progress));
            }
            SetKnockbackOffset(0f);
            knockback = null;
        }

        void SetKnockbackOffset(float offset)
        {
            transform.position += new Vector3(offset - knockbackOffset, 0f, 0f);
            knockbackOffset = offset;
        }

        void OnDisable()
        {
            // 쓰러져 풀로 돌아간 적이 번쩍임이나 밀린 자리를 들고 다시 나오지 않게 한다.
            if (flash != null) flash.enabled = false;
            flashing = null;
            knockback = null;
            SetKnockbackOffset(0f);
        }
    }
}
