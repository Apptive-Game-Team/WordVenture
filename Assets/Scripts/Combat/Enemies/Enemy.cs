using System.Collections;
using System.Collections.Generic;
using Battle.Turns;
using Cards;
using TMPro;
using UnityEngine;

namespace Combat.Enemies
{
    public abstract class EnemyAction
    {
        protected Enemy Enemy;
        protected EnemyAction(Enemy enemy)
        {
            this.Enemy = enemy;
        }
        public abstract void PlayAction(float distanceToPlayer);

    }

    public class EnemyAttackAction : EnemyAction
    {

        public EnemyAttackAction(Enemy enemy) : base(enemy) { }

        public override void PlayAction(float distanceToPlayer)
        {
            Enemy.Attack(distanceToPlayer);
        }
    }

    public class EnemyMoveAction : EnemyAction
    {

        public EnemyMoveAction(Enemy enemy) : base(enemy) { }
        public override void PlayAction(float distanceToPlayer)
        {

            float tempMoveDistance;

            if (distanceToPlayer > Enemy.moveDistance + Enemy.attackRange)
            {
                tempMoveDistance = Enemy.moveDistance;
            }
            else
            {
                tempMoveDistance = distanceToPlayer - Enemy.attackRange;
            }

            Enemy.StartCoroutine(Enemy.MoveDistance(tempMoveDistance));
        }
    }


    public enum ActionType
    {
        ATTACK = 0, MOVE = 1
    }


    public class Enemy : MonoBehaviour
    {
        protected SlimeAnimator Animator;

        protected TMP_Text HpText;

        public MagicType enemyType;

        [SerializeField] protected int id;
        protected int Hp = 1;
        protected int MaxHp = 1;
        protected int Damage;
        public ElementalStatus Status { get; private set; } = new ElementalStatus();
        public bool IsAlive => Hp > 0 && gameObject.activeInHierarchy;
        protected int AttackDamage => Status.GetAttackDamage(Damage);
        TMP_Text statusText;
        SpriteRenderer bodyRenderer;
        Color originalBodyColor;
        bool tookTurn;
        string reactionText;
        float reactionUntil;

        public float moveDistance = 5;

        public float attackRange = 3;

        private Vector3 tempVector3 = new Vector3();
        float turnTime;

        [SerializeField] private List<EnemyAction> enemyActions = new List<EnemyAction>();

        public void InitEnemyData(EnemyData enemyData)
        {
            id = enemyData.id;
            MaxHp = enemyData.maxHp;
            Hp = MaxHp;
            moveDistance = enemyData.moveDistance;
            attackRange = enemyData.attackRange;
            Damage = enemyData.damage;
            enemyType = enemyData.type;
            Status = new ElementalStatus(enemyType);
            reactionText = string.Empty;
            tookTurn = false;
            UpdateIndicator();
        }

        private void InitEnemyActions()
        {
            enemyActions.Add(new EnemyAttackAction(this));
            enemyActions.Add(new EnemyMoveAction(this));
        }



        private ActionType MakeActionDecision(float distanceToPlayer)
        {

            if (distanceToPlayer > attackRange)
            {
                return ActionType.MOVE;
            } else
            {
                return ActionType.ATTACK;
            }
        }

        public void UpdateIndicator()
        {
            if (HpText != null) HpText.SetText(Mathf.Max(0, Hp).ToString());
            UpdateStatusIndicator();
        }

        void UpdateStatusIndicator()
        {
            if (statusText != null)
                statusText.text = Time.time < reactionUntil ? reactionText : string.Empty;
            if (bodyRenderer != null)
            {
                Color tint = Status.Frozen || Status.Chill > 0 ? new Color(0.5f, 0.85f, 1f)
                    : Status.BurnTurns > 0 ? new Color(1f, 0.6f, 0.35f)
                    : Status.ShockTurns > 0 ? new Color(1f, 1f, 0.5f)
                    : Status.FractureTurns > 0 ? new Color(0.8f, 0.65f, 0.5f) : Color.white;
                bodyRenderer.color = originalBodyColor * tint;
            }
        }

        void Update()
        {
            if (!string.IsNullOrEmpty(reactionText) && Time.time >= reactionUntil)
            {
                reactionText = string.Empty;
                UpdateStatusIndicator();
            }
        }

        public void TakeSpellHit(MagicType element, MagicType spell, float baseDamage, float affinity)
        {
            if (!IsAlive) return;
            ElementalStatus.HitResult hit = Status.Hit(element, spell, baseDamage, affinity, this is BossEnemy);
            reactionText = hit.Reaction;
            reactionUntil = Time.time + 1.5f;
            TakeHit(hit.Damage + hit.ExtraDamage);
            UpdateStatusIndicator();
        }

