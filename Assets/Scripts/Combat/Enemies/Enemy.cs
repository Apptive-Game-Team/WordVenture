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
            HpText.SetText(Hp.ToString());
        }



        public void PlayTurnAction(float distanceToPlayer)
        {
            enemyActions[(int) MakeActionDecision(distanceToPlayer)].PlayAction(distanceToPlayer);
        }


        public IEnumerator MoveDistance(float distance)
        {
            Animator.MoveStart();
            float moveSpeed = moveDistance / turnTime;
            float movedDistance = 0;
            while (movedDistance <= distance)
            {

                yield return new WaitForSeconds(0.01f);
                movedDistance += moveSpeed * 0.01f;
                Move(-1, moveSpeed * 0.01f);
            }
            StopMove();
        }

        private void Awake()
        {
            InitIndicators();
            InitEnemyActions();
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

        public void TakeHit(int damage)
        {
            Hp -= damage;
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
            HpText.SetText(MaxHp.ToString());
        }

    }
}


