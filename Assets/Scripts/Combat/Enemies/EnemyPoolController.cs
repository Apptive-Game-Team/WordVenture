using System.Collections.Generic;
using Combat.Stage;
using UnityEngine;

namespace Combat.Enemies
{
    public class EnemyPoolController : MonoBehaviour
    {

        [SerializeField] EnemyDataContainer enemyDataContainer;

        [SerializeField] protected List<List<GameObject>> EnemyPools = new List<List<GameObject>>();

        int stagePosition = StageDataSingleton.Instance.stagePosition;

        private void Start()
        {
            InitPool();
        }

        private void InitPool()
        {
            for (int i = 0; i < enemyDataContainer.GetGearNum(); i++)
            {
                EnemyData enemyData = enemyDataContainer.GetGearData(i);
                EnemyPools.Add(new List<GameObject>());
                MakeObjects(enemyData, 5, EnemyPools[i]);

            }
        }

        public void AddToPool(GameObject @object, int id)
        {
            EnemyPools[id].Add(@object);
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
            if (EnemyPools[id].Count > 0)
            {
                GameObject bady = EnemyPools[id][0];
                bady.SetActive(true);
                EnemyPools[id].RemoveAt(0);

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

