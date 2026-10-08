using System.Collections;
using Cards;
using Combat.Enemies;
using Core;
using Combat.UI;
using Combat.Spells;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Turns
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
            TurnBattleSystem.Instance.cardManager.AddCard("Spell");
            TurnBattleSystem.Instance.cardManager.AddCard("MagicType");
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

            //TurnBattleSystem.Instance.ChangeTurn(TurnBattleSystem.PlayerTurn);
            TurnBattleSystem.Instance.enemyManager.PlayTurn();
            TurnBattleSystem.Instance.StartEnemyTurnCounter();

        }

        public override void OnEnd() // When Enemy Action End...
        {

        }


    }




    public class TurnBattleSystem : MonoBehaviour
    {
        public static TurnBattleSystem Instance;

        public static PlayerTurn PlayerTurn;
        public static EnemyTurn EnemyTurn;
        public static float TurnTime = 1f;
        Turn currentTurn;
        public event System.Action PlayerTurnEnded;

        [SerializeField] public CardManager cardManager;
        [SerializeField] public EnemyTestManager enemyManager;
        [SerializeField] public EnemyPoolController enemyPoolController;
        [SerializeField] Button turnEndButton;


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

        private void Update()
        {
            // 주문이 준비되거나 날아가는 동안 턴을 넘길 수 없다는 것을 버튼 색으로 보여준다.
            if (turnEndButton != null)
            {
                turnEndButton.interactable = !IsSpellInProgress();
            }
        }

        static bool IsSpellInProgress()
        {
            CombineZone zone = CombineZone.Instance;
            if (zone != null && (zone.IsAwaitingTarget || zone.IsCasting)) return true;
            return SpellObj.HasActiveSpells;
        }

        public void ChangeTurn(Turn turn)
        {
            currentTurn.OnEnd();
            currentTurn = turn;
            currentTurn.OnStart();
        }

        public void TurnEndButton()
        {
            if (InteractionLock.IsLocked) return;
            if (IsSpellInProgress()) return;
            if (currentTurn == PlayerTurn)
            {
                ChangeTurn(EnemyTurn);
                PlayerTurnEnded?.Invoke();
            }

        }


        public void StartEnemyTurnCounter()
        {
            StartCoroutine(EnemyTurnCounter());
        }

        IEnumerator EnemyTurnCounter()
        {
            yield return new WaitForSecondsRealtime(1f);
            enemyManager.EndTurnStatuses();
            ChangeTurn(PlayerTurn);
        }

    }

}
