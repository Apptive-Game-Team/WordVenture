using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
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
        [SerializeField] public Deck_Manage.MagicType type;
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