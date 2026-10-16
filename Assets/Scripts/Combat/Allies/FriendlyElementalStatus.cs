using Battle.Turns;
using Cards;
using Combat.Enemies;
using UnityEngine;

namespace Combat.Allies
{
    // 정수의 모방 주문이 실제로 명중한 대상에만 붙는다. 적과 같은 원소 반응/지속 규칙을 재사용한다.
    public sealed class FriendlyElementalStatus : MonoBehaviour
    {
        public ElementalStatus Status { get; private set; }
        TurnBattleSystem turns;
        Player player;
        AllySlime ally;
        SpriteRenderer mark;
        public static float AttackMultiplier(Component target)
        {
            var status = target.GetComponent<FriendlyElementalStatus>();
            return status != null ? status.Status.AttackMultiplier : 1f;
        }
        public static void ConsumeAttack(Component target)
        {
            if (target == null) return;
            var receiver = target.GetComponent<FriendlyElementalStatus>();
            if (receiver != null) receiver.Status.GetAttackDamage(0);
        }
        public static void Hit(Component target, MagicType element, MagicType spell, int damage)
        {
            if (target == null) return;
            var receiver = target.GetComponent<FriendlyElementalStatus>();
            if (receiver == null) receiver = target.gameObject.AddComponent<FriendlyElementalStatus>();
            // 워드는 보스와 마찬가지로 빙결 대신 공격 약화를 받아 턴 입력을 빼앗기지 않는다.
            var result = receiver.Status.Hit(element, spell, damage, 1f, receiver.player != null);
            receiver.Damage(result.Damage + result.ExtraDamage);
            if (receiver != null) receiver.RefreshMark(element);
        }
        void Awake()
        {
            player = GetComponent<Player>();
            ally = GetComponent<AllySlime>();
            Status = new ElementalStatus(ally != null ? ally.Element : MagicType.Holy);
            turns = TurnBattleSystem.Instance;
            if (turns != null) turns.PlayerTurnEnded += EndTurn;
            var child = new GameObject("IncomingElementStatus");
            child.transform.SetParent(transform, false);
            mark = child.AddComponent<SpriteRenderer>();
            mark.sortingOrder = 57;
            mark.enabled = false;
        }
        void Damage(int damage)
        {
            if (ally != null) ally.TakeHit(damage);
            else if (player != null) player.TakeHit(damage);
        }
        public bool BeginAction() => Status.BeginEnemyTurn();
        void EndTurn()
        {
            if (ally != null && !ally.IsAlive) return;
            int burn = Status.EndEnemyTurn();
            if (burn > 0) Damage(burn);
            if (mark != null) mark.enabled = Status.BurnTurns > 0 || Status.Chill > 0 || Status.Frozen || Status.ShockTurns > 0 || Status.FractureTurns > 0 || Status.Weakened;
        }
        void RefreshMark(MagicType element)
        {
            Sprite card = EnemyMagicVisual.Catalog.Element(element);
            if (card == null) return;
            mark.sprite = BossIntent.CardGlyph(card);
            mark.enabled = true;
        }
        void LateUpdate()
        {
            if (!mark.enabled) return;
            mark.transform.position = EnemyMagicVisual.Center(this) + Vector3.up * .7f;
            Vector3 scale = transform.lossyScale;
            mark.transform.localScale = new Vector3(.55f / scale.x, .55f / scale.y, 1);
        }
        void OnDestroy()
        {
            if (turns != null) turns.PlayerTurnEnded -= EndTurn;
        }
    }
}
