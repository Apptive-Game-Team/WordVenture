using System.Collections;
using Cards;
using Combat.Allies;
using Combat.Enemies;
using Core;
using Combat.UI;
using Combat.Spells;
using Story;
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
            TurnBattleSystem.Instance.BeginPlayerTurn();
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
        bool endingPlayerTurn;
        public bool IsEndingPlayerTurn => endingPlayerTurn;
        public bool IsPlayerTurn => currentTurn == PlayerTurn;
        public event System.Action PlayerTurnEnded;
        // 전투의 몇 번째 플레이어 턴인지. 첫 턴이 1이다. 적 공격력 상승과 빠른 클리어 보상이 이 값을 쓴다.
        public int TurnNumber { get; private set; }
        public event System.Action<int> PlayerTurnStarted;

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
            BattleStats.Reset();
            currentTurn = PlayerTurn;
            currentTurn.OnStart();
        }

        private void Update()
        {
            // 주문이 준비되거나 날아가는 동안, 그리고 턴 종료 후 아군 슬라임이 공격하는 동안
            // 턴을 넘길 수 없다는 것을 버튼 색으로 보여준다.
            if (turnEndButton != null)
            {
                turnEndButton.interactable = !InteractionLock.IsLocked && !WorldTreeTransition.IsPlaying && !IsSpellInProgress() && !endingPlayerTurn;
            }
        }

        static bool IsSpellInProgress()
        {
            CombineZone zone = CombineZone.Instance;
            if (zone != null && (zone.IsAwaitingTarget || zone.IsCasting)) return true;
            return SpellObj.HasActiveSpells;
        }

        public void BeginPlayerTurn()
        {
            TurnNumber++;
            PlayerTurnStarted?.Invoke(TurnNumber);
            // 분노 1단계가 되는 첫 턴에 한 번만 설명한다. 카드 뽑기가 끝난 뒤에 뜨도록 한 프레임 미룬다.
            if (EnemyRage.LevelFromTurn(TurnNumber) >= 1 && !GameTips.HasSeen(GameTip.EnemyRage))
                StartCoroutine(ShowEnemyRageTip());
        }

        IEnumerator ShowEnemyRageTip()
        {
            yield return null;
            yield return GameTips.ShowOnce(GameTip.EnemyRage);
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
            if (WorldTreeTransition.IsPlaying) return;
            if (IsSpellInProgress()) return;
            if (currentTurn == PlayerTurn && !endingPlayerTurn)
            {
                StartCoroutine(EndPlayerTurn());
            }

        }

        // 아군 슬라임은 플레이어 턴과 적 턴 사이에 공격한다. 아군이 없으면 기다리지 않고 바로 적 턴이 된다.
        IEnumerator EndPlayerTurn()
        {
            endingPlayerTurn = true;
            AllyFormation formation = AllyFormation.Current;
            if (formation != null && formation.Allies.Count > 0)
                yield return formation.AttackFrontEnemies(enemyManager.FindFrontEnemy);
            while (WorldTreeTransition.IsPlaying) yield return null;
            endingPlayerTurn = false;
            ChangeTurn(EnemyTurn);
            PlayerTurnEnded?.Invoke();
        }


        public void StartEnemyTurnCounter()
        {
            StartCoroutine(EnemyTurnCounter());
        }

        IEnumerator EnemyTurnCounter()
        {
            yield return new WaitForSecondsRealtime(1f);
            while (WorldTreeTransition.IsPlaying) yield return null;
            while (SurgeBossEnemy.IsCastingAny || FurnaceBossEnemy.IsReleasingAny || GraftBossEnemy.IsAbsorbingAny || EnemyFrontAttack.IsPlayingAny) yield return null;
            enemyManager.EndTurnStatuses();
            // 화상으로 외피가 쓰러지면 상태 처리 자체가 페이즈 전환을 시작한다.
            while (WorldTreeTransition.IsPlaying) yield return null;
            ChangeTurn(PlayerTurn);
        }

    }

}