        public void EndTurnStatuses()
        {
            if (!tookTurn) return;
            tookTurn = false;
            if (!IsAlive) return;
            int burn = Status.EndEnemyTurn();
            if (burn > 0) TakeHit(burn);
            UpdateStatusIndicator();
        }



        public void PlayTurnAction(float distanceToPlayer)
        {
            if (!IsAlive) return;
            tookTurn = true;
            if (Status.BeginEnemyTurn())
            {
                UpdateStatusIndicator();
                return;
            }
            enemyActions[(int) MakeActionDecision(distanceToPlayer)].PlayAction(distanceToPlayer);
        }


        // moveSpeed는 turnTime 안에 moveDistance를 지나도록 정해진다. 그런데 대기는
        // 0.01초를 요청하면서 실제 프레임은 그보다 길고, 이동량은 프레임 시간이 아닌
        // 0.01을 썼다. 그래서 60fps에서 적이 의도한 속도의 60% 정도로 움직이고 이동이
        // turnTime을 넘겨, 1초 뒤 턴을 넘기는 TurnBattleSystem의 타이머를 침범했다.
        public IEnumerator MoveDistance(float distance)
        {
            distance = Mathf.Max(0, distance) * Status.MovementMultiplier;
            Animator.MoveStart();
            float moveSpeed = moveDistance / turnTime;
            float movedDistance = 0;
            while (movedDistance < distance && IsAlive)
            {
                yield return null;

                float moveStep = Mathf.Min(moveSpeed * Time.deltaTime, distance - movedDistance);
                movedDistance += moveStep;
                Move(-1, moveStep);
            }
            if (IsAlive) StopMove();
        }

        private void Awake()
        {
            InitIndicators();
            InitEnemyActions();
            bodyRenderer = GetComponent<SpriteRenderer>();
            if (bodyRenderer != null) originalBodyColor = bodyRenderer.color;
            gameObject.AddComponent<ElementalStatusVfx>().Initialize(this, bodyRenderer);
        }

        protected virtual void Start()
        {
            Animator = GetComponent<SlimeAnimator>();
            turnTime = TurnBattleSystem.TurnTime;
        }



        protected void FaceToDirection(int direction)
        {
            if (direction > 0)
            {
                direction = 1;
            } else if (direction < 0)
            {
                direction = -1;
            } else
            {
                return;
            }
            tempVector3 = transform.localScale;
            tempVector3.x = Mathf.Abs(tempVector3.x) * direction;
            transform.localScale = tempVector3;
        }

        public void Move(int direction, float moveStep) {
            FaceToDirection(direction);
            tempVector3 = transform.position;
            tempVector3.x = tempVector3.x + moveStep * direction;
            transform.position = tempVector3;

        }

        protected virtual void StopMove()
        {
            Animator.MoveEnd();
        }

        virtual public void Attack(float distanceToPlayer)
        {
            Animator.Attack();
        }

        protected void Death()
        {
            Animator.Death();
            StartCoroutine(DeathCounter());
        }
        IEnumerator DeathCounter()
        {
            yield return new WaitForSeconds(0.25f);
            gameObject.SetActive(false);
        }

        // 남은 체력만큼 피해를 줘서 평소와 같은 사망 처리로 쓰러뜨린다. 이미 쓰러진 적은 건드리지 않는다.
        public void Kill()
        {
            if (Hp > 0 && gameObject.activeInHierarchy) TakeHit(Hp);
        }

        public void TakeHit(int damage)
        {
            if (!IsAlive) return;
            Hp -= damage;
            UpdateIndicator();
            if (Hp <= 0)
            {
                Death();
                return;
            }
            else {
                Animator.TakeHit();
                UpdateIndicator();
            }

        }

        private void InitIndicators()
        {
            HpText = gameObject.GetComponentInChildren<TMP_Text>();
            if (HpText == null) return;
            HpText.SetText(MaxHp.ToString());
            statusText = Instantiate(HpText, HpText.transform.parent);
            statusText.name = "ElementalStatusText";
            var presentation = Resources.Load<ElementalStatusPresentation>("Combat/ElementalStatusPresentation");
            if (presentation != null && presentation.font != null) statusText.font = presentation.font;
            statusText.raycastTarget = false;
            statusText.richText = true;
            statusText.enableAutoSizing = false;
            statusText.fontSize = HpText.fontSize * 0.7f;
            statusText.enableWordWrapping = false;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.rectTransform.anchoredPosition += new Vector2(0, HpText.rectTransform.rect.height * 0.8f);
            statusText.rectTransform.sizeDelta = new Vector2(HpText.rectTransform.rect.width * 6f,
                HpText.rectTransform.rect.height * 2f);
            statusText.text = string.Empty;
        }

    }
}


