using System.Collections;
using Cards;
using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    // 고정 위치 보스의 앞줄 공격. 뿌리나 마법이 실제 대상까지 도달한 뒤 피해를 준다.
    public sealed class EnemyFrontAttack : MonoBehaviour
    {
        Enemy owner;
        GameObject effect;
        static Sprite pixel, branch;
        public static bool IsPlayingAny => FindObjectOfType<EnemyFrontAttack>() != null;

        public static void Begin(Enemy owner, int damage, MagicType? element = null)
        {
            if (owner.GetComponent<EnemyFrontAttack>() != null) return;
            var attack = owner.gameObject.AddComponent<EnemyFrontAttack>();
            attack.owner = owner;
            attack.StartCoroutine(attack.Play(damage, element));
        }
        IEnumerator Play(int damage, MagicType? element)
        {
            Player player = Player.PlayerInt();
            if (player == null) { Destroy(this); yield break; }
            var front = AllyFormation.Current != null ? AllyFormation.Current.Front : null;
            Component target = front != null && front.transform.position.x <= owner.transform.position.x ? (Component)front : player;
            Vector3 from = EnemyMagicVisual.Center(owner);
            Vector3 destination = EnemyMagicVisual.Center(target);
            EnemyMagicVisual spell = null;
            SpriteRenderer[] roots = null;
            if (element.HasValue)
            {
                spell = EnemyMagicVisual.Spawn(MagicType.Shoot, element.Value, from);
                if (spell != null) { effect = spell.gameObject; effect.transform.localScale = new Vector3(-1, 1, 1); }
            }
            else
            {
                effect = new GameObject("AdvancingRoots");
                if (pixel == null)
                {
                    var texture = new Texture2D(1, 1) { filterMode = FilterMode.Point };
                    texture.SetPixel(0, 0, Color.white); texture.Apply(false, true);
                    pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 27);
                    string[] pattern = { ".......bb...", "......bgb...", ".....bggb...", ".....bgb....", "....bgb.....", "....bgb.....", "...bggb.....", "...bgb......", "..bggb......", "..bggb......", ".bbgggb.....", "bbbbbbbbbbbb" };
                    var bark = new Texture2D(12, 12, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    for (int y = 0; y < 12; y++) for (int x = 0; x < 12; x++)
                        bark.SetPixel(x, 11 - y, pattern[y][x] == 'b' ? new Color(.39f, .23f, .08f) : pattern[y][x] == 'g' ? new Color(.78f, .52f, .15f) : Color.clear);
                    bark.Apply(false, true);
                    branch = Sprite.Create(bark, new Rect(0, 0, 12, 12), new Vector2(.5f, 0), 27);
                }
                int count = Mathf.Max(4, Mathf.CeilToInt(Mathf.Abs(from.x - destination.x) / .3f));
                roots = new SpriteRenderer[count * 3];
                for (int i = 0; i < roots.Length; i++)
                {
                    var part = new GameObject("Root"); part.transform.SetParent(effect.transform);
                    roots[i] = part.AddComponent<SpriteRenderer>(); roots[i].sprite = i % 3 == 2 ? branch : pixel;
                    roots[i].sortingOrder = 55;
                }
            }
            const float travel = .6f;
            for (float time = 0; time < travel && owner.IsAlive; time += Time.deltaTime)
            {
                if (target != null) destination = EnemyMagicVisual.Center(target);
                float progress = Mathf.Clamp01(time / travel);
                if (spell != null) spell.transform.position = Vector3.Lerp(from, destination, progress);
                if (roots != null) for (int i = 0; i < roots.Length; i++)
                {
                    int section = i / 3;
                    float phase = section / (float)(roots.Length / 3 - 1);
                    float rise = Mathf.Clamp01((progress - phase * .78f) / .22f);
                    float x = Mathf.Lerp(from.x, destination.x, phase);
                    float ground = -3.35f + (section % 5 < 2 ? 1 : 0) / 27f;
                    roots[i].transform.position = new Vector3(x, ground, 0);
                    if (i % 3 == 2)
                    {
                        roots[i].enabled = section % 3 == 0;
                        roots[i].flipX = section % 2 == 0;
                        roots[i].transform.localScale = new Vector3(1.2f, rise * (1.1f + section % 5 * .23f), 1);
                    }
                    else
                    {
                        roots[i].transform.localScale = new Vector3(10, (i % 3 == 0 ? 4 : 2) * rise, 1);
                        roots[i].color = i % 3 == 0 ? new Color(.39f, .23f, .08f) : new Color(.78f, .52f, .15f);
                    }
                }
                yield return null;
            }
            if (owner.IsAlive && target != null && (!(target is AllySlime ally) || ally.IsAlive))
            {
                if (spell != null) { spell.transform.position = destination; spell.Impact(); }
                if (element.HasValue) FriendlyElementalStatus.Hit(target, element.Value, MagicType.Shoot, damage);
                else AllyFormation.HitFrontLine(damage, owner.transform.position.x);
                HitImpact.Play(HitStrength.Weak);
                EnemyMagicVisual.PlayImpactSound(destination, .35f);
            }
            yield return new WaitForSeconds(.2f);
            Destroy(this);
        }
        void Update() { if (owner != null && !owner.IsAlive) Destroy(this); }
        void OnDisable() { StopAllCoroutines(); if (effect != null) Destroy(effect); Destroy(this); }
        void OnDestroy() { if (effect != null) Destroy(effect); }
    }
}
