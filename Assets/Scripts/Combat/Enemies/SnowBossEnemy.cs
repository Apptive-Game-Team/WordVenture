using Core;
using TMPro;
using UnityEngine;

namespace Combat.Enemies
{
    public class SnowBossEnemy : BossEnemy
    {
        SnowballEnemy snowball;
        TMP_Text tell;
        public SnowballEnemy Snowball => snowball != null && snowball.IsAlive ? snowball : null;
        public override int SummonedEnemyId => 34;
        // 수치를 바꾸면 English.json 도 고친다.
        public override string AttackDescription => "눈덩이를 만들고 3단계까지 키운 다음 턴에 던진다. 눈덩이를 파괴하면 크기에 따라 본체가 8, 16, 28 피해를 받는다.";

        protected override void Start()
        {
            base.Start();
            RegionalBossTell.Show(ref tell, transform, HpText, "눈덩이 준비", new Color(0.65f, 0.9f, 1f));
        }
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (Snowball == null)
            {
                GameObject created = BattleWaveController.SpawnExtraEnemy(SummonedEnemyId, transform.position.x - 2f);
                if (created != null)
                {
                    snowball = created.GetComponent<SnowballEnemy>();
                    snowball.Initialize(this);
                }
                Animator.RangeAttack();
            }
            else if (snowball.Growth < 3)
            {
                snowball.Grow();
                Animator.RangeAttack();
            }
            else
            {
                snowball.Throw(AttackDamage);
                snowball = null;
                Animator.RangeAttack();

            }
            RegionalBossTell.Show(ref tell, transform, HpText,
                Snowball == null ? "눈덩이 준비" : snowball.Growth == 3 ? "다음 턴 눈덩이 투척" : "눈덩이 성장", new Color(0.65f, 0.9f, 1f));
        }

        protected override void Death()
        {
            if (snowball != null && snowball.IsAlive) snowball.Dismiss();
            snowball = null;
            base.Death();
        }
        void OnDisable()
        {
            if (snowball != null && snowball.IsAlive) snowball.Dismiss();
            snowball = null;
            RegionalBossTell.Hide(tell);
        }
        protected override void StopMove() => Animator.MoveEnd();
    }

    // 보스의 다음 행동만 짧게 표시한다. HP와 같은 폰트와 월드 좌표계를 쓴다.
    internal static class RegionalBossTell
    {
        public static void Show(ref TMP_Text label, Transform owner, TMP_Text hp, string message, Color color)
        {
            if (label == null)
            {
                GameObject go = new GameObject("BossIntent");
                go.transform.SetParent(owner, false);
                go.transform.localPosition = new Vector3(0, 3.1f, 0);
                var text = go.AddComponent<TextMeshPro>();
                if (hp != null) text.font = hp.font;
                text.fontSize = 3.5f;
                text.alignment = TextAlignmentOptions.Center;
                text.rectTransform.sizeDelta = new Vector2(12, 1.5f);
                text.GetComponent<MeshRenderer>().sortingOrder = 55;
                go.AddComponent<RegionalBossIntentAnchor>().Refresh();
                label = text;
            }

            label.color = color;
            label.text = Localization.Translate(message);
            label.gameObject.SetActive(true);
        }
        public static void Hide(TMP_Text label)
        {
            if (label != null) label.gameObject.SetActive(false);
        }
    }
    // 보스의 방향 전환과 분노 확대 후에도 예고는 읽을 수 있는 크기와 방향을 유지한다.
    internal sealed class RegionalBossIntentAnchor : MonoBehaviour
    {
        void LateUpdate() => Refresh();
        public void Refresh()
        {
            Transform owner = transform.parent;
            if (owner == null) return;
            var animator = owner.GetComponent<SlimeAnimator>();
            float top = animator != null && animator.FirstFrame != null
                ? owner.TransformPoint(new Vector3(0, animator.FirstFrame.bounds.max.y, 0)).y
                : owner.position.y + 1.8f;
            transform.position = new Vector3(owner.position.x, top + .9f, owner.position.z);
            transform.rotation = Quaternion.identity;
            Vector3 scale = owner.lossyScale;
            transform.localScale = new Vector3(
                Mathf.Abs(scale.x) > 0.001f ? 0.8f / scale.x : 1f,
                Mathf.Abs(scale.y) > 0.001f ? 0.8f / scale.y : 1f, 1);
        }
    }
}
