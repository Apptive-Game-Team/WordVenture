using System.Collections.Generic;
using Combat.Allies;
using Core;
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

#if UNITY_EDITOR
        // 테스트용: 에디터에서 K 키로 지금 나와 있는 적을 모두 쓰러뜨린다. 빌드에는 들어가지 않는다.
        void Update()
        {
            // 대화나 튜토리얼이 입력을 잡고 있을 때는 무시한다.
            if (InteractionLock.IsLocked || !Input.GetKeyDown(KeyCode.K)) return;
            KillAllEnemies();
        }

        public void KillAllEnemies()
        {
            foreach (Enemy enemy in FindObjectsOfType<Enemy>()) enemy.Kill();
        }
#endif

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
                // 앞 적의 공격으로 아군 슬라임이 쓰러질 수 있으므로 적마다 기준선을 다시 구한다.
                float frontLineX = AllyFormation.FrontLineX(player.transform.position.x);
                enemy.PlayTurnAction(enemy.transform.position.x - frontLineX);
            }
        }

        // 워드에 가장 가까운 살아 있는 적. 아군 슬라임이 공격할 대상이다.
        public Enemy FindFrontEnemy()
        {
            Enemy front = null;
            foreach (GameObject enemyObject in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                Enemy enemy = enemyObject.GetComponent<Enemy>();
                if (enemy == null || !enemy.IsAlive) continue;
                if (front == null || enemy.transform.position.x < front.transform.position.x) front = enemy;
            }
            return front;
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


