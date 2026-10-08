using System.Collections;
using System.Collections.Generic;
using Combat.Stage;
using Map;
using Story;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Combat.Enemies
{
    public class BattleWaveController : MonoBehaviour
    {
        public BattleScriptContainer battleScript;
        EnemyPoolController ememyPool;
        List<GameObject> activatedEnemies = new List<GameObject>();
        int wave = 0;


        public void Start1()
        {
            ememyPool = gameObject.GetComponent<EnemyPoolController>();
            StartWave(wave);
        }

        private void StartWave(int wave)
        {
            BattleWaveData battleWaveData = battleScript.GetBattleWaveDatas()[wave];
            for (int i = 0; i < battleWaveData.enemySpawnDatasInWave.Count; i++)
            {
                activatedEnemies.Add(ememyPool.SpawnObject(battleWaveData.enemySpawnDatasInWave[i].spawnPositionX, i , battleWaveData.enemySpawnDatasInWave[i].enemyId));
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
            {
                // 마왕 슬라임 등장처럼 wave 앞에 정해 둔 대화가 있으면 먼저 보여 준다.
                StageDialogueChapter chapter = StageDialogueView.FindUnseen(StageDataSingleton.Instance.stagePosition,
                    StageDialogueMoment.Wave, wave, out StageDialogueData data);
                if (chapter != null) yield return StageDialogueView.Play(data, chapter);
                StartWave(wave);
            }
            else
            {
                MapMove.StagePosition++;
                SceneManager.LoadScene("GameClearScene");
            }
        }

    }

}
