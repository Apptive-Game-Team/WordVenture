using System.Collections;
using System.Collections.Generic;
using Battle.Turns;
using Combat.Stage;
using Core;
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
            StartCoroutine(BeginWave(wave));
        }

        IEnumerator BeginWave(int wave)
        {
            BattleWaveData battleWaveData = battleScript.GetBattleWaveDatas()[wave];
            for (int i = 0; i < battleWaveData.enemySpawnDatasInWave.Count; i++)
            {
                activatedEnemies.Add(ememyPool.SpawnObject(battleWaveData.enemySpawnDatasInWave[i].spawnPositionX, i , battleWaveData.enemySpawnDatasInWave[i].enemyId));
            }

            foreach (GameObject enemy in activatedEnemies)
            {
                if (enemy == null || !enemy.activeInHierarchy) continue;
                TreeShellBossEnemy boss = enemy.GetComponent<TreeShellBossEnemy>();
                if (boss != null)
                {
                    bool wasLocked = InteractionLock.IsLocked;
                    InteractionLock.IsLocked = true;
                    try
                    {
                        // 첫 프레임부터 착지 위치를 확정해 거대 슬라임 자체를 날려 보낸다.
                        Vector3 landing = boss.transform.position;
                        landing.y = TreeShellBossEnemy.GroundY;
                        boss.transform.position = landing;
                        yield return WorldTreeEntrance.Play(boss);
                    }
                    finally { InteractionLock.IsLocked = wasLocked; }
                }
            }
            StartCoroutine(WaveEndSensor());
        }

        public static void BeginWorldTreeTransition(TreeShellBossEnemy shell)
        {
            BattleWaveController controller = FindObjectOfType<BattleWaveController>();
            if (controller != null && controller.isActiveAndEnabled)
                controller.StartCoroutine(WorldTreeTransition.Play(shell));
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
                if (WorldTreeTransition.IsPlaying) continue;
                foreach (GameObject enemy in activatedEnemies)
                {
                    if (enemy != null && enemy.activeSelf)
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
                // 처음 깰 때만 stagePosition이 StagePosition과 같다. 이미 깬 스테이지를 다시 깨면 진행도를 올리지 않는다.
                StageDataSingleton stageData = StageDataSingleton.Instance;
                stageData.isFirstClear = stageData.stagePosition == MapMove.StagePosition;
                if (stageData.isFirstClear) MapMove.StagePosition++;
                // 엔딩으로 가는 마지막 지역도 여기서 지급한다. AddShards 가 PlayerPrefs.Save 까지 한다.
                int turns = TurnBattleSystem.Instance != null ? TurnBattleSystem.Instance.TurnNumber : 0;
                stageData.lastScore = StageReward.Score(stageData.stagePosition, turns, BattleStats.CardsUsed,
                    BattleStats.DamageTaken, BattleStats.Combos, PlayerUpgrades.PlayerMaxHp);
                stageData.lastReward = StageReward.Calculate(stageData.stagePosition, stageData.isFirstClear, stageData.lastScore.Grade);
                stageData.hasResult = true;
                PlayerUpgrades.AddShards(stageData.lastReward.Total);
                SceneManager.LoadScene("GameClearScene");
            }
        }

    }

}
