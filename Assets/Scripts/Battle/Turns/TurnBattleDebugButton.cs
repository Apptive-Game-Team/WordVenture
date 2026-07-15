using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Battle.Turns;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;

namespace WordVenture.Battle.Turns
{
    public class TurnBattleDebugButton : MonoBehaviour
    {
        public void EndPlayerTurn()
        {
            TurnBattleSystem.Instance.ChangeTurn(TurnBattleSystem.EnemyTurn);
        }

        public void EndEnemyTurn()
        {
            TurnBattleSystem.Instance.ChangeTurn(TurnBattleSystem.PlayerTurn);
        }

        public void SpawnEnemy()
        {
            TurnBattleSystem.Instance.enemyManager.SpawnEnemies();
        }
    }

}

