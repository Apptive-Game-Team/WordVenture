using System.Collections;
using System.Collections.Generic;
using TurnBattle;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BattleWaveController : MonoBehaviour
{
    public BattleScriptContainer battleScript;
    Enemy.EnemyPoolController ememyPool;
    List<GameObject> activatedEnemies = new List<GameObject>();
    int wave = 0;


    public void Start1()
    {
        ememyPool = gameObject.GetComponent<Enemy.EnemyPoolController>();
        StartWave(wave);
    }

    private void StartWave(int wave)
    {
        BattleWaveData battleWaveData = battleScript.GetBattleWaveDatas()[wave];
        print(battleWaveData.enemySpawnDatasInWave.Count);
        for (int i = 0; i < battleWaveData.enemySpawnDatasInWave.Count; i++)
        {
            print(i);
            activatedEnemies.Add(ememyPool.SpawnObject(battleWaveData.enemySpawnDatasInWave[i].SpawnPositionX, battleWaveData.enemySpawnDatasInWave[i].EnemyId));
        }

        StartCoroutine(WaveEndSensor());
    }

    IEnumerator WaveEndSensor()
    {
        
        while (true)
        {
            bool waveEnd = true;
            //if (activatedEnemies.Count == 0)
            //    break;
            yield return new WaitForSeconds(0.1f);
            foreach (GameObject enemy in activatedEnemies)
            {
                if (enemy.activeSelf)//enemy != null)
                {
                    waveEnd = false;
                }
            }
            if (waveEnd)
            {
                break;
            }
        }
        yield return new WaitForSeconds(1f);

        wave += 1;
        if (wave < battleScript.GetBattleWaveDatas().Count)
            StartWave(wave);
        else
        {
            Map_scene.MapMove.StagePosition++;
            SceneManager.LoadScene("GameClearScene");
        }
    }

}
