using Deck_Manage;
using Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TurnBattle
{

    public enum TurnStatus 
    {
        NONE,
        PLAYER,
        ENEMY,
    }

    public interface ITurn
    {
        void OnStart();
    }


    public abstract class Turn : ITurn
    {
        TurnStatus status;

        public abstract void OnStart();
        public abstract void OnEnd();
    }

    public class PlayerTurn : Turn
    {
        public override void OnStart() // When Enemy Turn End...
        {
            //Debug.Log("Player Turn Start!");
            //Draw Cards.
            TurnBattleSystem.Instance.cardManager.AddCard();
        }
        public override void OnEnd() //When Player Hit End button...
        {
            //Debug.Log("Player Turn End!");
        }

    }
    public class EnemyTurn : Turn
    {
        public override void OnStart() // When Player Turn End...
        {
            //Debug.Log("Enemy Turn Start!");
            TurnBattleSystem.Instance.enemyManager.PlayTurn();
            //TurnBattleSystem.Instance.ChangeTurn(TurnBattleSystem.PlayerTurn);
            OnEnd();
        }

        public override void OnEnd() // When Enemy Action End...
        {
            //Debug.Log("Enemy Turn End!");
        }
    }


    public class TurnBattleSystem : MonoBehaviour
    {
        public static TurnBattleSystem Instance;

        public static PlayerTurn PlayerTurn;
        public static EnemyTurn EnemyTurn;

        Turn currentTurn;

        [SerializeField] public CardManager cardManager;
        [SerializeField] public EnemyTestManager enemyManager;
        [SerializeField] public EnemyPoolController enemyPoolController;


        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }

            PlayerTurn = new PlayerTurn();
            EnemyTurn = new EnemyTurn();
        }

        private void Start()
        {
            currentTurn = PlayerTurn;
            currentTurn.OnStart();
        }

        public void ChangeTurn(Turn turn)
        {
            currentTurn.OnEnd();
            currentTurn = turn;
            currentTurn.OnStart();
        }

    }

}

