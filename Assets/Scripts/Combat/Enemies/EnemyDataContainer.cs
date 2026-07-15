using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Stage;
using WordVenture.Combat.UI;

namespace WordVenture.Combat.Enemies
{
    [Serializable]
    public struct EnemyData
    {
        [SerializeField] public int id;
        [SerializeField] public string name;
        [SerializeField] public GameObject prefab;
        [SerializeField] public int maxHp;
        [SerializeField] public float moveDistance;
        [SerializeField] public float attackRange;
        [SerializeField] public int damage;
        [SerializeField] public WordVenture.Cards.MagicType type;
    }


    [CreateAssetMenu]
    public class EnemyDataContainer : ScriptableObject
    {

        [SerializeField] private List<EnemyData> enemyDatas = new List<EnemyData>();

        public EnemyData GetGearData(int n)
        {
            if (n == -1)
            {
                return new EnemyData();
            }

            return enemyDatas[n];
        }

        public int GetGearNum()
        {
            return enemyDatas.Count;
        }
    }
}