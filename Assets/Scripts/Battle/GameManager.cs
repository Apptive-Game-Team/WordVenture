using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Battle.Turns;

namespace WordVenture.Battle
{

    public class GameManager : MonoBehaviour
    {
        private TurnBattleSystem battleSystem;

        void Start()
        {
            battleSystem = FindObjectOfType<TurnBattleSystem>();

            if (battleSystem == null)
            {
                Debug.LogError("TurnBattleSystem not found in the scene.");
            }
            else
            {
                Debug.Log("Battle Start!");
            }
        }
    }
}
