using System;
using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Combat.Enemies
{
    [Serializable]
    public struct EnemyData
    {
        [SerializeField] public int id;
        [SerializeField] public string name;
        // 도감에 보이는 한국어 이름. 비워 두면 도감에 싣지 않는다.
        [SerializeField] public string displayName;
        [SerializeField] public GameObject prefab;
        [SerializeField] public int maxHp;
        [SerializeField] public float moveDistance;
        [SerializeField] public float attackRange;
        [SerializeField] public int damage;
        [SerializeField] public MagicType type;
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

        public IReadOnlyList<EnemyData> All => enemyDatas;

        public int GetGearNum()
        {
            return enemyDatas.Count;
        }
    }
}