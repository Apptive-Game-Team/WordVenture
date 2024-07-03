using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class EnemyPoolController : MonoBehaviour
    {

        [SerializeField] EnemyDataContainer enemyDataContainer;

        [SerializeField] protected List<List<GameObject>> enemyPools = new List<List<GameObject>>();
        
        int stagePosition = StageDataSingleton.Instance.StagePosition;

        private void Start()
        {
            InitPool();
        }

        private void InitPool()
        {
            for (int i = 0; i < enemyDataContainer.GetGearNum(); i++)
            {
                EnemyData enemyData = enemyDataContainer.GetGearData(i);
                enemyPools.Add(new List<GameObject>());
                MakeObjects(enemyData, 5, enemyPools[i]);

            }
        }

        public void AddToPool(GameObject Object, int id)
        {
            enemyPools[id].Add(Object);
        }

        protected void MakeObjects(EnemyData enemyData, int num, List<GameObject> pool)
        {
            
            for (int i = 0; i < num; i++)
            {
                if(enemyData.id/3 == stagePosition || (stagePosition==4 && enemyData.id>11))
                {
                    GameObject temp = Instantiate(enemyData.prefab, new Vector3(10, 10, 0), Quaternion.identity);
                    temp.GetComponent<Enemy>().InitEnemyData(enemyData);
                    pool.Add(temp);
                    pool[i].SetActive(false);
                }
                
            }
        }


        public GameObject SpawnObject(float positionX, float positionZ, int id)
        {
            if (enemyPools[id].Count > 0)
            {
                GameObject bady = enemyPools[id][0];
                bady.SetActive(true);
                enemyPools[id].RemoveAt(0);

                bady.GetComponent<Transform>().position = new Vector3(positionX, -3, positionZ);
                return bady;
            }
            else
            {
                return null;
            }
        }
    }
}

