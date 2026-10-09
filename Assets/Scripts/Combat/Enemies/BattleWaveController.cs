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

        // 분열 슬라임과 최종 보스가 웨이브 도중에 부르는 적. 이 적까지 쓰러뜨려야 웨이브가 끝난다.
        public static GameObject SpawnExtraEnemy(int enemyId, float positionX)
        {
            BattleWaveController controller = FindObjectOfType<BattleWaveController>();
            if (controller == null || controller.ememyPool == null) return null;
            GameObject enemy = controller.ememyPool.SpawnObject(positionX, controller.activatedEnemies.Count, enemyId);
            controller.activatedEnemies.Add(enemy);
            return enemy;
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
