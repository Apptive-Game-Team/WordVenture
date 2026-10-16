using Cards;
using Combat.Spells;
using UnityEngine;

namespace Combat.Enemies
{
    public sealed class EnemyMagicVisual : MonoBehaviour
    {
        static EnemyMagicCatalog catalog;
        SpriteRenderer body;
        Animator animationController;
        public static EnemyMagicCatalog Catalog => catalog != null ? catalog : catalog = Resources.Load<EnemyMagicCatalog>("Combat/EnemyMagicCatalog");
        public static Vector3 Center(Component target)
        {
            var renderer = target.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null) return target.transform.position;
            Bounds bounds = ElementalStatusVfx.GetVisibleBodyBounds(renderer.sprite);
            Vector3 center = bounds.center;
            if (renderer.flipX) center.x = -center.x;
            return renderer.transform.TransformPoint(center);
        }
        public static EnemyMagicVisual Spawn(MagicType spell, MagicType element, Vector3 center, float size = 1f)
        {
            GameObject prefab = Catalog.Prefab(spell, element);
            if (prefab == null) return null;
            var root = new GameObject("EnemyMagic." + element + "." + spell);
            root.transform.position = center;
            var view = root.AddComponent<EnemyMagicVisual>();
            GameObject visual = Instantiate(prefab, root.transform);
            foreach (var hit in visual.GetComponentsInChildren<Collider2D>()) hit.enabled = false;
            foreach (var logic in visual.GetComponentsInChildren<SpellObj>()) { logic.enabled = false; Destroy(logic); }
            view.body = visual.GetComponent<SpriteRenderer>();
            view.animationController = visual.GetComponent<Animator>();
            visual.transform.localScale *= size;
            view.body.sortingOrder = 65;
            view.LateUpdate();
            return view;
        }
        public void Impact()
        {
            if (animationController != null) animationController.SetTrigger("Hit");
            Destroy(gameObject, .5f);
        }
        public static void PlayImpactSound(Vector3 position, float volume = .5f)
        {
            if (Catalog.impactClip == null) return;
            var holder = new GameObject("EnemyMagicImpactSound");
            holder.transform.position = position;
            var source = holder.AddComponent<AudioSource>();
            source.spatialBlend = 0;
            source.clip = Catalog.impactClip;
            source.volume = volume;
            source.Play();
            Destroy(holder, source.clip.length + .1f);
        }
        void LateUpdate()
        {
            if (body == null || body.sprite == null) return;
            // 마법 그림의 바닥 피벗이나 프레임별 여백과 관계없이 실제 그림을 중심에 맞춘다.
            body.transform.localPosition = Vector3.zero;
            Vector3 center = body.transform.TransformPoint(Catalog.VisibleBounds(body.sprite).center);
            body.transform.position += transform.position - center;
        }
    }
}
