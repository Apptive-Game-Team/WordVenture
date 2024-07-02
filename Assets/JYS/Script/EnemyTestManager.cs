using System.Collections;
using System.Collections.Generic;
using TurnBattle;
using UnityEngine;

namespace Enemy
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
            enemyPoolController.SpawnObject(0, 0);
            enemyPoolController.SpawnObject(4, 1);
            enemyPoolController.SpawnObject(8, 2);
            InitList(enemies);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayTurn();
            } else if (Input.GetKeyDown(KeyCode.Return))
            {
                SpawnEnemies();
            } else if (Input.GetKeyDown(KeyCode.D))
            {
                foreach (Enemy enemy in enemies)
                {
                    enemy.TakeHit(1);
                }
            }
        }

        //MSP
        public List<Enemy> GetEnemies()
        {
            return enemies;
        }
    }
}


