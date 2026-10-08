using UnityEngine;

namespace Combat.Enemies
{
    public class EnemyPoolController : MonoBehaviour
    {

        [SerializeField] EnemyDataContainer enemyDataContainer;

        // 적 id가 더 이상 스테이지 번호와 묶여 있지 않으므로, 미리 만들어 두지 않고 소환할 때 만든다.
        public GameObject SpawnObject(float positionX, float positionZ, int id)
        {
            EnemyData enemyData = enemyDataContainer.GetGearData(id);
            GameObject enemy = Instantiate(enemyData.prefab, new Vector3(positionX, -3, positionZ), Quaternion.identity);
            enemy.GetComponent<Enemy>().InitEnemyData(enemyData);
            return enemy;
        }
    }
}
