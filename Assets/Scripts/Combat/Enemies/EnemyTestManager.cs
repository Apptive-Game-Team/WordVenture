using System.Collections.Generic;
using UnityEngine;

namespace Combat.Enemies
{
    public class EnemyTestManager : MonoBehaviour
    {

        [SerializeField] GameObject player;

        EnemyPoolController enemyPoolController;

        List<Enemy> enemies = new List<Enemy>();
        void Start()
        {
            InitList(enemies);
            enemyPoolController = gameObject.GetComponent<EnemyPoolController>();

        }

        private void InitList(List<Enemy> enemies)
        {
            enemies.Clear();
            GameObject[] temp = GameObject.FindGameObjectsWithTag("Enemy");
            if (temp == null) return;
            for(int i = 0; i < temp.Length; i++)
            {
                enemies.Add(temp[i].GetComponent<Enemy>());
            }
        }

        public void PlayTurn()
        {
            InitList(enemies);
            foreach(Enemy enemy in enemies)
            {
                enemy.PlayTurnAction(enemy.transform.position.x - player.transform.position.x);
            }
        }

        public void SpawnEnemies()
        {
            enemyPoolController.SpawnObject(0, 1, 0);
            enemyPoolController.SpawnObject(4, 2, 1);
            enemyPoolController.SpawnObject(8, 3, 2);
            InitList(enemies);
        }

        public void EndTurnStatuses()
        {
            // 턴 시작 시의 목록을 사용해 도중에 나타난 다음 웨이브의 상태를 소비하지 않는다.
            foreach (Enemy enemy in enemies)
                if (enemy != null) enemy.EndTurnStatuses();
        }

        //MSP
        public List<Enemy> GetEnemies()
        {
            return enemies;
        }
    }
}


